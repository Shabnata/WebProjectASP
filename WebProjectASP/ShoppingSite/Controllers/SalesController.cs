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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ShoppingSite.Controllers {
	[Authorize(Roles = "Administrator, Manager")]
	public class SalesController : Controller {

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
					List<BrandModel> brandsOnSale = new List<BrandModel>();
					string[] selectedBrands = Request.Form.GetValues("CheckedBrands") ?? new string[] { };
					foreach(string id in selectedBrands) {
						BrandModel b = await db.Brands.FindAsync(Int32.Parse(id));
						brandsOnSale.Add(b);
					}
					saleModel.BrandsOnSale = brandsOnSale;
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

			viewModel.Discount = saleModel.Discount;
			viewModel.Emblem = saleModel.Emblem;
			viewModel.StartDate = saleModel.StartDate;
			viewModel.EndDate = saleModel.EndDate;
			viewModel.SaleID = saleModel.SaleID;
			viewModel.SaleName = saleModel.SaleName;

			viewModel.BrandsOnSale = saleModel.BrandsOnSale.ToList();
			viewModel.AllBrands = (await db.Brands.ToListAsync()).Except(viewModel.BrandsOnSale).ToList();

			await this.FillViewBag();
			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit([Bind(Include = "SaleID, SaleName, StartDate, EndDate, Discount, Emblem")] SaleEditViewModel model) {
			if(ModelState.IsValid) {
				if(!await (from s in db.Sales where s.SaleID != model.SaleID && s.SaleName.ToLower() == model.SaleName.ToLower() && !(s.StartDate >= model.EndDate || s.EndDate <= model.StartDate) select s).AnyAsync()) {
					SaleModel editedModel = await db.Sales.FindAsync(model.SaleID);
					editedModel.BrandsOnSale.Clear();

					List<BrandModel> selectedBrands = new List<BrandModel>();
					string[] selectedBrandsStrings = Request.Form.GetValues("CheckedBrands") ?? new string[] { };
					foreach(string str in selectedBrandsStrings) {
						BrandModel brd = await db.Brands.FindAsync(Int32.Parse(str));
						selectedBrands.Add(brd);
					}

					editedModel.BrandsOnSale = selectedBrands;
					editedModel.Discount = model.Discount;
					editedModel.Emblem = model.Emblem;
					editedModel.EndDate = model.EndDate;
					editedModel.StartDate = model.StartDate;
					editedModel.SaleName = model.SaleName;
					db.Entry(editedModel).State = EntityState.Modified;
					await db.SaveChangesAsync();
					return RedirectToAction("Index");

				}
			}
			await this.FillViewBag();
			return View("Error");
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

		[HttpPost, ActionName("Search")]
		public async Task<ActionResult> Search(string SaleName) {

			IList<SaleModel> sales = (SaleName.Equals("")) ? await db.Sales.ToListAsync() : await (from s in db.Sales where s.SaleName.ToLower().Contains(SaleName.ToLower()) select s).ToListAsync();

			await this.FillViewBag();
			ViewBag.SearchString = SaleName;
			if(sales.Count == 0) {
				ViewBag.NotFoundError = "Sale not found";
			} else if(sales.Count == 1 && sales.First().SaleName.ToLower().Equals(SaleName.ToLower())) {
				return RedirectToAction("Details", new { SaleID = sales.First().SaleID });
			}

			return View("Index", sales);
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
            model.ProductsInSale = await db.GetAllProductsInSaleAsync(SaleID);
            await this.FillViewBag();
			return View("Browse", model);
		}

		[HttpPost]
		public async Task<ActionResult> ActiveSalesSearch(string SaleName) {

			IList<SaleModel> allActiveSales = await db.GetActiveSalesAsync();
			IList<SaleModel> searchResults = (from s in allActiveSales where s.SaleName.ToLower().Contains(SaleName.ToLower()) select s).ToList();

			await this.FillViewBag();
			ViewBag.SearchString = SaleName;
			if(searchResults.Count == 0) {
				ViewBag.NotFoundError = "Sale not found";
			} else if(searchResults.Count == 1 && searchResults.First().SaleName.ToLower().Equals(SaleName.ToLower())) {
				return RedirectToAction("Details", new { SaleID = searchResults.First().SaleID });
			}

			return View("Index", searchResults);
		}

	}
}
