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

		public virtual ICollection<ProductModel> Products { get; set; }

		public virtual ICollection<CategoryModel> ParentCategories { get; set; }
	}
}