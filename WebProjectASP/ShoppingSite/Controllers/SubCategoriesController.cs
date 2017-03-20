using ShoppingSite.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShoppingSite.Controllers {
	[Authorize(Roles = "Administrator, Manager")]
	public class SubCategoriesController : Controller {

		private ApplicationDbContext db = new ApplicationDbContext();

		// GET: SubCategories
		public async Task<ActionResult> Index() {
			return View(await db.SubCategories.ToListAsync());
		}

		// GET: SubCategories/Create
		[HttpGet]
		public async Task<ActionResult> Create() {
			SubCategoryCreateEditViewModel model = new SubCategoryCreateEditViewModel();
			model.AllCategories = await db.Categories.ToListAsync();
			return View(model);
		}

		// POST: SubCategories/Create/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create([Bind(Include = "SubCategoryName, SubCategoryLogo")] SubCategoryModel subCategoryModel) {
			if(ModelState.IsValid) {
				foreach(string c in Request.Form.GetValues("SelectedCategories")) {
					subCategoryModel.ParentCategories.Add(await db.Categories.FindAsync(Int32.Parse(c)));
				}
				db.SubCategories.Add(subCategoryModel);
				await db.SaveChangesAsync();
				return View("Index");
			}

			return View("Error");
		}
	}
}