using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System;
using System.Data.SqlClient;

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
		[DataType(DataType.Text)]
		public string FirstName { get; set; }

		[Required]
		[Display(Name = "Last name")]
		[RegularExpression("^[a-zA-Z]{2,}$", ErrorMessage = "Last name must be at least 2 characters long.")]
		[Column("LastName", TypeName = "varchar")]
		[DataType(DataType.Text)]
		public string LastName { get; set; }

		[Required]
		[Display(Name = "Address")]
		[RegularExpression("^[a-zA-Z]{3,}.*", ErrorMessage = "Address must be at least 3 characters long.")]
		[Column("Address", TypeName = "varchar")]
		[DataType(DataType.Text)]
		public string Address { get; set; }

		public virtual ICollection<OrderModel> Orders { get; set; }

		public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager) {
			// Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
			var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
			// Add custom user claims here

			return userIdentity;
		}
	}

	public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
		public ApplicationDbContext() : base("ShoppingSiteDBContext", throwIfV1Schema: false) { }

		// Users
		public virtual DbSet<BrandModel> Brands { get; set; }
		public virtual DbSet<CategoryModel> Categories { get; set; }
		public virtual DbSet<SubCategoryModel> SubCategories { get; set; }
		public virtual DbSet<OrderItemModel> OrderItems { get; set; }
		public virtual DbSet<OrderModel> Orders { get; set; }
		public virtual DbSet<ProductModel> Products { get; set; }
		public virtual DbSet<ProductPictureModel> ProductPictures { get; set; }
		public virtual DbSet<SaleModel> Sales { get; set; }

		public static ApplicationDbContext Create() {
			return new ApplicationDbContext();
		}

		public async Task<IList<SaleModel>>  GetActiveSalesAsync() {
            DateTime today = DateTime.Now;
            List<SaleModel> activeSales = await (from s in this.Sales where s.StartDate <= today && s.EndDate >= today select s).ToListAsync();

			return activeSales;
		}

		public async Task<IList<SubCategoryModel>> GetBrandSubCategoriesAsync(int BrandID) {
			List<SubCategoryModel> brandSubCategories = new List<SubCategoryModel>();
			BrandModel brand = null;
			try {
				brand = await this.Brands.FindAsync(BrandID);
				Boolean flag = false;
				foreach(ProductModel pm in brand.Products) {
					foreach(SubCategoryModel pcm in pm.ProductCategories) {
						flag = false;
						foreach(SubCategoryModel scm in brandSubCategories) {
							if(pcm.SubCategoryID == scm.SubCategoryID) {
								flag = true;
								break;
							}
						}
						if(!flag) {
							brandSubCategories.Add(pcm);
						}
					}
				}
			}catch(SqlException ex) {

			}
			return brandSubCategories;
		}

		public async Task<IList<ProductModel>> GetCategoryProductsAsync(int CategoryID) {
			List<ProductModel> categoryProducts = new List<ProductModel>();

			CategoryModel category = await this.Categories.FindAsync(CategoryID);
			Boolean flag = false;
			foreach(SubCategoryModel scm in category.SubCategories) {
				foreach(ProductModel scpm in scm.Products) {
					flag = false;
					foreach(ProductModel pm in categoryProducts) {
						if(scpm.SKU == pm.SKU) {
							flag = true;
							break;
						}
					}
					if(!flag) {
						categoryProducts.Add(scpm);
					}
				}
			}
			return categoryProducts;
		}
	}
}