using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ShoppingSite.Models {
	[Table("Products")]
	public class ProductModel {

		[Key]
		[Required]
		[Display(Name = "Product ID", AutoGenerateField = true)]
		[Column("ProductID")]
		public int SKU { get; set; }

		[Required]
		[Display(Name = "Product name")]
		[Column("ProductName", TypeName = "varchar")]
		public string ProductName { get; set; }

		[Required]
		[Display(Name = "Product description")]
		[Column("ProductDescription", TypeName = "varchar")]
		public string Description { get; set; }

		[Required]
		[Display(Name = "CoverPath")]
		[Column("CoverPath", TypeName = "varchar")]
		[DataType(DataType.ImageUrl)]
		public string CoverPath { get; set; }

		[Required]
		[Display(Name = "Price")]
		[Column("Price")]
		[DataType(DataType.Currency)]
		public decimal Price { get; set; }

		//		public string PicturePath { get; set; }

		[Required]
		[Display(Name = "Brand ID")]
		[Column("BrandID")]
		public int BrandID { get; set; }

		[ForeignKey("BrandID")]
		public virtual BrandModel Brand { get; set; }

		public virtual ICollection<SubCategoryModel> ProductCategories { get; set; }
		//		public string Sales { get; set; } 
		//		public string VendorID { get; set; }
		//		public string Orders { get; set; }


	}
}