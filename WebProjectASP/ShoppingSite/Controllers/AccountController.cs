using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ShoppingSite.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Collections.Generic;

namespace ShoppingSite.Controllers {
	[Authorize]
	public class AccountController : Controller {
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

        public AccountController() {
		}

		public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) {
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
		// GET: /Account/Login
		[AllowAnonymous]
		public async Task<ActionResult> Login(string returnUrl) {
			ViewBag.ReturnUrl = returnUrl;
            await this.FillViewBag();
            return View();
		}

		//
		// POST: /Account/Login
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Login(LoginViewModel model, string returnUrl) {
			if(!ModelState.IsValid) {
                await this.FillViewBag();
                return View(model);
			}

			// This doesn't count login failures towards account lockout
			// To enable password failures to trigger account lockout, change to shouldLockout: true
			var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            await this.FillViewBag();
            switch (result) {
				case SignInStatus.Success:
					await MergeCarts(model.Email);
                    return RedirectToLocal(returnUrl);
				case SignInStatus.LockedOut:
					return View("Lockout");
				case SignInStatus.RequiresVerification:
					return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
				case SignInStatus.Failure:
				default:
					ModelState.AddModelError("", "Invalid login attempt.");
					return View(model);
			}
		}

		private async Task<Boolean> MergeCarts(string email) {
			IList<CartItemModel> guestCartItems = Session["GuestCartItems"] as IList<CartItemModel> ?? new List<CartItemModel>();
			ApplicationUser user = await (from u in db.Users where u.Email.Equals(email) select u).SingleAsync();

			foreach(CartItemModel cim in guestCartItems) {
				CartItemModel updateModel = null;
				
				if((from c in user.CartItems where c.SKU == cim.SKU select c).Any()) {
					updateModel = (from c in user.CartItems where c.SKU == cim.SKU select c).Single();
					updateModel.Quantity += cim.Quantity;
					user.CartItems.Add(updateModel);
				} else {
					updateModel = new CartItemModel() { SKU = cim.SKU, Quantity = cim.Quantity, ApplicationUserID = user.Id };
					user.CartItems.Add(updateModel);
				}
				updateModel = null;
			}

			Session["GuestCartItems"] = null;
			await db.SaveChangesAsync();
			return true;
		}

		//
		// GET: /Account/Register
		[AllowAnonymous]
		public async Task<ActionResult> Register() {
            await this.FillViewBag();
            return View();
		}

		//
		// POST: /Account/Register
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Register(RegisterViewModel model) {
			if(ModelState.IsValid) {
				var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, Address = model.Address, FirstName = model.FirstName, LastName = model.LastName };

				ApplicationDbContext dbContext = new ApplicationDbContext();
				IdentityRole customerRole = (from r in dbContext.Roles where r.Name == "Customer" select r).Single();
				user.Roles.Add(new IdentityUserRole() { UserId = user.Id, RoleId = customerRole.Id });

				var result = await UserManager.CreateAsync(user, model.Password);
				if(result.Succeeded) {
					await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

					// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
					// Send an email with this link
					// string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
					// var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
					// await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

					return RedirectToAction("Index", "Home");
				}
				AddErrors(result);
			}

            // If we got this far, something failed, redisplay form
            await this.FillViewBag();
            return View(model);
		}

		//
		// GET: /Account/ForgotPassword
		[AllowAnonymous]
        public async Task<ActionResult> ForgotPassword() {
            await this.FillViewBag();
            return View();
		}

		//
		// POST: /Account/ForgotPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model) {
            await this.FillViewBag();
            if (ModelState.IsValid) {
				var user = await UserManager.FindByNameAsync(model.Email);
				if(user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id))) {
					// Don't reveal that the user does not exist or is not confirmed
					return View("ForgotPasswordConfirmation");
				}

				// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
				// Send an email with this link
				// string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
				// var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
				// await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
				// return RedirectToAction("ForgotPasswordConfirmation", "Account");
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		//
		// GET: /Account/ForgotPasswordConfirmation
		[AllowAnonymous]
		public async Task<ActionResult> ForgotPasswordConfirmation() {
            await this.FillViewBag();
            return View();
		}

		//
		// GET: /Account/ResetPassword
		[AllowAnonymous]
		public async Task<ActionResult> ResetPassword(string code) {
            await this.FillViewBag();
            return code == null ? View("Error") : View();
		}

		//
		// POST: /Account/ResetPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model) {
            await this.FillViewBag();
            if (!ModelState.IsValid) {
				return View(model);
			}
			var user = await UserManager.FindByNameAsync(model.Email);
			if(user == null) {
				// Don't reveal that the user does not exist
				return RedirectToAction("ResetPasswordConfirmation", "Account");
			}
			var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
			if(result.Succeeded) {
				return RedirectToAction("ResetPasswordConfirmation", "Account");
			}
			AddErrors(result);
			return View();
		}

		//
		// GET: /Account/ResetPasswordConfirmation
		[AllowAnonymous]
		public async Task<ActionResult> ResetPasswordConfirmation() {
            await this.FillViewBag();
            return View();
		}

		//
		// POST: /Account/LogOff
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> LogOff() {
			AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            await this.FillViewBag();
            return RedirectToAction("Index", "Home");
		}

		protected override void Dispose(bool disposing) {
			if(disposing) {
				if(_userManager != null) {
					_userManager.Dispose();
					_userManager = null;
				}

				if(_signInManager != null) {
					_signInManager.Dispose();
					_signInManager = null;
				}
			}

			base.Dispose(disposing);
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

		private ActionResult RedirectToLocal(string returnUrl) {
			if(Url.IsLocalUrl(returnUrl)) {
				return Redirect(returnUrl);
			}
			return RedirectToAction("Index", "Home");
		}

		internal class ChallengeResult : HttpUnauthorizedResult {
			public ChallengeResult(string provider, string redirectUri)
				: this(provider, redirectUri, null) {
			}

			public ChallengeResult(string provider, string redirectUri, string userId) {
				LoginProvider = provider;
				RedirectUri = redirectUri;
				UserId = userId;
			}

			public string LoginProvider { get; set; }
			public string RedirectUri { get; set; }
			public string UserId { get; set; }

			public override void ExecuteResult(ControllerContext context) {
				var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
				if(UserId != null) {
					properties.Dictionary[XsrfKey] = UserId;
				}
				context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
			}
		}
		#endregion
	}
}