using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ShoppingSite.Models {
	// You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
	public class ApplicationUser : IdentityUser {

		// Id
		// Email
		// PhoneNumber
		// UserName
		// Roles

		[Required]
		[Display(Name = "First name")]
		[RegularExpression("^[a-zA-Z]{2,}$", ErrorMessage = "First name must be at least 2 characters long.")]
		[Column("FirstName", TypeName = "varchar")]
		public string FirstName { get; set; }

		[Required]
		[Display(Name = "Last name")]
		[RegularExpression("^[a-zA-Z]{2,}$", ErrorMessage = "Last name must be at least 2 characters long.")]
		[Column("LastName", TypeName = "varchar")]
		public string LastName { get; set; }

		[Required]
		[Display(Name = "Address")]
		[RegularExpression("^[a-zA-Z]{3,}", ErrorMessage = "Address must be at least 3 characters long.")]
		[Column("Address", TypeName = "varchar")]
		public string Address { get; set; }

		//public virtual ICollection<Order> Orders { get; set; }

		public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager) {
			// Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
			var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
			// Add custom user claims here

			return userIdentity;
		}
	}

	public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
		//public ApplicationDbContext(): base("DefaultConnection", throwIfV1Schema: false){}
		public ApplicationDbContext() : base("ShoppingSiteDBContext", throwIfV1Schema: false) { }

		public static ApplicationDbContext Create() {
			return new ApplicationDbContext();
		}
	}
}