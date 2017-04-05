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
		[Column("SubCategoryID", TypeName ="int")]
		[DataType(DataType.Text)]
		public int SubCategoryID { get; set; }

		[Required]
		[Display(Name = "SubCategory name")]
		[Column("SubCategoryName",TypeName ="varchar")]
		[DataType(DataType.Text)]
		public string SubCategoryName { get; set; }

		[Required]
		[Display(Name = "SubCategory logo")]
		[Column("Logo", TypeName = "varchar")]
		[DataType(DataType.ImageUrl)]
		public string SubCategoryLogo { get; set; }

		public virtual ICollection<ProductModel> Products { get; set; }

		public virtual ICollection<CategoryModel> ParentCategories { get; set; }
	}

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