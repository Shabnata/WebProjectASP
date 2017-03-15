using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingSite.Models {
	[Table("Brands")]
	public class BrandModel {

		[Key]
		[Required]
		[Display(Name = "Brand ID", AutoGenerateField = true)]
		[Column("BrandID")]
		public int BrandID { get; set; }

		[Required]
		[Display(Name = "Brand name")]
		[Column("BrandName")]
		public string BrandName { get; set; }

		[Required]
		[Display(Name = "Brand logo")]
		[Column("Logo")]
		[DataType(DataType.ImageUrl)]
		public string Logo { get; set; }

		// TODO Add AJAX validator
		[Required]
		[Display(Name = "Country of origin")]
		[Column("Country")]
		public string Country { get; set; }

		[Required]
		[Display(Name = "Brand description")]
		[Column("BrandDescription", TypeName = "varchar")]
		public string Description { get; set; }

		[Required]
		[Display(Name = "Product description")]
		[Column("FoundationYear")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
		public string FoundationYear { get; set; }

		// TODO Check table structure
		public virtual ICollection<ProductModel> Products { get; set; }
	}
}