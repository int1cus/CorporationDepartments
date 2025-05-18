using CorporationDepartments.UI.Commands;
using CorporationDepartments.UI.Services.Interfaces;
using CorporationDepartments.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CorporationDepartments.UI.ViewModels
{
	public class EmployeesViewModel : ViewModelBase
	{
		private readonly IEmployeeService _employeeService;
		private readonly INavigationService _navigationService;
		private readonly IAuthenticationService _authenticationService;
		private ObservableCollection<EmployeeViewModel> _employees;
		private EmployeeViewModel _selectedEmployee;
		private string _searchText;
		private bool _isLoading;
		private string _loadingMessage;
		private bool _showEmptyState;
		private bool _showEmployees;
		private string _currentUserEmail;

		public ObservableCollection<EmployeeViewModel> Employees
		{
			get => _employees;
			set
			{
				if (SetProperty(ref _employees, value))
				{
					UpdateVisibilityStates();
				}
			}
		}

		public EmployeeViewModel SelectedEmployee
		{
			get => _selectedEmployee;
			set => SetProperty(ref _selectedEmployee, value);
		}

		public string SearchText
		{
			get => _searchText;
			set
			{
				if (SetProperty(ref _searchText, value))
				{
					FilterEmployees();
				}
			}
		}

		public bool IsLoading
		{
			get => _isLoading;
			set
			{
				if (SetProperty(ref _isLoading, value))
				{
					UpdateVisibilityStates();
				}
			}
		}

		public bool ShowEmptyState
		{
			get => _showEmptyState;
			private set => SetProperty(ref _showEmptyState, value);
		}

		public bool ShowEmployees
		{
			get => _showEmployees;
			private set => SetProperty(ref _showEmployees, value);
		}

		public string LoadingMessage
		{
			get => _loadingMessage;
			set => SetProperty(ref _loadingMessage, value);
		}

		public ICommand RefreshCommand { get; }
		public ICommand EditCommand { get; }
		public ICommand DeleteCommand { get; }
		public ICommand NavigateToDashboardCommand { get; }

		private ObservableCollection<EmployeeViewModel> _allEmployees;

		public EmployeesViewModel(
			IEmployeeService employeeService,
			INavigationService navigationService,
			IAuthenticationService authenticationService)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
			_authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));

			_currentUserEmail = _authenticationService.CurrentUserEmail;

			Employees = new ObservableCollection<EmployeeViewModel>();
			_allEmployees = new ObservableCollection<EmployeeViewModel>();

			RefreshCommand = new AsyncRelayCommand(LoadEmployeesAsync);
			EditCommand = new AsyncRelayCommand<EmployeeViewModel>(EditEmployeeAsync, CanEditEmployee);
			DeleteCommand = new AsyncRelayCommand<EmployeeViewModel>(DeleteEmployeeAsync, CanDeleteEmployee);
			NavigateToDashboardCommand = new RelayCommand(ExecuteNavigateToDashboard);

			InitializeAsync();
		}

		private bool CanEditEmployee(EmployeeViewModel employee)
		{
			if (employee == null) return false;
			return employee.Email != _currentUserEmail;
		}

		private bool CanDeleteEmployee(EmployeeViewModel employee)
		{
			if (employee == null) return false;
			return employee.Email != _currentUserEmail;
		}

		private void UpdateVisibilityStates()
		{
			Debug.WriteLine($"Обновление состояний видимости: Загрузка={IsLoading}, Количество сотрудников={Employees?.Count ?? 0}");
			ShowEmptyState = !IsLoading && (Employees == null || !Employees.Any());
			ShowEmployees = !IsLoading && Employees != null && Employees.Any();
			Debug.WriteLine($"Обновление состояний видимости: Пустое состояние={ShowEmptyState}, Показать сотрудников={ShowEmployees}");
		}

		private async void InitializeAsync()
		{
			Debug.WriteLine("Инициализация: Начало загрузки сотрудников");
			await LoadEmployeesAsync();
		}

		private void ExecuteNavigateToDashboard()
		{
			_navigationService.NavigateToPage<DashboardPage>();
		}

		private async Task LoadEmployeesAsync()
		{
			Debug.WriteLine("Загрузка сотрудников: Начало");
			try
			{
				IsLoading = true;
				LoadingMessage = "Загрузка сотрудников...";
				Debug.WriteLine("Загрузка сотрудников: Установлено состояние загрузки");

				var employees = await _employeeService.GetAllEmployeesAsync();
				Debug.WriteLine($"Загрузка сотрудников: Получено {employees?.Count ?? 0} сотрудников из сервиса");

				if (employees != null && employees.Any())
				{
					Debug.WriteLine("Загрузка сотрудников: Обработка данных сотрудников");
					_allEmployees = new ObservableCollection<EmployeeViewModel>(employees);
					FilterEmployees();
				}
				else
				{
					Debug.WriteLine("Загрузка сотрудников: Данные о сотрудниках не получены");
					_allEmployees = new ObservableCollection<EmployeeViewModel>();
					Employees = new ObservableCollection<EmployeeViewModel>();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Загрузка сотрудников ОШИБКА: {ex.Message}\n\nСтек вызовов: {ex.StackTrace}");
				_allEmployees = new ObservableCollection<EmployeeViewModel>();
				Employees = new ObservableCollection<EmployeeViewModel>();
			}
			finally
			{
				IsLoading = false;
				Debug.WriteLine($"Загрузка сотрудников: Завершено. Пустое состояние={ShowEmptyState}, Показать сотрудников={ShowEmployees}");
			}
		}

		private void FilterEmployees()
		{
			try
			{
				Debug.WriteLine("Фильтрация сотрудников: Начало");
				if (string.IsNullOrWhiteSpace(SearchText))
				{
					Employees = new ObservableCollection<EmployeeViewModel>(_allEmployees);
					Debug.WriteLine($"Фильтрация сотрудников: Фильтр не применен. Всего сотрудников: {Employees.Count}");
					return;
				}

				var searchText = SearchText.ToLower();
				var filtered = _allEmployees.Where(e =>
					e.FullName.ToLower().Contains(searchText) ||
					e.Position.ToLower().Contains(searchText) ||
					e.Email.ToLower().Contains(searchText) ||
					(e.PhoneNumber != null && e.PhoneNumber.ToLower().Contains(searchText))
				);

				Employees = new ObservableCollection<EmployeeViewModel>(filtered);
				Debug.WriteLine($"Фильтрация сотрудников: Фильтр применен. Количество после фильтрации: {Employees.Count}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Фильтрация сотрудников ОШИБКА: {ex.Message}\n\nСтек вызовов: {ex.StackTrace}");
			}
		}

		private async Task EditEmployeeAsync(EmployeeViewModel employee)
		{
			if (employee == null || employee.Email == _currentUserEmail) return;
			Debug.WriteLine($"Редактирование сотрудника: Попытка редактирования сотрудника {employee.EmployeeID}");

			try
			{
				// Создаем копию для редактирования
				var employeeToEdit = new EmployeeViewModel
				{
					EmployeeID = employee.EmployeeID,
					FullName = employee.FullName,
					Email = employee.Email,
					PhoneNumber = employee.PhoneNumber,
					Position = employee.Position,
					CreatedDate = employee.CreatedDate,
					ModifiedDate = employee.ModifiedDate
				};

				// Создаем окно редактирования
				var editWindow = new EditEmployeeWindow
				{
					DataContext = employeeToEdit,
					Owner = Application.Current.MainWindow,
					WindowStartupLocation = WindowStartupLocation.CenterOwner
				};

				// Отображаем диалог и обрабатываем результат
				var result = editWindow.ShowDialog();

				if (result == true)
				{
					// Сохраняем изменения в базу данных
					var success = await _employeeService.UpdateEmployeeAsync(employeeToEdit);

					if (success)
					{
						// Обновляем данные в локальных коллекциях
						var index = _allEmployees.IndexOf(employee);
						if (index >= 0)
						{
							_allEmployees[index] = employeeToEdit;
						}

						index = Employees.IndexOf(employee);
						if (index >= 0)
						{
							Employees[index] = employeeToEdit;
						}

						Debug.WriteLine($"Редактирование сотрудника: Успешно отредактирован сотрудник {employee.EmployeeID}");

						// Перезагружаем список сотрудников для обновления всех данных
						await LoadEmployeesAsync();
					}
					else
					{
						MessageBox.Show(
							"Не удалось сохранить изменения.",
							"Ошибка",
							MessageBoxButton.OK,
							MessageBoxImage.Error);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Редактирование сотрудника ОШИБКА: {ex.Message}\n\nСтек вызовов: {ex.StackTrace}");
				MessageBox.Show(
					$"Ошибка при редактировании сотрудника: {ex.Message}",
					"Ошибка",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}

		private async Task DeleteEmployeeAsync(EmployeeViewModel employee)
		{
			if (employee == null || employee.Email == _currentUserEmail) return;

			var result = MessageBox.Show(
				$"Вы уверены, что хотите удалить сотрудника {employee.FullName}?",
				"Подтверждение удаления",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question);

			if (result != MessageBoxResult.Yes) return;

			try
			{
				Debug.WriteLine($"Удаление сотрудника: Попытка удаления сотрудника {employee.EmployeeID}");
				var success = await _employeeService.DeleteEmployeeAsync(employee.EmployeeID);
				if (success)
				{
					_allEmployees.Remove(employee);
					Employees.Remove(employee);
					UpdateVisibilityStates();
					Debug.WriteLine("Удаление сотрудника: Сотрудник успешно удален");
				}
				else
				{
					Debug.WriteLine("Удаление сотрудника: Не удалось удалить сотрудника");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Удаление сотрудника ОШИБКА: {ex.Message}\n\nСтек вызовов: {ex.StackTrace}");
			}
		}
	}
}
