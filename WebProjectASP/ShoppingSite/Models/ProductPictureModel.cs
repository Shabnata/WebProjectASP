using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingSite.Models {

	[Table("ProductPictures")]
	public class ProductPictureModel {

		[Key]
		[Required]
		[Display(Name = "Picture path")]
		[Column("PicturePath", TypeName = "varchar", Order = 1)]
		[DataType(DataType.Url)]
		public string PicturePath { get; set; }

		[Key]
		[Required]
		[Display(Name = "Product ID")]
		[Column("ProductID", TypeName = "int", Order = 2)]
		[DataType(DataType.Text)]
		public int SKU { get; set; }

		[ForeignKey("SKU")]
		public virtual ProductModel Product { get; set; }
	}
}
