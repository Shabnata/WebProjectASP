using ShoppingSite.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ShoppingSite.Controllers {
	public class HomeController : Controller {
		private ApplicationDbContext db = new ApplicationDbContext();

		private async Task<Boolean> FillViewBag() {
			ViewBag.AllCategories = await db.Categories.ToListAsync();
			ViewBag.AllBrands = await db.Brands.ToListAsync();
			ViewBag.AllActiveSales = await db.GetActiveSalesAsync();
            //----
            bool hasPermission = false;
            if (User.Identity.IsAuthenticated) { // User logged in
                ApplicationUser user = db.Users.Find(User.Identity.GetUserId());

                bool Administrator = false; //1
                bool Manager = false; //2
                bool Employee = false; //3

                foreach (IdentityUserRole iur in user.Roles) {
                    if (iur.RoleId.Equals("1")) {
                        Administrator = true;
                        break;
                    }
                    if (iur.RoleId.Equals("2")) {
                        Manager = true;
                        break;
                    }
                    if (iur.RoleId.Equals("3")) {
                        Employee = true;
                        break;
                    }
                }
                if (Administrator || Manager || Employee) {
                    hasPermission = true;
                }
            }
            ViewBag.hasPermission = hasPermission;
            //----
            return true;
		}

		[HttpGet]
		public async Task<ActionResult> Index() {
			await this.FillViewBag();
			return View();
		}

		[HttpGet]
		public async Task<ActionResult> About() {
			await this.FillViewBag();
			ViewBag.Message = "Online Sup-Shop";

			return View();
		}

		[HttpGet]
		public async Task<ActionResult> Contact() {
			ViewBag.Message = "Contact page";
			await this.FillViewBag();
			return View();
		}

		[HttpGet]
		public async Task<ActionResult> PrivacyPolicy() {
			ViewBag.Message = "Contact page";
			await this.FillViewBag();
			return View();
		}

	}
}