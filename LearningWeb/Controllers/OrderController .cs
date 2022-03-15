using Learning.DataAccess.Repository.IRepository;
using Learning.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Get(string? status=null)
        {
            var orderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            if (status == "cancelled")
            {
                orderHeaderList = orderHeaderList.Where(x => x.Status == SD.StatusCancelled || x.Status == SD.StatusRejected);
            }
            else if (status == "completed")
            {
                orderHeaderList = orderHeaderList.Where(x => x.Status == SD.StatusCompleted);
            }
            else if (status == "ready")
            {
                orderHeaderList = orderHeaderList.Where(x => x.Status == SD.StatusReady);
            }
            else
            {
                orderHeaderList = orderHeaderList.Where(x => x.Status == SD.StatusSubmitted || x.Status == SD.StatusInProcess);
            }
            return Json(new { data = orderHeaderList });
        }

    }
}
