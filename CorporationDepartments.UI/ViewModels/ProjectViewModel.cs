using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.ViewModels
{
	public class ProjectViewModel : ViewModelBase
	{
		private int _projectID;
		private string _name;
		private string _description;
		private int? _departmentID;
		private string _departmentName;
		private decimal? _budget;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private int _employeesCount;
		private string _status;

		public int ProjectID
		{
			get => _projectID;
			set => SetProperty(ref _projectID, value);
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

		public int? DepartmentID
		{
			get => _departmentID;
			set => SetProperty(ref _departmentID, value);
		}

		public string DepartmentName
		{
			get => _departmentName;
			set => SetProperty(ref _departmentName, value);
		}

		public decimal? Budget
		{
			get => _budget;
			set => SetProperty(ref _budget, value);
		}

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetProperty(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => SetProperty(ref _endDate, value);
		}

		public int EmployeesCount
		{
			get => _employeesCount;
			set => SetProperty(ref _employeesCount, value);
		}

		public string Status
		{
			get => _status;
			set => SetProperty(ref _status, value);
		}

		public DateTime? CreatedDate { get; set; }
		public DateTime? ModifiedDate { get; set; }

		public string BudgetFormatted => Budget.HasValue ? string.Format("{0:C}", Budget) : "Не указан";

		public string DateRangeFormatted
		{
			get
			{
				if (StartDate.HasValue && EndDate.HasValue)
					return $"{StartDate.Value.ToShortDateString()} - {EndDate.Value.ToShortDateString()}";
				else if (StartDate.HasValue)
					return $"с {StartDate.Value.ToShortDateString()}";
				else if (EndDate.HasValue)
					return $"до {EndDate.Value.ToShortDateString()}";
				else
					return "Не указан";
			}
		}
	}
}
