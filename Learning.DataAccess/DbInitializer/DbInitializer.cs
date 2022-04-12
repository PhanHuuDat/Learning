using Learning.DataAccess.Data;
using Learning.Models;
using Learning.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

            }

            if (!_roleManager.RoleExistsAsync(SD.KitchenRole).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.KitchenRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.ManagerRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.FrontDeskRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.CustomerRole)).GetAwaiter().GetResult();
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@project.com",
                    Email = "admin@project.com",
                    EmailConfirmed = true,
                    FirstName = "Phan",
                    LastName = "Dat"
                }, "Admin123*").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUser.FirstOrDefault(u => u.Email == "admin@project.com");

                _userManager.AddToRoleAsync(user, SD.ManagerRole).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
