using ShoppingSite.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

		// Sales
		public async Task<ActionResult> Index() {
			await this.FillViewBag();

			return View(await db.Sales.ToListAsync());
		}

		// GET: Sales/Create
		[HttpGet]
		public async Task<ActionResult> Create() {
			await this.FillViewBag();

			return View();
		}

		// POST: Sales/Create/5
		[HttpPost]
		public async Task<ActionResult> Create([Bind(Include= "SaleName, StartDate, EndDate, Discount, Emblem")] SaleModel saleModel) {
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

		// GET: Sales/Edit/5
		[HttpGet]
		public async Task<ActionResult> Edit(int? SaleID) {
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

		// POST: Sales/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit([Bind(Include= "SaleName, StartDate, EndDate, Discount, Emblem")] SaleModel saleModel) {
			if(ModelState.IsValid) {
				if(!await (from s in db.Sales where s.SaleName.ToLower() == saleModel.SaleName.ToLower() && !(s.StartDate >= saleModel.EndDate || s.EndDate <= saleModel.StartDate) select s).AnyAsync()) {
					string[] brands = Request.Form.GetValues("Brands");
					string[] subCategories = Request.Form.GetValues("SubCategories");
					string[] products = Request.Form.GetValues("Products");
					List<ProductModel> productsOnSale = new List<ProductModel>();

					foreach(string b in brands) {
						BrandModel tmpBrand = await db.Brands.FindAsync(Int32.Parse(b));
						foreach(ProductModel pm in tmpBrand.Products) {
							productsOnSale.Add(pm);
						}
					}
					Boolean flag = false;
					foreach(string c in subCategories) {
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
					foreach(string p in products) {
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

					saleModel.Products = productsOnSale;
					db.Entry(saleModel).State = EntityState.Modified;
					await db.SaveChangesAsync();
					return RedirectToAction("Index");
				}
			}
			await this.FillViewBag();
			return View(saleModel);
		}
	}
}