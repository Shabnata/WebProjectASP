using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingSite.Models {
	[Table("SubCategories")]
	public class SubCategoryModel {

		[Key]
		[Required]
		[Display(Name = "Sub Category ID", AutoGenerateField = true)]
		[Column("SubCategoryID")]
		public int SubCategoryID { get; set; }

		[Required]
		[Display(Name = "SubCategory name")]
		[Column("SubCategoryName")]
		public string SubCategoryName { get; set; }

		// TODO Check table structure
		public virtual ICollection<ProductModel> Products { get; set; }

		// TODO Check table structure
		public virtual ICollection<CategoryModel> ParentCategories { get; set; }
	}
}