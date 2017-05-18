using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ShoppingSite.Models;
using System.Data.Entity;

namespace ShoppingSite.Controllers {
	[Authorize]
	public class ManageController : Controller {
		private ApplicationSignInManager _signInManager;
		private ApplicationUserManager _userManager;

        private ApplicationDbContext db = new ApplicationDbContext();

        private async Task<Boolean> FillViewBag()
        {
            ViewBag.AllCategories = await db.Categories.ToListAsync();
            ViewBag.AllBrands = await db.Brands.ToListAsync();
            ViewBag.AllActiveSales = await db.GetActiveSalesAsync();
            return true;
        }

        public ManageController() {
		}

		public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) {
			UserManager = userManager;
			SignInManager = signInManager;
		}

		public ApplicationSignInManager SignInManager {
			get {
				return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
			}
			private set {
				_signInManager = value;
			}
		}

		public ApplicationUserManager UserManager {
			get {
				return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
			}
			private set {
				_userManager = value;
			}
		}

		//
		// GET: /Manage/Index
		public async Task<ActionResult> Index(ManageMessageId? message) {
			ViewBag.StatusMessage =
				message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
				: message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
				: message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
				: message == ManageMessageId.Error ? "An error has occurred."
				: message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
				: message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
				: "";

			var userId = User.Identity.GetUserId();
			var model = new IndexViewModel {
				HasPassword = HasPassword(),
				PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
				TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
				Logins = await UserManager.GetLoginsAsync(userId),
				BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
			};
			ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
			model.FirstName = user.FirstName ?? "Error";
			model.LastName = user.LastName ?? "Error";
            await this.FillViewBag();
            return View(model);
		}

		//
		// POST: /Manage/RemoveLogin
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey) {
			ManageMessageId? message;
			var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
			if(result.Succeeded) {
				var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
				if(user != null) {
					await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
				}
				message = ManageMessageId.RemoveLoginSuccess;
			} else {
				message = ManageMessageId.Error;
			}
			return RedirectToAction("ManageLogins", new { Message = message });
		}

		//
		// GET: /Manage/AddPhoneNumber
		public async Task<ActionResult> AddPhoneNumber() {
            await this.FillViewBag();
            return View();
		}

		[HttpPost]
		public async Task<ActionResult> UpdatePhoneNumber(string Number) {

			ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
			user.PhoneNumber = Number;

			await db.SaveChangesAsync();

//			await this.FillViewBag();
			return RedirectToAction("Index");
		}

		//
		// GET: /Manage/ChangePassword
		public async Task<ActionResult> ChangePassword() {
            await this.FillViewBag();
            return View();
		}

		//
		// POST: /Manage/ChangePassword
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model) {
			if(!ModelState.IsValid) {
				return View(model);
			}
			var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
			if(result.Succeeded) {
				var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
				if(user != null) {
					await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
				}
				return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
			}
			AddErrors(result);
            await this.FillViewBag();
            return View(model);
		}

		//
		// GET: /Manage/SetPassword
		public async Task<ActionResult> SetPassword() {
            await this.FillViewBag();
            return View();
		}

		//
		// POST: /Manage/SetPassword
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> SetPassword(SetPasswordViewModel model) {
			if(ModelState.IsValid) {
				var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
				if(result.Succeeded) {
					var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
					if(user != null) {
						await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
					}
					return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
				}
				AddErrors(result);
			}

            // If we got this far, something failed, redisplay form
            await this.FillViewBag();
            return View(model);
		}

		//
		// GET: /Manage/ManageLogins
		public async Task<ActionResult> ManageLogins(ManageMessageId? message) {
            await this.FillViewBag();
            ViewBag.StatusMessage =
				message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
				: message == ManageMessageId.Error ? "An error has occurred."
				: "";
			var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
			if(user == null) {
				return View("Error");
			}
			var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
			var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
			ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
			return View(new ManageLoginsViewModel {
				CurrentLogins = userLogins,
				OtherLogins = otherLogins
			});
		}

		protected override void Dispose(bool disposing) {
			if(disposing && _userManager != null) {
				_userManager.Dispose();
				_userManager = null;
			}

			base.Dispose(disposing);
		}

		[Authorize]
		public async Task<ActionResult> UpdateName() {
			ApplicationUser user = db.Users.Find(User.Identity.GetUserId());

			EditUserNameViewModel model = new EditUserNameViewModel() { UserID = user.Id, FirstName = user.FirstName ?? "Error", LastName = user.LastName ?? "Error"};
			await this.FillViewBag();
            return View(model);
		}

		[Authorize]
		[HttpPost]
		public async Task<ActionResult> UpdateName(string UserID, string FirstName, string LastName) {
			ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
			user.FirstName = FirstName;
			user.LastName = LastName;

			
			await db.SaveChangesAsync();

//			await this.FillViewBag();
			return RedirectToAction("Index");
		}
		#region Helpers
		// Used for XSRF protection when adding external logins
		private const string XsrfKey = "XsrfId";

		private IAuthenticationManager AuthenticationManager {
			get {
				return HttpContext.GetOwinContext().Authentication;
			}
		}

		private void AddErrors(IdentityResult result) {
			foreach(var error in result.Errors) {
				ModelState.AddModelError("", error);
			}
		}

		private bool HasPassword() {
			var user = UserManager.FindById(User.Identity.GetUserId());
			if(user != null) {
				return user.PasswordHash != null;
			}
			return false;
		}

		private bool HasPhoneNumber() {
			var user = UserManager.FindById(User.Identity.GetUserId());
			if(user != null) {
				return user.PhoneNumber != null;
			}
			return false;
		}

		public enum ManageMessageId {
			AddPhoneSuccess,
			ChangePasswordSuccess,
			SetTwoFactorSuccess,
			SetPasswordSuccess,
			RemoveLoginSuccess,
			RemovePhoneSuccess,
			Error
		}

		#endregion
	}
}