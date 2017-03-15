using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingSite.Models {
	[Table("Orders")]
	public class OrderModel {

		[Key]
		[Required]
		[Display(Name = "Order ID", AutoGenerateField = true)]
		[Column("OrderID")]
		public int OrderID { get; set; }

		[Required]
		[Display(Name = "Order date")]
		[Column("OrderDate")]
		[DataType(DataType.Date)]
		public string OrderDate { get; set; }

		// Status

		[Display(Name = "Paid date")]
		[Column("PaidDate")]
		[DataType(DataType.Date)]
		public string PaidDate { get; set; }

		[Required]
		[Display(Name = "User ID")]
		[Column("ApplicationUserID")]
		public string ApplicationUserID { get; set; }

		[ForeignKey("ApplicationUserID")]
		public virtual ApplicationUser User { get; set; }

		// OrderItem

	}
}