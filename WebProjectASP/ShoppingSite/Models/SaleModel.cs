using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingSite.Models {
	[Table("Sales")]
	public class SaleModel {

		[Key]
		[Required]
		[Display(Name = "Sale ID",AutoGenerateField = true)]
		[Column("SaleID",TypeName = "int")]
		[DataType(DataType.Text)]
		public int SaleID { get; set; }

		[Required]
		[Display(Name = "Sale name")]
		[Column("SaleName",TypeName = "varchar")]
		[DataType(DataType.Text)]
		public string SaleName { get; set; }

		[Required]
		[Display(Name = "Start date")]
		[Column("StartDate", TypeName = "date")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}")]
		public DateTime StartDate { get; set; }

		[Required]
		[Display(Name = "End date")]
		[Column("EndDate", TypeName = "date")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}")]
		public DateTime EndDate { get; set; }

		[Required]
		[Display(Name = "Discount %")]
		[Column("Discount", TypeName = "decimal")]
		[DataType(DataType.Text)]
		public decimal Discount { get; set; }

		public virtual ICollection<ProductModel> Products { get; set; }

		[Required]
		[Display(Name ="Emblem")]
		[Column("Emblem", TypeName = "varchar")]
		[DataType(DataType.ImageUrl)]
		public string Emblem { get; set; }
	}

	public class SaleEditViewModel {
		
		[Required]
		[Display(Name = "Sale ID")]
		[DataType(DataType.Text)]
		public int SaleID { get; set; }

		[Required]
		[Display(Name = "Sale name")]
		[DataType(DataType.Text)]
		public string SaleName { get; set; }

		[Required]
		[Display(Name = "Start date")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}")]
		public DateTime StartDate { get; set; }

		[Required]
		[Display(Name = "End date")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}")]
		public DateTime EndDate { get; set; }

		[Required]
		[Display(Name = "Discount %")]
		[DataType(DataType.Text)]
		public decimal Discount { get; set; }

		[Required]
		[Display(Name ="Emblem")]
		[Column("Emblem", TypeName = "varchar")]
		[DataType(DataType.ImageUrl)]
		public string Emblem { get; set; }

		public IList<ProductModel> ProductsOnSale { get; set; }

		public IList<BrandModel> AllBrands { get; set; }

		public IList<SubCategoryModel> AllSubCategories { get; set; }

		public IList<ProductModel> AllProducts { get; set; }
	}

	public class SaleBrowseViewModel {

		public IList<SaleModel> AllActiveSales { get; set; }
		public SaleModel ThisSale { get; set; }
	}
}