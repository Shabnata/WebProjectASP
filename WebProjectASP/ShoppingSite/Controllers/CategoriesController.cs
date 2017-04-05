﻿using ShoppingSite.Models;
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

		[HttpGet]
		public async Task<ActionResult> Index() {
			await this.FillViewBag();

			return View(await db.Categories.ToListAsync());
		}

		[HttpGet]
		public async Task<ActionResult> Create() {
			await this.FillViewBag();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create([Bind(Include = "CategoryName, Logo")] CategoryModel categoryModel) {
			if(ModelState.IsValid) {
				if(!await (from c in db.Categories where c.CategoryID != categoryModel.CategoryID && c.CategoryName.ToLower() == categoryModel.CategoryName.ToLower() select c).AnyAsync()) {
					db.Categories.Add(categoryModel);
					await db.SaveChangesAsync();

					return RedirectToAction("Index");
				}
			}
			await this.FillViewBag();
			return View("Error");
		}

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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit([Bind(Include = "CategoryID, CategoryName, Logo")] CategoryModel categoryModel) {
			if(ModelState.IsValid) {
				if(!await (from c in db.Categories where c.CategoryID != categoryModel.CategoryID && c.CategoryName.ToLower() == categoryModel.CategoryName.ToLower() select c).AnyAsync()) {
					db.Entry(categoryModel).State = EntityState.Modified;
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}
			await this.FillViewBag();
			return View(categoryModel);
		}

		[HttpGet]
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

		[HttpGet]
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

		[AllowAnonymous]
		[HttpPost, ActionName("Search")]
		public async Task<ActionResult> Search(string CategoryName) {

			IList<CategoryModel> categories = await (from c in db.Categories where c.CategoryName.ToLower().Contains(CategoryName.ToLower()) select c).ToListAsync();

			await this.FillViewBag();
			ViewBag.SearchString = CategoryName;
			if(categories.Count == 0) {
				ViewBag.NotFoundError = "Category not found";
			} else if(categories.Count == 1 && categories.First().CategoryName.ToLower().Equals(CategoryName.ToLower())) {
				return RedirectToAction("Details", new { CategoryID = categories.First().CategoryID });
			}

			return View("Index", categories);
		}

		[AllowAnonymous]
		[HttpPost]
		public async Task<ActionResult> TypeSearch(string SearchString) {
			if(SearchString == null || SearchString.Equals("")) {
				return Json("");
			}
			ICollection<string> categories = await (from c in db.Categories where c.CategoryName.ToLower().StartsWith(SearchString.ToLower()) select c.CategoryName).ToListAsync();
			return Json(categories);
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult> Browse(String Btn) {

			CategoryModel category = await (from c in db.Categories where c.CategoryName.ToLower() == Btn.ToLower() select c).SingleAsync();

			IList<ProductModel> allProucts = await db.GetCategoryProductsAsync(category.CategoryID);
			List<ProductModel> featuredProucts = new List<ProductModel>();
			// TODO Randomize this maybe?
			for(int i = 0; i < 8 && i < allProucts.Count; i++) {
				featuredProucts.Add(allProucts[i]);
			}
			CategoryBrowseViewModel viewModel = new CategoryBrowseViewModel() { Category = category, FeaturedProducts = featuredProucts };

			await this.FillViewBag();
			return View(viewModel);
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult> BrowseByID(int CategoryID) {

			CategoryModel category = await db.Categories.FindAsync(CategoryID);

			IList<ProductModel> allProucts = await db.GetCategoryProductsAsync(CategoryID);
			List<ProductModel> featuredProucts = new List<ProductModel>();
			// TODO Randomize this maybe?
			for(int i = 0; i < 8 && i < allProucts.Count; i++) {
				featuredProucts.Add(allProucts[i]);
			}
			CategoryBrowseViewModel viewModel = new CategoryBrowseViewModel() { Category = category, FeaturedProducts = featuredProucts };

			await this.FillViewBag();
			return View("Browse", viewModel);
		}

	}
}
