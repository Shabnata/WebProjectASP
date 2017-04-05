using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ShoppingSite.Models;
using System.Data.SqlClient;

namespace ShoppingSite.Controllers {
	[Authorize(Roles = "Administrator, Manager")]
	public class BrandsController : Controller {

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
			return View(await db.Brands.ToListAsync());
		}

		[HttpGet]
		public async Task<ActionResult> Create() {
			await this.FillViewBag();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create([Bind(Include = "BrandID, BrandName, Logo, Country, Description")] BrandModel brandModel, int FoundationYear) {
			brandModel.FoundationYear = new DateTime(FoundationYear, 0, 1);
			if(ModelState.IsValid) {
				if(!await (from b in db.Brands where b.BrandName.ToLower() == brandModel.BrandName.ToLower() select b).AnyAsync()) {
					db.Brands.Add(brandModel);
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}
			await this.FillViewBag();
			return View("Error");
		}

		[HttpGet]
		public async Task<ActionResult> Edit(int? BrandID) {
			await this.FillViewBag();
			if(BrandID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			BrandModel brandModel = await db.Brands.FindAsync(BrandID);
			if(brandModel == null) {
				return HttpNotFound();
			}
			return View(brandModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit([Bind(Include = "BrandID, BrandName, Logo, Country, Description, FoundationYear")] BrandModel brandModel) {
			await this.FillViewBag();
			if(ModelState.IsValid) {
				if(!await (from b in db.Brands where b.BrandID != brandModel.BrandID && b.BrandName.ToLower() == brandModel.BrandName.ToLower() select b).AnyAsync()) {
					db.Entry(brandModel).State = EntityState.Modified;
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}
			return View(brandModel);
		}

		[HttpGet]
		public async Task<ActionResult> Details(int? BrandID) {
			await this.FillViewBag();
			if(BrandID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			BrandModel brandModel = await db.Brands.FindAsync(BrandID);
			if(brandModel == null) {
				return HttpNotFound();
			}
			return View(brandModel);
		}

		[HttpGet]
		public async Task<ActionResult> Delete(int? BrandID) {
			await this.FillViewBag();
			if(BrandID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			BrandModel brandModel = await db.Brands.FindAsync(BrandID);
			if(brandModel == null) {
				return HttpNotFound();
			}
			return View(brandModel);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DeleteConfirmed(int BrandID) {
			BrandModel brandModel = await db.Brands.FindAsync(BrandID);
			db.Brands.Remove(brandModel);
			await db.SaveChangesAsync();
			return RedirectToAction("Index");
		}

		protected override void Dispose(bool disposing) {
			if(disposing) {
				db.Dispose();
			}
			base.Dispose(disposing);
		}

		[AllowAnonymous]
		[HttpPost, ActionName("Search")]
		public async Task<ActionResult> Search(string BrandName) {

			IList<BrandModel> brands = await (from b in db.Brands where b.BrandName.ToLower().Contains(BrandName.ToLower()) select b).ToListAsync();

			await this.FillViewBag();
			ViewBag.SearchString = BrandName;
			if(brands.Count == 0) {
				ViewBag.NotFoundError = "Brand not found";
			} else if(brands.Count == 1 && brands.First().BrandName.ToLower().Equals(BrandName.ToLower())) {
				return RedirectToAction("Details", new { BrandID = brands.First().BrandID });
			}

			return View("Index", brands);
		}

		[AllowAnonymous]
		[HttpPost]
		public async Task<ActionResult> TypeSearch(string SearchString) {
			if(SearchString == null || SearchString.Equals("")) {
				return Json("");
			}
			ICollection<string> brands = await (from b in db.Brands where b.BrandName.ToLower().StartsWith(SearchString.ToLower()) select b.BrandName).ToArrayAsync();
			return Json(brands);
		}

		[HttpPost]
		public async Task<ActionResult> CheckAvailability(int ID, string Name) {
			Boolean available = await (from b in db.Brands where b.BrandID != ID && b.BrandName.ToLower().Equals(Name.ToLower()) select b).AnyAsync();
			return Json(!available);
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult> BrowseByID(int BrandID) {

			IList<SubCategoryModel> subCat = null;
			try {
				subCat = await db.GetBrandSubCategoriesAsync(BrandID);
			} catch(SqlException ex) {

			}
			await this.FillViewBag();
			return View("Browse", subCat);
		}
	}
}
