using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.ViewModels
{
	public class EmployeeViewModel
	{
		public int EmployeeID { get; set; }
		public string FullName { get; set; }
		public string Position { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public DateTime? CreatedDate { get; set; }
		public DateTime? ModifiedDate { get; set; }
	}
}
