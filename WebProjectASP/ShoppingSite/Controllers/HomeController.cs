using ShoppingSite.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShoppingSite.Controllers {
	public class HomeController : Controller {
		private ApplicationDbContext db = new ApplicationDbContext();

		private async Task<Boolean> FillViewBag() {
			ViewBag.AllCategories = await db.Categories.ToListAsync();
			ViewBag.AllBrands = await db.Brands.ToListAsync();
			ViewBag.AllActiveSales = await db.GetActiveSalesAsync();
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
			ViewBag.Message = "Your application description page.";

			return View();
		}

		[HttpGet]
		public async Task<ActionResult> Contact() {
			ViewBag.Message = "Your contact page.";
			await this.FillViewBag();
			return View();
		}

		[HttpGet]
		public async Task<ActionResult> PrivacyPolicy() {
			ViewBag.Message = "Your contact page.";
			await this.FillViewBag();
			return View();
		}

	}
}