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
	public class ProductsController : Controller {

		private ApplicationDbContext db = new ApplicationDbContext();
		private async Task<Boolean> FillViewBag() {
            ViewBag.AllCategories = await db.Categories.ToListAsync();
            ViewBag.AllBrands = await db.Brands.ToListAsync();
            ViewBag.AllActiveSales = await db.GetActiveSalesAsync();
            return true;
        }
        // GET: Products
        public async Task<ActionResult> Index() {
            await this.FillViewBag();
            return View(await db.Products.ToListAsync());
		}

		// GET: Products/Create
		[HttpGet]
		public async Task<ActionResult> Create() {
			ProductViewModel model = new ProductViewModel();
			model.AllBrand = await db.Brands.ToListAsync();
            model.AllSubCategories = await db.SubCategories.ToListAsync(); 
            await this.FillViewBag();
            return View(model);
		}

		// POST: Products/Create/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create([Bind(Include = "ProductName, Description, CoverPath, Price, BrandID")] ProductModel productModel) {
            await this.FillViewBag();
			if(ModelState.IsValid) {
				if(!await (from p in db.Products where p.ProductName.ToLower() == productModel.ProductName.ToLower() && p.BrandID == productModel.BrandID select p).AnyAsync()) {
					string[] pictures = Request.Form.GetValues("ProductPictures");
					List<ProductPictureModel> ppm = new List<ProductPictureModel>();
					foreach(string str in pictures) {
						if(!str.Equals("")) {
                            ppm.Add(new ProductPictureModel { SKU = productModel.SKU, PicturePath = str, Product = productModel });
                        }
					}
					productModel.ProductPictures = ppm;
                    productModel.Brand = await db.Brands.FindAsync(productModel.BrandID);
					db.Products.Add(productModel);
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}

			return View("Error");
		}

		// POST: Products/Edit/5
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
            return View(model);
		}

		// POST: Products/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit([Bind(Include = "SKU, ProductName, Description, CoverPath, Price, BrandID")] ProductModel productModel) {
			if(ModelState.IsValid) {
				if(!await (from p in db.Products where p.ProductName.ToLower() == productModel.ProductName.ToLower() && p.BrandID == productModel.BrandID && p.SKU != productModel.SKU select p).AnyAsync()) {
					ProductModel editedProduct = await db.Products.FindAsync(productModel.SKU);
					string[] pictures = (Request.Form.GetValues("ProductPictures") != null) ? Request.Form.GetValues("ProductPictures") : new string[] { };
					List<ProductPictureModel> ppm = new List<ProductPictureModel>();
					foreach(string str in pictures) {
						ppm.Add(new ProductPictureModel { SKU = productModel.SKU, PicturePath = str, Product = productModel });
					}

					string[] selectedSubCategoriesStrings = (Request.Form.GetValues("CheckedSubCategories") != null) ? Request.Form.GetValues("CheckedSubCategories") : new string[] { };
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
					foreach(SubCategoryModel scm in editedProduct.ProductCategories) {
						scm.Products.Remove(editedProduct);
					}
					editedProduct.Brand = await db.Brands.FindAsync(productModel.BrandID);
					editedProduct.BrandID = productModel.BrandID;
					editedProduct.CoverPath = productModel.CoverPath;
					editedProduct.Description = productModel.Description;
					editedProduct.Price = productModel.Price;
					editedProduct.ProductCategories = selectedSubCategoriesList;
					editedProduct.ProductName = productModel.ProductName;
					editedProduct.ProductPictures = ppm;
                  
					db.Entry(editedProduct).State = EntityState.Modified;
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}
            await this.FillViewBag();
            return View("Error");
		}

		// Products/Details/5
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


		// GET: Products/Delete/5
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

		// POST: Products/Delete/5
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

			ProductModel product = null;
			try {
				product = await (from p in db.Products where p.ProductName.ToLower() == ProductName.ToLower() select p).SingleAsync();
			} catch(SqlException ex) {

			}

			if(product != null) {
				return RedirectToAction("Details", new { SKU = product.SKU });
			}
			ViewBag.SearchString = ProductName;
			ViewBag.NotFoundError = "Product not found";
            await this.FillViewBag();
            return View("Index");
		}

		[HttpPost]
		public async Task<ActionResult> TypeSearch(string SearchString) {
			if(SearchString == null || SearchString.Equals("")) {
				return Json("");
			}
			ICollection<string> products = await (from p in db.Products where p.ProductName.ToLower().StartsWith(SearchString.ToLower()) select p.ProductName).ToListAsync();
			return Json(products);
		}

	}
}