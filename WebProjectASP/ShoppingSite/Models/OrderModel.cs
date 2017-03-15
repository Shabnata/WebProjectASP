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
		[Column("OrderID",TypeName ="int")]
		[DataType(DataType.Text)]
		public int OrderID { get; set; }

		[Required]
		[Display(Name = "Order date")]
		[Column("OrderDate", TypeName = "date")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
		public DateTime OrderDate { get; set; }

		[Required]
		[Display(Name = "Order status")]
		[Column("Status", TypeName = "varchar")]
		[DataType(DataType.Text)]
		public string Status { get; set; }

		[Display(Name = "Paid date")]
		[Column("PaidDate", TypeName = "date")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}")]
		public DateTime PaidDate { get; set; }

		[Required]
		[Display(Name = "User ID")]
		[Column("ApplicationUserID", TypeName = "nvarchar")]
		[DataType(DataType.Text)]
		public string ApplicationUserID { get; set; }

		[ForeignKey("ApplicationUserID")]
		public virtual ApplicationUser User { get; set; }

		public virtual ICollection<OrderItemModel> OrderItems { get; set; }

	}
}