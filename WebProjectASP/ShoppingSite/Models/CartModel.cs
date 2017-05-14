using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ShoppingSite.Models {

	public class CartViewModel {

		public ApplicationUser User { get; set; }
		public ICollection<CartItemModel> CartItems { get; set; }
		public decimal TotalPrice { get; set; }
		public IDictionary<CartItemModel,SaleModel> CartItemsSales { get; set; }
	}

	[Table("CartItems")]
	public class CartItemModel {

		[Key, ForeignKey("Customer")]
		[Required]
		[Column("ApplicationUserID", TypeName = "nvarchar", Order = 1)]
		[DataType(DataType.Text)]
		public string ApplicationUserID { get; set; }

		[Key, ForeignKey("Product")]
		[Required]
		[Display(Name = "SKU")]
		[Column("SKU", TypeName = "int", Order = 2)]
		[DataType(DataType.Text)]
		public int SKU { get; set; }

		[Required]
		[Display(Name = "Quantity")]
		[Column("Quantity", TypeName = "int")]
		[DataType(DataType.Text)]
		[Range(0, 9999, ErrorMessage = "Quantity must be in the range of 0-9999 items.")]
		public int Quantity { get; set; }

		[ForeignKey("SKU")]
		public virtual ProductModel Product { get; set; }

		[ForeignKey("ApplicationUserID")]
		public virtual ApplicationUser Customer { get; set; }

	}

}
