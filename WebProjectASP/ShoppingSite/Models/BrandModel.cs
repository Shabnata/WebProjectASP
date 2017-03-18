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
		[Column("BrandID", TypeName = "int")]
		[DataType(DataType.Text)]
		public int BrandID { get; set; }

		[Required]
		[Display(Name = "Brand name")]
		[Column("BrandName", TypeName = "varchar")]
		[DataType(DataType.Text)]
		public string BrandName { get; set; }

		[Required]
		[Display(Name = "Brand logo")]
		[Column("Logo", TypeName = "varchar")]
		[DataType(DataType.ImageUrl)]
		public string Logo { get; set; }

		// TODO Add AJAX validator
		[Required]
		[Display(Name = "Country of origin")]
		[Column("Country", TypeName = "varchar")]
		[DataType(DataType.Text)]
		public string Country { get; set; }

		[Required]
		[Display(Name = "Brand description")]
		[Column("BrandDescription", TypeName = "varchar")]
		[DataType(DataType.MultilineText)]
		public string Description { get; set; }

		[Required]
		[Display(Name = "Foundation Year")]
		[Column("FoundationYear", TypeName = "date")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:y}")]
		public DateTime FoundationYear { get; set; }

		public virtual ICollection<ProductModel> Products { get; set; }
	}
}