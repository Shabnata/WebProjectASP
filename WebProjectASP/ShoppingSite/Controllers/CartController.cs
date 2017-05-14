using ShoppingSite.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace ShoppingSite.Controllers {
	public class CartController : Controller {

		private ApplicationDbContext db = new ApplicationDbContext();

		private async Task<Boolean> FillViewBag() {
			ViewBag.AllCategories = await db.Categories.ToListAsync();
			ViewBag.AllBrands = await db.Brands.ToListAsync();
			ViewBag.AllActiveSales = await db.GetActiveSalesAsync();
			return true;
		}

		[HttpGet]
		public async Task<ActionResult> ViewCart() {

			CartViewModel model = new CartViewModel();
			model.CartItemsSales = new Dictionary<CartItemModel, SaleModel>();

			if(User.Identity.IsAuthenticated) { // User logged in
				model.User = db.Users.Find(User.Identity.GetUserId());
				model.CartItems = model.User.CartItems;

			} else { // Guest
				model.User = new ApplicationUser() { FirstName = "Guest" };
				model.CartItems = Session["GuestCartItems"] as IList<CartItemModel> ?? new List<CartItemModel>();
			}

			model.TotalPrice = 0;
			foreach(CartItemModel cim in model.CartItems) {
				decimal tmpItemPrice = cim.Product.Price * cim.Quantity;
				SaleModel tmpItemSale = await db.GetProductBestActiveSale(cim.Product.SKU) ?? new SaleModel() { Discount = 0};
				model.CartItemsSales.Add(cim, tmpItemSale);
				tmpItemPrice *= ((100 - tmpItemSale.Discount) / 100);
				model.TotalPrice += tmpItemPrice;
			}


			await this.FillViewBag();
			return View(model);
		}

		[HttpPost]
		public async Task<ActionResult> Update(int SKU, int Quantity = 1) {
			
			CartViewModel model = new CartViewModel();

			if(User.Identity.IsAuthenticated) { // User logged in
				model.User = db.Users.Find(User.Identity.GetUserId());
				model.CartItems = model.User.CartItems;

			} else { // Guest
				model.User = null; 
				model.CartItems = Session["GuestCartItems"] as IList<CartItemModel> ?? new List<CartItemModel>();
				Session["GuestCartItems"] = model.CartItems;
			}

			CartItemModel updateModel = model.CartItems.SingleOrDefault((CartItemModel a) => { return a.SKU == SKU; });

			if(updateModel == null) { // New item
				updateModel = new CartItemModel();
				updateModel.Product = await db.Products.FindAsync(SKU);
				updateModel.SKU = SKU;
				updateModel.Quantity = Quantity;
				if(model.User != null) {
					updateModel.ApplicationUserID = model.User.Id;
					updateModel.Customer = model.User;
					model.User.CartItems.Add(updateModel);

				} else {
					// model.CartItems
					(Session["GuestCartItems"] as IList<CartItemModel>).Add(updateModel);
				}

			} else { // Existing item
				updateModel.Quantity = Quantity;
				if(updateModel.Quantity <= 0) {
					if(model.User != null) {
						model.User.CartItems.Remove(updateModel);
					} else {
						// model.CartItems
						(Session["GuestCartItems"] as IList<CartItemModel>).Remove(updateModel);
					}
				}
			}


			if(model.User != null) {
				await db.SaveChangesAsync();
			}

			return RedirectToAction("ViewCart");
		}

		[HttpPost]
		public async Task<ActionResult> Add(int SKU) {

			IList<CartItemModel> model = null;
			if(User.Identity.IsAuthenticated) { // User logged in
				model = db.Users.Find(User.Identity.GetUserId()).CartItems.ToList();
			} else { // Guest
				model = Session["GuestCartItems"] as IList<CartItemModel> ?? new List<CartItemModel>();
			}

			CartItemModel updateModel = model.SingleOrDefault((CartItemModel a) => { return a.SKU == SKU; }) ?? new CartItemModel() { SKU=SKU, Quantity = 0};

			// Note: RedirectToAction doesn't work here because Update is accessible via POST only. This is like "Forward" except probably a poor implementation.
			//return RedirectToAction("Update", new { SKU = updateModel.SKU, Quantity = (updateModel.Quantity + 1)});
			return await this.Update(updateModel.SKU,(updateModel.Quantity + 1));
		}

		[HttpPost]
		public async Task<ActionResult> Clear() {

			if(User.Identity.IsAuthenticated) { // User logged in
				db.Users.Find(User.Identity.GetUserId()).CartItems.Clear();
				await db.SaveChangesAsync();
			} else { // Guest
				if(Session["GuestCartItems"] as IList<CartItemModel> != null) {
					(Session["GuestCartItems"] as IList<CartItemModel>).Clear();
				} else {
					Session["GuestCartItems"] = new List<CartItemModel>();
				}
			}

			
			return RedirectToAction("ViewCart");
		}

		[HttpPost]
		public async Task<ActionResult> Checkout() {
			// TODO
			await this.FillViewBag();
			return View();
		}
	}
}