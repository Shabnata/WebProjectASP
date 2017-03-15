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
		[Column("ProductID", TypeName = "int")]
		[DataType(DataType.Text)]
		public int SKU { get; set; }

		[Required]
		[Display(Name = "Product name")]
		[Column("ProductName", TypeName = "varchar")]
		[DataType(DataType.Text)]
		public string ProductName { get; set; }

		[Required]
		[Display(Name = "Product description")]
		[Column("ProductDescription", TypeName = "varchar")]
		[DataType(DataType.MultilineText)]
		public string Description { get; set; }

		[Required]
		[Display(Name = "CoverPath")]
		[Column("CoverPath", TypeName = "varchar")]
		[DataType(DataType.ImageUrl)]
		public string CoverPath { get; set; }

		[Required]
		[Display(Name = "Price")]
		[Column("Price", TypeName = "decimal")]
		[DataType(DataType.Currency)]
		public decimal Price { get; set; }

		[Required]
		[Display(Name = "Brand ID")]
		[Column("BrandID", TypeName = "int")]
		[DataType(DataType.Text)]
		public int BrandID { get; set; }

		[ForeignKey("BrandID")]
		public virtual BrandModel Brand { get; set; }

		public virtual ICollection<SubCategoryModel> ProductCategories { get; set; }

		public virtual ICollection<ProductPictureModel> ProductPictures { get; set; }

		public virtual ICollection<SaleModel> Sales { get; set; }

		public virtual ICollection<OrderModel> Orders { get; set; }

	}
}