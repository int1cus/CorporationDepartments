using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.ViewModels
{
	public class DepartmentViewModel : ViewModelBase
	{
		private int _departmentID;
		private string _name;
		private string _description;
		private string _location;
		private string _building;
		private string _floor;
		private decimal? _budget;
		private int? _fiscalYear;
		private int _employeesCount;
		private string _headEmployeeName;
		private int? _headEmployeeID;
		private ObservableCollection<EmployeeViewModel> _employees;
		private ObservableCollection<ProjectViewModel> _projects;

		public int DepartmentID
		{
			get => _departmentID;
			set => SetProperty(ref _departmentID, value);
		}

		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}

		public string Description
		{
			get => _description;
			set => SetProperty(ref _description, value);
		}

		public string Location
		{
			get => _location;
			set => SetProperty(ref _location, value);
		}

		public string Building
		{
			get => _building;
			set => SetProperty(ref _building, value);
		}

		public string Floor
		{
			get => _floor;
			set => SetProperty(ref _floor, value);
		}

		public decimal? Budget
		{
			get => _budget;
			set => SetProperty(ref _budget, value);
		}

		public int? FiscalYear
		{
			get => _fiscalYear;
			set => SetProperty(ref _fiscalYear, value);
		}

		public int EmployeesCount
		{
			get => _employeesCount;
			set => SetProperty(ref _employeesCount, value);
		}

		public string HeadEmployeeName
		{
			get => _headEmployeeName;
			set => SetProperty(ref _headEmployeeName, value);
		}

		public int? HeadEmployeeID
		{
			get => _headEmployeeID;
			set => SetProperty(ref _headEmployeeID, value);
		}

		public ObservableCollection<EmployeeViewModel> Employees
		{
			get => _employees;
			set => SetProperty(ref _employees, value);
		}

		public ObservableCollection<ProjectViewModel> Projects
		{
			get => _projects;
			set => SetProperty(ref _projects, value);
		}

		public DateTime? CreatedDate { get; set; }
		public DateTime? ModifiedDate { get; set; }

		public string BudgetFormatted => Budget.HasValue ? $"{Budget:N0} ₽" : "Не указан";
		public string LocationFormatted => string.IsNullOrEmpty(Location) ? "Не указано" :
			$"{Location}{(string.IsNullOrEmpty(Building) ? "" : $", {Building}")}{(string.IsNullOrEmpty(Floor) ? "" : $", этаж {Floor}")}";

		public DepartmentViewModel()
		{
			Employees = new ObservableCollection<EmployeeViewModel>();
			Projects = new ObservableCollection<ProjectViewModel>();
		}
	}
}
