using ShoppingSite.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShoppingSite.Controllers {
	[Authorize(Roles = "Administrator, Manager")]
	public class CategoriesController : Controller {

		private ApplicationDbContext db = new ApplicationDbContext();

        private async Task<Boolean> FillViewBag() {
            ViewBag.AllCategories = await db.Categories.ToListAsync();
            ViewBag.AllBrands = await db.Brands.ToListAsync();
            ViewBag.AllActiveSales = await db.GetActiveSalesAsync();
            return true;
        }

		// GET: Categories
		public async Task<ActionResult> Index() {
			await this.FillViewBag();

            return View(await db.Categories.ToListAsync());
		}

		// GET: Categories/Create
		[HttpGet]
		public async Task<ActionResult> Create() {
            await this.FillViewBag();
            return View();
		}

		// POST: Categories/Create/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create([Bind(Include = "CategoryName, Logo")] CategoryModel categoryModel) {
			if(ModelState.IsValid) {
				if(!await (from c in db.Categories where c.CategoryName.ToLower() == categoryModel.CategoryName.ToLower() select c).AnyAsync()) {
					db.Categories.Add(categoryModel);
					await db.SaveChangesAsync();
                   
                    return RedirectToAction("Index");
				}
			}
            await this.FillViewBag();
            return View("Error");
		}

		// POST: Categories/Edit/5
		[HttpGet]
		public async Task<ActionResult> Edit(int? CategoryID) {
			if(CategoryID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			CategoryModel categoryModel = await db.Categories.FindAsync(CategoryID);
			if(categoryModel == null) {
				return HttpNotFound();
			}
            await this.FillViewBag();
            return View(categoryModel);
		}

		// POST: Categories/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit([Bind(Include = "CategoryID, CategoryName, Logo")] CategoryModel categoryModel) {
			if(ModelState.IsValid) {
				if(!await (from c in db.Categories where c.CategoryName.ToLower() == categoryModel.CategoryName.ToLower() select c).AnyAsync()) {
					db.Entry(categoryModel).State = EntityState.Modified;
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}
            await this.FillViewBag();
            return View(categoryModel);
		}

		// Categories/Details/5
		public async Task<ActionResult> Details(int? CategoryID) {
			if(CategoryID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			CategoryModel categoryModel = await db.Categories.FindAsync(CategoryID);
			if(categoryModel == null) {
				return HttpNotFound();
			}
            await this.FillViewBag();
            return View(categoryModel);
		}

		// GET: Categories/Delete/5
		public async Task<ActionResult> Delete(int? CategoryID) {
			if(CategoryID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			CategoryModel categoryModel = await db.Categories.FindAsync(CategoryID);
			if(categoryModel == null) {
				return HttpNotFound();
			}
            await this.FillViewBag();
            return View(categoryModel);
		}

		// POST: Categories/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DeleteConfirmed(int CategoryID) {
			CategoryModel categoryModel = await db.Categories.FindAsync(CategoryID);
			db.Categories.Remove(categoryModel);
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
		public async Task<ActionResult> Search(string CategoryName) {

			CategoryModel category = null;
			try {
				category = await (from c in db.Categories where c.CategoryName.ToLower() == CategoryName.ToLower() select c).SingleAsync();
			} catch(SqlException ex) {

			}

			if(category != null) {
				return RedirectToAction("Details", new { CategoryID = category.CategoryID });
			}
			ViewBag.SearchString = CategoryName;
			ViewBag.NotFoundError = "Category not found";
            await this.FillViewBag();
            return View("Index");
		}

		[HttpPost]
		public async Task<ActionResult> TypeSearch(string SearchString) {
			if(SearchString == null || SearchString.Equals("")) {
				return Json("");
			}
			ICollection<string> categories = await (from c in db.Categories where c.CategoryName.ToLower().StartsWith(SearchString.ToLower()) select c.CategoryName).ToListAsync();
			return Json(categories);
		}
		// GET: Categories/Browse
		public async Task<ActionResult> Browse(String Btn) {
            
            CategoryModel category = null;
            try
            {
                category = await (from c in db.Categories where c.CategoryName.ToLower() == Btn.ToLower() select c).SingleAsync();
            }
            catch (SqlException ex)
            {

            }

            await this.FillViewBag();
            CategoryBrowseViewModel model = new CategoryBrowseViewModel();
            model.subCategories = category.SubCategories.ToList();
            IList<ProductModel> allProucts = await db.GetCategoryProductsAsync(category.CategoryID);
            List<ProductModel> featuredProucts = new List<ProductModel>();
            for (int i = 0; i < 10 && i< allProucts.Count; i++) {
                featuredProucts.Add(allProucts[i]);
            }
            model.featuredProducts = featuredProucts;
            return View(model);
		}
        public async Task<ActionResult> BrowseByID(int CategoryID)
        {

            CategoryModel category = null;
            try
            {
                category = await db.Categories.FindAsync(CategoryID);
            }
            catch (SqlException ex)
            {

            }
            await this.FillViewBag();
            CategoryBrowseViewModel model = new CategoryBrowseViewModel();
            model.subCategories = category.SubCategories.ToList();
            IList<ProductModel> allProucts = await db.GetCategoryProductsAsync(CategoryID);
            List<ProductModel> featuredProucts = new List<ProductModel>();
            for (int i = 0; i < 10 && i < allProucts.Count; i++)
            {
                featuredProucts.Add(allProucts[i]);
            }
            model.featuredProducts = featuredProucts;
            return View("Browse",model);
        }



    }
}
