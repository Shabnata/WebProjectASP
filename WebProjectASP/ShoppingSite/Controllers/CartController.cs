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

		public async Task<ActionResult> ViewCart() {

			CartViewModel model = new CartViewModel();

			if(User.Identity.IsAuthenticated) { // User logged in
				model.User = db.Users.Find(User.Identity.GetUserId());
				model.CartItems = model.User.CartItems;
				
			} else { // Guest
				model.User = null; // Display name with model.User.Name ?? "Guest"
				model.CartItems = Session["GuestCartItems"] as IList<CartItemModel> ?? new List<CartItemModel>();
			}

			model.TotalPrice = 0;
			foreach(CartItemModel cim in model.CartItems) {
				decimal tmpItemPrice = cim.Product.Price * cim.Quantity;
				SaleModel tmpItemSale = await db.GetProductBestActiveSale(cim.Product.SKU);
				tmpItemPrice *= ((100 - tmpItemSale.Discount) / 100);
				model.TotalPrice += tmpItemPrice;
			}


            await this.FillViewBag();
			return View(model);
		}

		public async Task<ActionResult> Update(int SKU, int Quantity = 1) {
			// TODO Finish this
			CartViewModel model = new CartViewModel();

			if(User.Identity.IsAuthenticated) { // User logged in
				model.User = db.Users.Find(User.Identity.GetUserId());
				model.CartItems = model.User.CartItems;
				
			} else { // Guest
				model.User = null; // Display name with model.User.Name ?? "Guest"
				model.CartItems = Session["GuestCartItems"] as IList<CartItemModel> ?? new List<CartItemModel>();
			}

			CartItemModel updateModel = model.CartItems.SingleOrDefault((CartItemModel a) => { return a.SKU == SKU; });

			if(updateModel.SKU != SKU) { // New item
				updateModel.Product = await db.Products.FindAsync(SKU);
				updateModel.SKU = SKU;
				updateModel.Quantity = Quantity;
				if(model.User != null) {
					updateModel.ApplicationUserID = model.User.Id;
					updateModel.Customer = model.User;
					model.User.CartItems.Add(updateModel);
					await db.SaveChangesAsync();
				} else {
					(Session["GuestCartItems"] as IList<CartItemModel>).Add(updateModel);
				}

			} else { // Existing item
				if(model.User != null) {
					updateModel.Quantity = Quantity;
				}
				if(updateModel.Quantity <= 0) {
					model.User.CartItems.Remove(updateModel);
				}
			}

			if(model.User != null) {
				if(updateModel.Quantity <= 0) {
					model.User.CartItems.Remove(updateModel);
				}
			} else { // Guest

			}

			return View("ViewCart");
		}

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

			
			return View("ViewCart");
		}

		public async Task<ActionResult> Checkout() {
			await this.FillViewBag();
			return View();
		}
	}
}