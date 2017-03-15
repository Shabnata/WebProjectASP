using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingSite.Models {
	[Table("Categories")]
	public class CategoryModel {

		[Key]
		[Required]
		[Display(Name = "Category ID", AutoGenerateField = true)]
		[Column("CategoryID")]
		public int CategoryID { get; set; }

		[Required]
		[Display(Name = "Category name")]
		[Column("CategoryName")]
		public string CategoryName { get; set; }

		// TODO Check table structure
		public virtual ICollection<SubCategoryModel> SubCategories { get; set; }
	}
}