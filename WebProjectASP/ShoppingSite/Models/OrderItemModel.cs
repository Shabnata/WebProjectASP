using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingSite.Models {
	[Table("OrderItems")]
	public class OrderItemModel {

		[Key]
		[Required]
		[Display(Name = "Order ID")]
		[Column("OrderID")]
		public int OrderID { get; set; }

		[Key]
		[Required]
		[Display(Name = "SKU")]
		[Column("SKU")]
		public int SKU { get; set; }

		[Required]
		[Display(Name = "Quantity")]
		[Column("Quantity")]
		[Range(0, 9999, ErrorMessage = "Quantity must be in the range of 0-9999 items.")]
		public int Quantity { get; set; }

		[ForeignKey("OrderID")]
		public virtual OrderModel Order { get; set; }

		[ForeignKey("SKU")]
		public virtual ProductModel Product { get; set; }
	}
}