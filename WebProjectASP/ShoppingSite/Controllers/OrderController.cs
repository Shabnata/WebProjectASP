using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ShoppingSite.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShoppingSite.Controllers {
    [Authorize]
    public class OrderController : Controller {

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

        [Authorize]
        public async Task<ActionResult> Index() {

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            IdentityRole customerRole = (from r in db.Roles where r.Name == "Customer" select r).Single();
            bool customer = false;

            foreach (IdentityUserRole iur in user.Roles) {
                if (iur.RoleId.Equals(customerRole.Id)) {
                    customer = true;
                    break;
                }
            }

            IList<OrderModel> orders = customer ? user.Orders.ToList() : await db.Orders.ToListAsync();

            await this.FillViewBag();
            return View(orders);
        }

        public async Task<ActionResult> Checkout() {

            ApplicationUser user = this.db.Users.Find(User.Identity.GetUserId());

            if (user.CartItems.Count < 1) {
                await this.FillViewBag();
                return View("EmptyCartError");
            }

            OrderModel order = new OrderModel() { ApplicationUserID = user.Id, OrderDate = DateTime.Now, Status = "New", OrderItems = new List<OrderItemModel>() };
            user.Orders.Add(order);

            foreach (CartItemModel cim in user.CartItems) {
                SaleModel bestSale = await db.GetProductBestActiveSale(cim.Product.SKU) ?? new SaleModel() { Discount = 0 };
                OrderItemModel oim = new OrderItemModel() { Price = cim.Product.Price * cim.Quantity * ((100 - bestSale.Discount) / 100), Quantity = cim.Quantity, SKU = cim.Product.SKU };
                order.OrderItems.Add(oim);
            }

            user.CartItems.Clear();

            await db.SaveChangesAsync();
            CheckoutViewModel model = new CheckoutViewModel() { UserName = user.UserName, OrderID = order.OrderID };

            await this.FillViewBag();
            return View(model);
        }

        [Authorize]
        public async Task<ActionResult> Search(int? OrderID) {

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            IdentityRole customerRole = (from r in db.Roles where r.Name == "Customer" select r).Single();
            bool customer = false;

            foreach (IdentityUserRole iur in user.Roles) {
                if (iur.RoleId.Equals(customerRole.Id)) {
                    customer = true;
                    break;
                }
            }

            IList<OrderModel> model;

            if (customer) {
                model = (OrderID != null) ? (from o in user.Orders where o.OrderID == OrderID select o).ToList() : user.Orders.ToList();
            } else {
                model = (OrderID != null) ? await (from o in db.Orders where o.OrderID == OrderID select o).ToListAsync() : await db.Orders.ToListAsync();
            }

            if (model.Count == 0) {
                ViewBag.NotFoundError = "Order not found";
            }

            await this.FillViewBag();
            return View("Index", model);
        }

        [Authorize]
        public async Task<ActionResult> SearchByUser(string OrderID) {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            IdentityRole customerRole = (from r in db.Roles where r.Name == "Customer" select r).Single();
            bool customer = false;

            foreach (IdentityUserRole iur in user.Roles) {
                if (iur.RoleId.Equals(customerRole.Id)) {
                    customer = true;
                    break;
                }
            }

            IList<OrderModel> model;

            if (customer) {
                model = user.Orders.ToList();
            } else {
                model = (!OrderID.Equals("")) ? await (from o in db.Orders where o.User.UserName.ToLower().Contains(OrderID.ToLower()) select o).ToListAsync() : await db.Orders.ToListAsync();
            }

            if (model.Count == 0) {
                ViewBag.NotFoundError = "Order not found";
            }

            await this.FillViewBag();
            return View("Index", model);
        }

        [Authorize]
        public async Task<ActionResult> Details(int OrderID) {

            OrderModel model = await db.Orders.FindAsync(OrderID);

            await this.FillViewBag();
            return View(model);
        }

    }
}