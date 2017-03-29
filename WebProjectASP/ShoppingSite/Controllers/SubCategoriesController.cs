using ShoppingSite.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShoppingSite.Controllers {
	[Authorize(Roles = "Administrator, Manager")]
	public class SubCategoriesController : Controller {

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
			return View(await db.SubCategories.ToListAsync());
		}

		[HttpGet]
		public async Task<ActionResult> Create() {
			SubCategoryCreateEditViewModel model = new SubCategoryCreateEditViewModel();
			model.AllCategories = await db.Categories.ToListAsync();
			await this.FillViewBag();
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create([Bind(Include = "SubCategoryName, SubCategoryLogo")] SubCategoryModel subCategoryModel) {
			if(ModelState.IsValid) {
				if(!await (from sc in db.SubCategories where sc.SubCategoryName.ToLower() == subCategoryModel.SubCategoryName.ToLower() select sc).AnyAsync()) {
					subCategoryModel.ParentCategories = new List<CategoryModel>();
					foreach(string c in Request.Form.GetValues("SelectedCategories")) {
						subCategoryModel.ParentCategories.Add(await db.Categories.FindAsync(Int32.Parse(c)));
					}
					db.SubCategories.Add(subCategoryModel);
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}
			await this.FillViewBag();
			return View("Error");
		}

		[HttpGet]
		public async Task<ActionResult> Edit(int? SubCategoryID) {
			if(SubCategoryID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			SubCategoryModel subCategoryModel = await db.SubCategories.FindAsync(SubCategoryID);
			if(subCategoryModel == null) {
				return HttpNotFound();
			}
			SubCategoryCreateEditViewModel model = new SubCategoryCreateEditViewModel();
			model.SubCategoryID = subCategoryModel.SubCategoryID;
			model.SubCategoryLogo = subCategoryModel.SubCategoryLogo;
			model.SubCategoryName = subCategoryModel.SubCategoryName;
			model.ParentCategories = subCategoryModel.ParentCategories.ToList();
			model.AllCategories = await db.Categories.ToListAsync();
			List<CategoryModel> tmpLst = new List<CategoryModel>();
			foreach(CategoryModel cm in model.AllCategories) {
				foreach(CategoryModel pc in model.ParentCategories) {
					if(cm.CategoryID == pc.CategoryID) {
						tmpLst.Add(cm);
						break;
					}
				}
			}
			foreach(CategoryModel tcm in tmpLst) {
				model.AllCategories.Remove(tcm);
			}
			await this.FillViewBag();
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit([Bind(Include = "SubCategoryID, SubCategoryName, SubCategoryLogo")] SubCategoryModel subCategoryModel) {
			if(ModelState.IsValid) {
				if(!await (from sc in db.SubCategories where sc.SubCategoryName.ToLower() == subCategoryModel.SubCategoryName.ToLower() select sc).AnyAsync()) {
					string[] selectedCategoriesStrings = Request.Form.GetValues("CheckedCategories");
					List<CategoryModel> selectedCategoriesList = new List<CategoryModel>();
					foreach(string str in selectedCategoriesStrings) {
						CategoryModel cat = await (from c in db.Categories where c.CategoryID == Int32.Parse(str) select c).SingleAsync();
						selectedCategoriesList.Add(cat);
					}
					subCategoryModel.ParentCategories = selectedCategoriesList;
					db.Entry(subCategoryModel).State = EntityState.Modified;
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}
			await this.FillViewBag();
			return View(subCategoryModel);
		}

		[HttpGet]
		public async Task<ActionResult> Details(int? SubCategoryID) {
			if(SubCategoryID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			SubCategoryModel subCategoryModel = await db.SubCategories.FindAsync(SubCategoryID);
			if(subCategoryModel == null) {
				return HttpNotFound();
			}
			await this.FillViewBag();
			return View(subCategoryModel);
		}

		[HttpGet]
		public async Task<ActionResult> Delete(int? SubCategoryID) {
			if(SubCategoryID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			SubCategoryModel subCategoryModel = await db.SubCategories.FindAsync(SubCategoryID);
			if(subCategoryModel == null) {
				return HttpNotFound();
			}
			await this.FillViewBag();
			return View(subCategoryModel);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DeleteConfirmed(int SubCategoryID) {
			SubCategoryModel subCategoryModel = await db.SubCategories.FindAsync(SubCategoryID);
			db.SubCategories.Remove(subCategoryModel);
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
		public async Task<ActionResult> Search(string SubCategoryName) {

			SubCategoryModel subCategory = null;
			try {
				subCategory = await (from sc in db.SubCategories where sc.SubCategoryName.ToLower() == SubCategoryName.ToLower() select sc).SingleAsync();
			} catch(SqlException ex) {

			}

			if(subCategory != null) {
				return RedirectToAction("Details", new { SubCategoryID = subCategory.SubCategoryID });
			}
			ViewBag.SearchString = SubCategoryName;
			ViewBag.NotFoundError = "SubCategory not found";
			await this.FillViewBag();
			return View("Index");
		}

		[AllowAnonymous]
		[HttpPost]
		public async Task<ActionResult> TypeSearch(string SearchString) {
			if(SearchString == null || SearchString.Equals("")) {
				return Json("");
			}
			ICollection<string> subCategories = await (from sc in db.SubCategories where sc.SubCategoryName.ToLower().StartsWith(SearchString.ToLower()) select sc.SubCategoryName).ToListAsync();
			return Json(subCategories);
		}
	}
}
