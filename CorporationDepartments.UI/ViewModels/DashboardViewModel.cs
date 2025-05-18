using CorporationDepartments.UI.Commands;
using CorporationDepartments.UI.Services.Interfaces;
using CorporationDepartments.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CorporationDepartments.UI.ViewModels
{
	public class DashboardViewModel : ViewModelBase
	{
		private readonly IAuthenticationService _authenticationService;
		private readonly INavigationService _navigationService;
		private readonly IEmployeeService _employeeService;
		private readonly IDepartmentService _departmentService;
		private string _welcomeMessage;
		private int _totalEmployees;
		private int _totalDepartments;
		private bool _isLoading;

		public string WelcomeMessage
		{
			get => _welcomeMessage;
			set => SetProperty(ref _welcomeMessage, value);
		}

		public int TotalEmployees
		{
			get => _totalEmployees;
			set => SetProperty(ref _totalEmployees, value);
		}

		public int TotalDepartments
		{
			get => _totalDepartments;
			set => SetProperty(ref _totalDepartments, value);
		}

		public bool IsLoading
		{
			get => _isLoading;
			set => SetProperty(ref _isLoading, value);
		}

		public ICommand LogoutCommand { get; }
		public ICommand NavigateToEmployeesCommand { get; }
		public ICommand NavigateToDepartmentsCommand { get; }
		public ICommand NavigateToProjectsCommand { get; }
		public ICommand RefreshCommand { get; }

		public DashboardViewModel(
			IAuthenticationService authenticationService,
			INavigationService navigationService,
			IEmployeeService employeeService,
			IDepartmentService departmentService)
		{
			_authenticationService = authenticationService;
			_navigationService = navigationService;
			_employeeService = employeeService;
			_departmentService = departmentService;

			LogoutCommand = new RelayCommand(ExecuteLogout);
			NavigateToEmployeesCommand = new RelayCommand(ExecuteNavigateToEmployees);
			NavigateToDepartmentsCommand = new RelayCommand(ExecuteNavigateToDepartments);
			NavigateToProjectsCommand = new RelayCommand(ExecuteNavigateToProjects);
			RefreshCommand = new AsyncRelayCommand(LoadDataAsync);

			Initialize();
		}

		private async void Initialize()
		{
			if (!_authenticationService.IsAuthenticated)
			{
				_navigationService.NavigateToPage<LoginPage>();
				return;
			}

			WelcomeMessage = $"Добро пожаловать в систему управления отделами кадров!";
			await LoadDataAsync();
		}

		private async Task LoadDataAsync()
		{
			try
			{
				IsLoading = true;
				var employees = await _employeeService.GetAllEmployeesAsync();
				TotalEmployees = employees?.Count ?? 0;

				var departments = await _departmentService.GetAllDepartmentsAsync();
				TotalDepartments = departments?.Count ?? 0;
			}
			catch
			{
				TotalEmployees = 0;
				TotalDepartments = 0;
			}
			finally
			{
				IsLoading = false;
			}
		}

		private void ExecuteLogout()
		{
			_authenticationService.Logout();
			_navigationService.NavigateToPage<LoginPage>();
		}

		private void ExecuteNavigateToEmployees()
		{
			_navigationService.NavigateToPage<EmployeesPage>();
		}

		private void ExecuteNavigateToDepartments()
		{
			_navigationService.NavigateToPage<DepartmentsPage>();
		}

		private void ExecuteNavigateToProjects()
		{
			_navigationService.NavigateToPage<ProjectsPage>();
		}
	}
}
