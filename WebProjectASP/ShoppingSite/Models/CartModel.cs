using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ShoppingSite.Models {

	[Table("Carts")]
	public class CartModel {

		[Key, ForeignKey("User")]
		[Required]
		[Display(Name = "Cart ID")]
		[Column("CartID", TypeName = "nvarchar")]
		[DataType(DataType.Text)]
		public string CartID { get; set; } // CartID == ApplicationUser.ID

		[ForeignKey("CartID")]
		public virtual ApplicationUser User { get; set; }

		public virtual ICollection<CartItemModel> CartItems { get; set; }
	}

	[Table("CartItems")]
	public class CartItemModel {

		[Key]
		[Required]
		[Column("CartID", TypeName = "nvarchar", Order = 1)]
		[DataType(DataType.Text)]
		public string CartID { get; set; }

		[Key]
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

		[ForeignKey("CartID")]
		public virtual CartModel Cart { get; set; }
	}
}
