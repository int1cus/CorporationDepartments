using CorporationDepartments.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.Services.Interfaces
{
	public interface IEmployeeService
	{
		Task<List<EmployeeViewModel>> GetAllEmployeesAsync();
		Task<EmployeeViewModel> GetEmployeeByIdAsync(int id);
		Task<bool> UpdateEmployeeAsync(EmployeeViewModel employee);
		Task<bool> DeleteEmployeeAsync(int id);
	}
}
