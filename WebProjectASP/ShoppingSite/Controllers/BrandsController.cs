using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ShoppingSite.Models;

namespace ShoppingSite.Controllers {
	[Authorize(Roles = "Administrator, Manager")]
	public class BrandsController : Controller {
		private ApplicationDbContext db = new ApplicationDbContext();

		// GET: Brands
		public async Task<ActionResult> Index() {
			return View(await db.Brands.ToListAsync());
		}

		// GET: Brands/Details/5
		public async Task<ActionResult> Details(int? id) {
			if(id == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			BrandModel brandModel = await db.Brands.FindAsync(id);
			if(brandModel == null) {
				return HttpNotFound();
			}
			return View(brandModel);
		}

		// GET: Brands/Create
		public ActionResult Create() {
			return View();
		}

		// POST: Brands/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create([Bind(Include = "BrandID,BrandName,Logo,Country,Description,FoundationYear")] BrandModel brandModel) {
			if(ModelState.IsValid) {
				db.Brands.Add(brandModel);
				await db.SaveChangesAsync();
				return RedirectToAction("Index");
			}

			return View(brandModel);
		}

		// GET: Brands/Edit/5
		public async Task<ActionResult> Edit(int? id) {
			if(id == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			BrandModel brandModel = await db.Brands.FindAsync(id);
			if(brandModel == null) {
				return HttpNotFound();
			}
			return View(brandModel);
		}

		// POST: Brands/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit([Bind(Include = "BrandID,BrandName,Logo,Country,Description,FoundationYear")] BrandModel brandModel) {
			if(ModelState.IsValid) {
				db.Entry(brandModel).State = EntityState.Modified;
				await db.SaveChangesAsync();
				return RedirectToAction("Index");
			}
			return View(brandModel);
		}

		// GET: Brands/Delete/5
		public async Task<ActionResult> Delete(int? id) {
			if(id == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			BrandModel brandModel = await db.Brands.FindAsync(id);
			if(brandModel == null) {
				return HttpNotFound();
			}
			return View(brandModel);
		}

		// POST: Brands/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DeleteConfirmed(int id) {
			BrandModel brandModel = await db.Brands.FindAsync(id);
			db.Brands.Remove(brandModel);
			await db.SaveChangesAsync();
			return RedirectToAction("Index");
		}

		protected override void Dispose(bool disposing) {
			if(disposing) {
				db.Dispose();
			}
			base.Dispose(disposing);
		}

		[HttpPost, ActionName("Search")]
		public async Task<ActionResult> Search(string BrandName) {

			BrandModel brand = await (from b in db.Brands where b.BrandName.ToLower() == BrandName.ToLower() select b).SingleAsync();

			if(brand != null) {
				return RedirectToAction("Details", brand.BrandID);
			}
			ViewBag.SearchString = BrandName;
			ViewBag.NotFoundError = "Brand not found";
			return View("Index");
		}

		[HttpPost]
		public async Task<ActionResult> TypeSearch(string SearchString) {
			ICollection<string> brands = await (from b in db.Brands where b.BrandName.ToLower().Contains(SearchString.ToLower()) select b.BrandName).ToListAsync();
			return Json(brands);
		}
	}
}
