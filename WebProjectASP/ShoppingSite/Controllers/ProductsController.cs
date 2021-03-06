﻿using ShoppingSite.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Script.Serialization;

namespace ShoppingSite.Controllers {
	[Authorize(Roles = "Administrator, Manager")]
	public class ProductsController : Controller {

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
			return View(await db.Products.ToListAsync());
		}

		[HttpGet]
		public async Task<ActionResult> Create() {
			ProductViewModel model = new ProductViewModel();
			model.AllBrand = await db.Brands.ToListAsync();
			model.AllSubCategories = await db.SubCategories.ToListAsync();
			await this.FillViewBag();
			ViewBag.DefaultIMG = Url.Content("/Content/icons/DefaultCover.png");
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create([Bind(Include = "ProductName, Description, CoverPath, Price, BrandID")] ProductModel productModel) {
			await this.FillViewBag();
			if(ModelState.IsValid) {
				if(!await (from p in db.Products where p.ProductName.ToLower() == productModel.ProductName.ToLower() && p.BrandID == productModel.BrandID select p).AnyAsync()) {
					string[] pictures = Request.Form.GetValues("ProductPictures") ?? new string[] { };
					for(int i = 0; i < pictures.Length; i++) {
						for(int j = i + 1; j < pictures.Length; j++) {
							if(pictures[i].Equals(pictures[j])) {
								pictures[j] = "";
							}
						}
					}
					List<ProductPictureModel> ppm = new List<ProductPictureModel>();
					foreach(string str in pictures) {
						if(!str.Equals("")) {
							ppm.Add(new ProductPictureModel { SKU = productModel.SKU, PicturePath = str, Product = productModel });
						}
					}
					string[] subCategories = Request.Form.GetValues("CheckedSubCategories") ?? new string[] { };
					List<SubCategoryModel> scmLst = new List<SubCategoryModel>();
					foreach(string str in subCategories) {
						scmLst.Add(await db.SubCategories.FindAsync(Int32.Parse(str)));
					}

					productModel.ProductPictures = ppm;
					productModel.ProductCategories = scmLst;
					productModel.Brand = await db.Brands.FindAsync(productModel.BrandID);
					db.Products.Add(productModel);
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}

			ProductModel model = await (from p in db.Products where p.ProductName.ToLower() == productModel.ProductName.ToLower() && p.BrandID == productModel.BrandID select p).FirstAsync();
			ViewBag.ProductName = productModel.ProductName;
			ViewBag.BrandName = model.Brand.BrandName;
			return View("Error", model);
		}

		[HttpGet]
		public async Task<ActionResult> Edit(int? SKU) {
			if(SKU == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ProductModel productModel = await db.Products.FindAsync(SKU);
			if(productModel == null) {
				return HttpNotFound();
			}
			ProductViewModel model = new ProductViewModel();
			model.AllBrand = await db.Brands.ToListAsync();
			model.BrandID = productModel.BrandID;
			model.CoverPath = productModel.CoverPath;
			model.Description = productModel.Description;
			model.Price = productModel.Price;
			model.ProductName = productModel.ProductName;
			model.SKU = productModel.SKU;
			model.RelatedSubCategories = productModel.ProductCategories.ToList();
			model.AllSubCategories = await db.SubCategories.ToListAsync();
			;
			List<SubCategoryModel> tmpLst = new List<SubCategoryModel>();
			foreach(SubCategoryModel cm in model.AllSubCategories) {
				foreach(SubCategoryModel pc in model.RelatedSubCategories) {
					if(cm.SubCategoryID == pc.SubCategoryID) {
						tmpLst.Add(cm);
						break;
					}
				}
			}
			foreach(SubCategoryModel tcm in tmpLst) {
				model.AllSubCategories.Remove(tcm);
			}
			model.ProductPictures = (from pp in productModel.ProductPictures select pp.PicturePath).ToList();
			await this.FillViewBag();
			ViewBag.DefaultIMG = Url.Content("/Content/icons/DefaultCover.png");
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit([Bind(Include = "SKU, ProductName, Description, CoverPath, Price, BrandID")] ProductModel productModel) {
			if(ModelState.IsValid) {
				if(!await (from p in db.Products where p.ProductName.ToLower() == productModel.ProductName.ToLower() && p.BrandID == productModel.BrandID && p.SKU != productModel.SKU select p).AnyAsync()) {
					ProductModel editedProduct = await db.Products.FindAsync(productModel.SKU);
					string[] pictures = Request.Form.GetValues("ProductPictures") ?? new string[] { };
					for(int i = 0; i < pictures.Length; i++) {
						for(int j = i + 1; j < pictures.Length; j++) {
							if(pictures[i].Equals(pictures[j])) {
								pictures[j] = "";
							}
						}
					}
					List<ProductPictureModel> ppmLst = new List<ProductPictureModel>();
					foreach(string str in pictures) {
						if(!str.Equals("")) {
							ppmLst.Add(new ProductPictureModel { SKU = editedProduct.SKU, PicturePath = str, Product = editedProduct });
						}
					}

					//foreach(ProductPictureModel ppm in editedProduct.ProductPictures) {
					//	db.Entry(ppm).State = EntityState.Deleted;
					//}

					editedProduct.ProductPictures.Clear();

					editedProduct.ProductCategories.Clear();

					string[] selectedSubCategoriesStrings = Request.Form.GetValues("CheckedSubCategories") ?? new string[] { };
					List<SubCategoryModel> selectedSubCategoriesList = new List<SubCategoryModel>();
					foreach(string str in selectedSubCategoriesStrings) {
						SubCategoryModel subCat = await db.SubCategories.FindAsync(Int32.Parse(str));
						selectedSubCategoriesList.Add(subCat);
					}

					BrandModel brand = await db.Brands.FindAsync(productModel.BrandID);
					if(editedProduct.BrandID != productModel.BrandID) {
						brand.Products.Remove(editedProduct);
						db.Entry(brand).State = EntityState.Modified;
					}

					editedProduct.Brand = brand;
					editedProduct.BrandID = productModel.BrandID;
					editedProduct.CoverPath = productModel.CoverPath;
					editedProduct.Description = productModel.Description;
					editedProduct.Price = productModel.Price;
					editedProduct.ProductCategories = selectedSubCategoriesList;
					editedProduct.ProductName = productModel.ProductName;
					editedProduct.ProductPictures = ppmLst;

					db.Entry(editedProduct).State = EntityState.Modified;
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}
			await this.FillViewBag();
			ProductModel model = await (from p in db.Products where p.ProductName.ToLower() == productModel.ProductName.ToLower() && p.BrandID == productModel.BrandID select p).FirstAsync();
			ViewBag.ProductName = productModel.ProductName;
			ViewBag.BrandName = model.Brand.BrandName;
			return View("Error", model);
		}

		[HttpGet]
		public async Task<ActionResult> Details(int? SKU) {
			if(SKU == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ProductModel productModel = await db.Products.FindAsync(SKU);
			if(productModel == null) {
				return HttpNotFound();
			}
			await this.FillViewBag();
			return View(productModel);
		}


        [HttpGet]
		[AllowAnonymous]
        public async Task<ActionResult> Page(int? SKU) {
            if (SKU == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductModel productModel = await db.Products.FindAsync(SKU);
            if (productModel == null) {
                return HttpNotFound();
            }
			SaleModel sale = await db.GetProductBestActiveSale(productModel.SKU);
			ViewBag.OnSale = sale != null;
			ViewBag.ProductSale = sale;
            await this.FillViewBag();
            return View(productModel);
        }

        [HttpGet]
		public async Task<ActionResult> Delete(int? SKU) {
			if(SKU == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ProductModel productModel = await db.Products.FindAsync(SKU);
			if(productModel == null) {
				return HttpNotFound();
			}
			await this.FillViewBag();
			return View(productModel);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DeleteConfirmed(int SKU) {
			ProductModel productModel = await db.Products.FindAsync(SKU);
			db.Products.Remove(productModel);
			await db.SaveChangesAsync();
			await this.FillViewBag();
			return RedirectToAction("Index");
		}

		protected override void Dispose(bool disposing) {
			if(disposing) {
				db.Dispose();
			}
			base.Dispose(disposing);
		}

		[HttpPost, ActionName("Search")]
		public async Task<ActionResult> Search(string ProductName) {

			IList<ProductModel> products = await (from p in db.Products where p.ProductName.ToLower().Contains(ProductName.ToLower()) select p).ToListAsync();

			await this.FillViewBag();
			ViewBag.SearchString = ProductName;
			if(products.Count == 0) {
				ViewBag.NotFoundError = "Product not found";
			} else if(products.Count == 1 && products.First().ProductName.ToLower().Equals(ProductName.ToLower())) {
				return RedirectToAction("Details", new { SKU = products.First().SKU });
			}

			return View("Index", products);
		}

		[HttpPost]
		public async Task<ActionResult> CheckAvailability(int ID, string Name) {
			Boolean available = await (from p in db.Products where p.SKU != ID && p.ProductName.ToLower().Equals(Name.ToLower()) select p).AnyAsync();
			return Json(!available);
		}

		[AllowAnonymous]
		[HttpPost]
		public async Task<ActionResult> TypeSearch(string SearchString) {
			if(SearchString == null || SearchString.Equals("")) {
				return Json("");
			}
			ICollection<string> products = await (from p in db.Products where p.ProductName.ToLower().StartsWith(SearchString.ToLower()) select p.ProductName).ToListAsync();
			return Json(products);
		}

		[AllowAnonymous]
		[HttpPost, ActionName("BrowseFromNav")]
		public async Task<ActionResult> BrowseFromNav(string NavSearch, int? Page) {

			IList<ProductModel> products = await (from p in db.Products where p.ProductName.ToLower().Contains(NavSearch.ToLower()) select p).ToListAsync();

			int maxRows = 4;
			int maxCols = 4;
			int maxPages = decimal.ToInt32(decimal.Ceiling((decimal)products.Count / (decimal)(maxRows * maxCols)));
			IList<ProductModel> productPage = (from p in products select p).Skip((maxRows * maxCols) * ((Page ?? 1) - 1)).Take(maxRows * maxCols).ToList();
			
			ProductBrowseFromNavViewModel viewModel = new ProductBrowseFromNavViewModel() { ProductsPage = productPage, SearchString = NavSearch, AllCategories = await db.Categories.ToListAsync()};

			ViewBag.MaxRows = maxRows;
			ViewBag.MaxCols = maxCols;
			ViewBag.Page = Page ?? 1;
			ViewBag.MaxPages = maxPages;
			ViewBag.SearchString = NavSearch;

			await this.FillViewBag();
			
			ViewBag.NotFoundError = (products.Count == 0) ? "Product not found" : null;

			return View("BrowseFromNavAJAX", viewModel);
		}

		[AllowAnonymous]
		[HttpPost, ActionName("BrowseFromNavAJAX")]
		public async Task<ActionResult> BrowseFromNavAJAX(string NavSearch, int? Page) {
			IList<ProductModel> products = await (from p in db.Products where p.ProductName.ToLower().Contains(NavSearch.ToLower()) select p).ToListAsync();

			int maxRows = 4;
			int maxCols = 4;
			int maxPages = decimal.ToInt32(decimal.Ceiling((decimal)products.Count / (decimal)(maxRows * maxCols)));

			
			IList<ProductModel> productPage = (from p in products select p).Skip((maxRows * maxCols) * ((Page ?? 1) - 1)).Take(maxRows * maxCols).ToList();

			var resultArr = from product in productPage select new { SKU = product.SKU.ToString(), Price = product.Price.ToString(), ProductName = product.ProductName, CoverPath = product.CoverPath };

			JavaScriptSerializer myJSS = new JavaScriptSerializer();

			return Json(myJSS.Serialize(resultArr));
		}

	}
}
