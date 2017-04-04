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
	public class SalesController : Controller {

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

			return View(await db.Sales.ToListAsync());
		}

		[HttpGet]
		public async Task<ActionResult> Create() {
            SaleViewModel model = new SaleViewModel();
            model.AllBrands = await db.Brands.ToListAsync();
            await this.FillViewBag();
            return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create([Bind(Include = "SaleName, StartDate, EndDate, Discount, Emblem")] SaleModel saleModel) {
			if(ModelState.IsValid) {
				if(!await (from s in db.Sales where s.SaleName.ToLower() == saleModel.SaleName.ToLower() && !(s.StartDate >= saleModel.EndDate || s.EndDate <= saleModel.StartDate) select s).AnyAsync()) {
					db.Sales.Add(saleModel);
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}
			await this.FillViewBag();
			return View("Error");
		}

		[HttpGet]
		public async Task<ActionResult> Edit(int? SaleID) {
			if(SaleID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			SaleModel saleModel = await db.Sales.FindAsync(SaleID);
			if(saleModel == null) {
				return HttpNotFound();
			}

			SaleEditViewModel viewModel = new SaleEditViewModel();
            //viewModel.AllSubCategories = await db.SubCategories.ToListAsync();
			viewModel.Discount = saleModel.Discount;
			viewModel.Emblem = saleModel.Emblem;
			viewModel.StartDate = saleModel.StartDate;
			viewModel.EndDate = saleModel.EndDate;
			viewModel.SaleID = saleModel.SaleID;
			viewModel.SaleName = saleModel.SaleName;

            //viewModel.ProductsOnSale = saleModel.Products.ToList();
            viewModel.BrandsOnSale= saleModel.Brands.ToList();
            viewModel.AllBrands= (await db.Brands.ToListAsync()).Except(viewModel.BrandsOnSale).ToList();

            //viewModel.AllProducts = (await db.Products.ToListAsync()).Except(viewModel.ProductsOnSale).ToList();
            //viewModel.AllProducts = await (db.Products).ToListAsync();
            //foreach(ProductModel pm in viewModel.ProductsOnSale) {
            //	viewModel.AllProducts.Remove(pm);
            //}

            await this.FillViewBag();
			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit([Bind(Include = "SaleID, SaleName, StartDate, EndDate, Discount, Emblem")] SaleEditViewModel model) {
			if(ModelState.IsValid) {
				if(!await (from s in db.Sales where s.SaleID != model.SaleID && s.SaleName.ToLower() == model.SaleName.ToLower() && !(s.StartDate >= model.EndDate || s.EndDate <= model.StartDate) select s).AnyAsync()) {
					string[] brandIDs = (Request.Form.GetValues("Brands") != null) ? Request.Form.GetValues("Brands") : new string[] { };
					string[] subCategoriesIDs = (Request.Form.GetValues("SubCategories") != null) ? Request.Form.GetValues("SubCategories") : new string[] { };
					string[] productIDs = (Request.Form.GetValues("Products") != null) ? Request.Form.GetValues("Products") : new string[] { };
					List<ProductModel> productsOnSale = new List<ProductModel>();

					foreach(string b in brandIDs) {
						BrandModel tmpBrand = await db.Brands.FindAsync(Int32.Parse(b));
						foreach(ProductModel pm in tmpBrand.Products) {
							productsOnSale.Add(pm);
						}
					}
					Boolean flag = false;
					foreach(string c in subCategoriesIDs) {
						SubCategoryModel tmpSubCategory = await db.SubCategories.FindAsync(Int32.Parse(c));
						foreach(ProductModel scpm in tmpSubCategory.Products) {
							flag = false;
							foreach(ProductModel pm in productsOnSale) {
								if(pm.SKU == scpm.SKU) {
									flag = true;
									break;
								}
							}
							if(!flag) {
								productsOnSale.Add(scpm);
							}
						}
					}
					foreach(string p in productIDs) {
						ProductModel product = await db.Products.FindAsync(Int32.Parse(p));
						flag = false;
						foreach(ProductModel pm in productsOnSale) {
							if(pm.SKU == product.SKU) {
								flag = true;
								break;
							}
						}
						if(!flag) {
							productsOnSale.Add(product);
						}
					}

					SaleModel sm = await db.Sales.FindAsync(model.SaleID);
					sm.Discount = model.Discount;
					sm.Emblem = model.Emblem;
					sm.StartDate = model.StartDate;
					sm.EndDate = model.EndDate;
                    //sm.Brands= BrandsOnSale;
                    //sm.Products = productsOnSale;
                    sm.SaleName = model.SaleName;

					db.Entry(sm).State = EntityState.Modified;
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}
			await this.FillViewBag();
			return View(model);
		}

		[HttpGet]
		public async Task<ActionResult> Details(int? SaleID) {
			if(SaleID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			SaleModel saleModel = await db.Sales.FindAsync(SaleID);
			if(saleModel == null) {
				return HttpNotFound();
			}
			await this.FillViewBag();
			return View(saleModel);
		}

		[HttpGet]
		public async Task<ActionResult> Delete(int? SaleID) {
			if(SaleID == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			SaleModel saleModel = await db.Sales.FindAsync(SaleID);
			if(saleModel == null) {
				return HttpNotFound();
			}
			await this.FillViewBag();
			return View(saleModel);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DeleteConfirmed(int SaleID) {
			SaleModel saleModel = await db.Sales.FindAsync(SaleID);
			db.Sales.Remove(saleModel);
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

		[AllowAnonymous]
		[HttpPost, ActionName("Search")]
		public async Task<ActionResult> Search(string SaleName) {

			List<SaleModel> salesLst = null;
			try {
				salesLst = await (from s in db.Sales where s.SaleName.ToLower() == SaleName.ToLower() select s).ToListAsync();
			} catch(SqlException ex) {

			}

			if(salesLst != null) {
				if(salesLst.Count == 1) {
					return RedirectToAction("Details", new { SaleID = salesLst[0].SaleID });
				} else {
					await this.FillViewBag();
					// TODO Add this View
					return View("ListSales", salesLst);
				}
			}
			ViewBag.SearchString = SaleName;
			ViewBag.NotFoundError = "Sale not found";
			await this.FillViewBag();
			return View("Index");
		}

		[AllowAnonymous]
		[HttpPost]
		public async Task<ActionResult> TypeSearch(string SearchString) {
			if(SearchString == null || SearchString.Equals("")) {
				return Json("");
			}
			// TODO Check if Distinct() works 
			ICollection<string> sales = await (from s in db.Sales where s.SaleName.ToLower().StartsWith(SearchString.ToLower()) select s.SaleName).Distinct().ToListAsync();
			return Json(sales);
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult> BrowseByID(int SaleID) {

			SaleModel sale = await db.Sales.FindAsync(SaleID);
			SaleBrowseViewModel model = new SaleBrowseViewModel();
			model.AllActiveSales = await db.GetActiveSalesAsync();
			model.ThisSale = sale;
			await this.FillViewBag();
            return View("Browse", model);
        }
	}
}