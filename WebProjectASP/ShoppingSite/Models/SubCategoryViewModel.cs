using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShoppingSite.Models {
	public class SubCategoryCreateEditViewModel {

		[Required]
		[Display(Name = "Sub Category ID")]
		[DataType(DataType.Text)]
		public int SubCategoryID { get; set; }

		[Required]
		[Display(Name = "SubCategory name")]
		[DataType(DataType.Text)]
		public string SubCategoryName { get; set; }

		[Required]
		[Display(Name = "SubCategory logo")]
		[DataType(DataType.ImageUrl)]
		public string SubCategoryLogo { get; set; }

		[Display(Name = "Parent categories")]
		public IList<CategoryModel> ParentCategories { get; set; }

		[Display(Name = "All categories")]
		public IList<CategoryModel> AllCategories { get; set; }
	}
}