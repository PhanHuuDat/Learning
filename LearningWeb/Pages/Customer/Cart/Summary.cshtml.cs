using Learning.DataAccess.Repository.IRepository;
using Learning.Models;
using Learning.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe.Checkout;
using System.Security.Claims;

namespace LearningWeb.Pages.Customer.Cart
{
    [Authorize]
    [BindProperties]
    public class SummaryModel : PageModel
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public OrderHeader OrderHeader { get; set; }

        private readonly IUnitOfWork _unitOfWork;

        public SummaryModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            OrderHeader = new OrderHeader();
        }
        public void OnGet()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                    filter: u => u.ApplicationUserId == claim.Value,
                    includeProperties: "MenuItem,MenuItem.FoodType,MenuItem.Category");

                foreach (var cart in ShoppingCartList)
                {
                    OrderHeader.OrderTotal += cart.MenuItem.Price * cart.Count;
                }
                OrderHeader.OrderTotal = double.Parse(string.Format("{0:0.##}", OrderHeader.OrderTotal));
                ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
                OrderHeader.PickupName = applicationUser.FirstName + " " + applicationUser.LastName;
                OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            }
        }

        public IActionResult OnPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                    filter: u => u.ApplicationUserId == claim.Value,
                    includeProperties: "MenuItem,MenuItem.FoodType,MenuItem.Category");

                foreach (var cart in ShoppingCartList)
                {
                    OrderHeader.OrderTotal += cart.MenuItem.Price * cart.Count;
                }
                OrderHeader.Status = SD.StatusPending;
                OrderHeader.OrderDate = DateTime.Now;
                OrderHeader.UserId = claim.Value;
                OrderHeader.PickUpDate = Convert.ToDateTime(OrderHeader.PickUpDate.ToShortDateString() + " " + OrderHeader.PickUpTime.ToShortTimeString());
                _unitOfWork.OrderHeader.Add(OrderHeader);
                _unitOfWork.Save();

                foreach (var item in ShoppingCartList)
                {
                    OrderDetails orderDetails = new()
                    {
                        MenuItemId = item.MenuItemId,
                        OrderId = OrderHeader.Id,
                        Name = item.MenuItem.Name,
                        Price = item.MenuItem.Price,
                        Count = item.Count
                    };
                    _unitOfWork.OrderDetails.Add(orderDetails);
                }
                
                //_unitOfWork.ShoppingCart.RemoveRange(ShoppingCartList);
                _unitOfWork.Save();

                var domain = "https://localhost:44355";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>(),

                    PaymentMethodTypes = new List<String>
                    {
                        "card",
                    },

                    Mode = "payment",
                    SuccessUrl = domain + $"/customer/cart/OrderConfirmation?id={OrderHeader.Id}",
                    CancelUrl = domain + "/customer/cart/Index",
                };
                //addline item
                foreach (var item in ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            // 1.11->111
                            UnitAmount = (long)(item.MenuItem.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.MenuItem.Name
                            },
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new Stripe.Checkout.SessionService();
                Session session = service.Create(options);

                Response.Headers.Add("Location", session.Url);
                OrderHeader.SessionId = session.Id;
                OrderHeader.PaymentIntentId = session.PaymentIntentId;
                _unitOfWork.Save();
                return new StatusCodeResult(303);
            }
            return Page();
        }
    }
}
