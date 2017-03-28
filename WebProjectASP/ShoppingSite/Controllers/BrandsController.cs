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
        private async Task<Boolean> FillViewBag()
        {
            ViewBag.AllCategories = await db.Categories.ToListAsync();
            ViewBag.AllBrands = await db.Brands.ToListAsync();
            ViewBag.AllActiveSales = await db.GetActiveSalesAsync();
            return true;
        }
        // GET: Brands
        public async Task<ActionResult> Index() {
            await this.FillViewBag();
            return View(await db.Brands.ToListAsync());
		}

		// GET: Brands/Create
		public async Task<ActionResult> Create() {
            await this.FillViewBag();
            return View();
		}

		// POST: Brands/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create([Bind(Include = "BrandID, BrandName, Logo, Country, Description")] BrandModel brandModel, int FoundationYear) {
			brandModel.FoundationYear = new DateTime(FoundationYear,0,1);
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

		// GET: Brands/Edit/5
		[HttpGet]
		public async Task<ActionResult> Edit(int? BrandID) {
            await this.FillViewBag();
            if (BrandID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			BrandModel brandModel = await db.Brands.FindAsync(BrandID);
			if(brandModel == null) {
				return HttpNotFound();
			}
			return View(brandModel);
		}

		// POST: Brands/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit([Bind(Include = "BrandID, BrandName, Logo, Country, Description, FoundationYear")] BrandModel brandModel) {
            await this.FillViewBag();
            if (ModelState.IsValid) {
				if(!await (from b in db.Brands where b.BrandName.ToLower() == brandModel.BrandName.ToLower() select b).AnyAsync()) {
					db.Entry(brandModel).State = EntityState.Modified;
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}
			return View(brandModel);
		}

		// Brands/Details/5
		public async Task<ActionResult> Details(int? BrandID) {
            await this.FillViewBag();
            if (BrandID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			BrandModel brandModel = await db.Brands.FindAsync(BrandID);
			if(brandModel == null) {
				return HttpNotFound();
			}
			return View(brandModel);
		}

		// GET: Brands/Delete/5
		public async Task<ActionResult> Delete(int? BrandID) {
            await this.FillViewBag();
            if (BrandID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			BrandModel brandModel = await db.Brands.FindAsync(BrandID);
			if(brandModel == null) {
				return HttpNotFound();
			}
			return View(brandModel);
		}

		// POST: Brands/Delete/5
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

		[HttpPost, ActionName("Search")]
		public async Task<ActionResult> Search(string BrandName) {
            await this.FillViewBag();
            BrandModel brand = null;
			try {
				brand = await (from b in db.Brands where b.BrandName.ToLower() == BrandName.ToLower() select b).SingleAsync();
			} catch(SqlException ex) {

			}

			if(brand != null) {
				return RedirectToAction("Details", new { BrandID = brand.BrandID });
			}
			ViewBag.SearchString = BrandName;
			ViewBag.NotFoundError = "Brand not found";
			return View("Index");
		}

		[HttpPost]
		public async Task<ActionResult> TypeSearch(string SearchString) {
			if(SearchString == null || SearchString.Equals("")) {
				return Json("");
			}
			ICollection<string> brands = await (from b in db.Brands where b.BrandName.ToLower().StartsWith(SearchString.ToLower()) select b.BrandName).ToArrayAsync();
			return Json(brands);
		}

       
        public async Task<ActionResult> BrowseByID(int BrandID)
        {

            IList<SubCategoryModel> subCat = null;
            try
            {
                subCat = await db.GetBrandSubCategoriesAsync(BrandID);
            }
            catch (SqlException ex)
            {

            }
            await this.FillViewBag();
            return View("Browse", subCat);
        }
    }
}
