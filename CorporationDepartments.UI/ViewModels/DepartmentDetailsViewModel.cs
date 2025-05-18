using CorporationDepartments.UI.Commands;
using CorporationDepartments.UI.Services.Interfaces;
using CorporationDepartments.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CorporationDepartments.UI.ViewModels
{
	public class DepartmentDetailsViewModel : ViewModelBase, IDisposable
	{
		private readonly IDepartmentService _departmentService;
		private readonly INavigationService _navigationService;
		private DepartmentViewModel _department;
		private ObservableCollection<EmployeeViewModel> _employees;
		private ObservableCollection<ProjectViewModel> _projects;
		private ObservableCollection<EmployeeViewModel> _availableEmployees;
		private EmployeeViewModel _selectedEmployee;
		private ProjectViewModel _selectedProject;
		private EmployeeViewModel _selectedHeadEmployee;
		private bool _isNewDepartment;
		private bool _isLoading;
		private bool _isEditing;
		private string _loadingMessage;
		private bool _hasChanges;
		private bool _isManagingEmployees;
		private bool _isManagingProjects;
		private bool _isSelectingHead;
		private bool _isEditingAllowed;
		private bool _disposed = false;

		public DepartmentViewModel Department
		{
			get => _department;
			set
			{
				if (_department != null)
				{
					// Unsubscribe from old department's property changed events
					_department.PropertyChanged -= Department_PropertyChanged;
				}

				if (SetProperty(ref _department, value))
				{
					if (_department != null)
					{
						// Subscribe to new department's property changed events
						_department.PropertyChanged += Department_PropertyChanged;
					}

					IsNewDepartment = value != null && value.DepartmentID == 0;
					OnPropertyChanged(nameof(PageTitle));
					OnPropertyChanged(nameof(CanSave));
				}
			}
		}

		// Handler for department property changes
		private void Department_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (IsEditing && !HasChanges)
			{
				HasChanges = true;
			}
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

		public ObservableCollection<EmployeeViewModel> AvailableEmployees
		{
			get => _availableEmployees;
			set => SetProperty(ref _availableEmployees, value);
		}

		public EmployeeViewModel SelectedEmployee
		{
			get => _selectedEmployee;
			set => SetProperty(ref _selectedEmployee, value);
		}

		public ProjectViewModel SelectedProject
		{
			get => _selectedProject;
			set => SetProperty(ref _selectedProject, value);
		}

		public EmployeeViewModel SelectedHeadEmployee
		{
			get => _selectedHeadEmployee;
			set => SetProperty(ref _selectedHeadEmployee, value);
		}

		public bool IsNewDepartment
		{
			get => _isNewDepartment;
			private set => SetProperty(ref _isNewDepartment, value);
		}

		public bool IsLoading
		{
			get => _isLoading;
			set => SetProperty(ref _isLoading, value);
		}

		public bool IsEditing
		{
			get => _isEditing;
			set
			{
				if (SetProperty(ref _isEditing, value))
				{
					OnPropertyChanged(nameof(CanEdit));
					OnPropertyChanged(nameof(CanSave));
					OnPropertyChanged(nameof(CanCancel));
				}
			}
		}

		public bool IsEditingAllowed
		{
			get => _isEditingAllowed;
			set
			{
				if (SetProperty(ref _isEditingAllowed, value))
				{
					OnPropertyChanged(nameof(CanEdit));
					OnPropertyChanged(nameof(CanManageEmployees));
					OnPropertyChanged(nameof(CanManageProjects));
					OnPropertyChanged(nameof(CanSelectHead));
				}
			}
		}

		public bool IsManagingEmployees
		{
			get => _isManagingEmployees;
			set => SetProperty(ref _isManagingEmployees, value);
		}

		public bool IsManagingProjects
		{
			get => _isManagingProjects;
			set => SetProperty(ref _isManagingProjects, value);
		}

		public bool IsSelectingHead
		{
			get => _isSelectingHead;
			set => SetProperty(ref _isSelectingHead, value);
		}

		public string LoadingMessage
		{
			get => _loadingMessage;
			set => SetProperty(ref _loadingMessage, value);
		}

		public bool HasChanges
		{
			get => _hasChanges;
			set
			{
				if (SetProperty(ref _hasChanges, value))
				{
					OnPropertyChanged(nameof(CanSave));
				}
			}
		}

		public bool CanEdit => !IsLoading && !IsEditing && !IsNewDepartment && IsEditingAllowed;
		public bool CanSave => !IsLoading && (IsEditing || IsNewDepartment) && Department != null && !string.IsNullOrWhiteSpace(Department.Name);
		public bool CanCancel => !IsLoading && (IsEditing || IsNewDepartment);
		public bool CanManageEmployees => !IsLoading && IsEditingAllowed;
		public bool CanManageProjects => !IsLoading && IsEditingAllowed;
		public bool CanSelectHead => !IsLoading && IsEditingAllowed;
		public string PageTitle => IsNewDepartment ? "Создание нового отдела" : $"Детали отдела: {Department?.Name}";

		public ICommand SaveCommand { get; }
		public ICommand EditCommand { get; }
		public ICommand CancelCommand { get; }
		public ICommand BackCommand { get; }
		public ICommand LoadEmployeesCommand { get; }
		public ICommand LoadProjectsCommand { get; }
		public ICommand ManageEmployeesCommand { get; }
		public ICommand ManageProjectsCommand { get; }
		public ICommand SelectHeadCommand { get; }
		public ICommand AddEmployeeCommand { get; }
		public ICommand RemoveEmployeeCommand { get; }
		public ICommand AddProjectCommand { get; }
		public ICommand RemoveProjectCommand { get; }
		public ICommand AssignHeadCommand { get; }
		public ICommand CancelManagementCommand { get; }

		public DepartmentDetailsViewModel(IDepartmentService departmentService, INavigationService navigationService)
		{
			_departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
			_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

			Department = new DepartmentViewModel();
			Employees = new ObservableCollection<EmployeeViewModel>();
			Projects = new ObservableCollection<ProjectViewModel>();
			AvailableEmployees = new ObservableCollection<EmployeeViewModel>();
			IsEditingAllowed = true; // По умолчанию разрешено редактирование

			SaveCommand = new AsyncRelayCommand(SaveDepartmentAsync, () => CanSave);
			EditCommand = new RelayCommand(ExecuteEdit, () => CanEdit);
			CancelCommand = new RelayCommand(ExecuteCancel, () => CanCancel);
			BackCommand = new RelayCommand(ExecuteBack);
			LoadEmployeesCommand = new AsyncRelayCommand(LoadEmployeesAsync);
			LoadProjectsCommand = new AsyncRelayCommand(LoadProjectsAsync);
			ManageEmployeesCommand = new AsyncRelayCommand(ManageEmployeesAsync, () => CanManageEmployees);
			ManageProjectsCommand = new AsyncRelayCommand(ManageProjectsAsync, () => CanManageProjects);
			SelectHeadCommand = new AsyncRelayCommand(SelectHeadAsync, () => CanSelectHead);
			AddEmployeeCommand = new AsyncRelayCommand<EmployeeViewModel>(AddEmployeeAsync, e => e != null);
			RemoveEmployeeCommand = new AsyncRelayCommand<EmployeeViewModel>(RemoveEmployeeAsync, e => e != null);
			AddProjectCommand = new AsyncRelayCommand<ProjectViewModel>(AddProjectAsync, p => p != null);
			RemoveProjectCommand = new AsyncRelayCommand<ProjectViewModel>(RemoveProjectAsync, p => p != null);
			AssignHeadCommand = new AsyncRelayCommand<EmployeeViewModel>(AssignHeadAsync, e => e != null);
			CancelManagementCommand = new RelayCommand(ExecuteCancelManagement);
		}

		public void Initialize(DepartmentViewModel department, bool allowEditing = true)
		{
			Department = department ?? new DepartmentViewModel();
			IsEditing = IsNewDepartment;
			IsEditingAllowed = allowEditing;
			HasChanges = false;

			if (!IsNewDepartment)
			{
				// Загружаем сотрудников только для существующего отдела
				LoadEmployeesAsync().ConfigureAwait(false);
				// Загружаем проекты
				LoadProjectsAsync().ConfigureAwait(false);
			}
		}

		private void ExecuteEdit()
		{
			IsEditing = true;
			HasChanges = false;
		}

		private void ExecuteCancel()
		{
			if (IsNewDepartment)
			{
				_navigationService.GoBack();
				return;
			}

			IsEditing = false;
			HasChanges = false;

			// Reload department data to discard changes
			if (Department != null && Department.DepartmentID > 0)
			{
				LoadDepartmentAsync(Department.DepartmentID);
			}
		}

		private void ExecuteBack()
		{
			_navigationService.GoBack();
		}

		private async Task SaveDepartmentAsync()
		{
			try
			{
				IsLoading = true;
				LoadingMessage = "Сохранение изменений...";

				if (IsNewDepartment)
				{
					var departmentId = await _departmentService.CreateDepartmentAsync(Department);
					if (departmentId > 0)
					{
						Department.DepartmentID = departmentId;
						IsNewDepartment = false;
						MessageBox.Show("Отдел успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
					}
				}
				else
				{
					bool success = await _departmentService.UpdateDepartmentAsync(Department);
					if (success)
					{
						MessageBox.Show("Изменения успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
					}
				}

				// Переключаемся в режим просмотра
				IsEditing = false;
				HasChanges = false;

				// Обновляем информацию об отделе после сохранения
				if (!IsNewDepartment)
				{
					var updatedDepartment = await _departmentService.GetDepartmentByIdAsync(Department.DepartmentID);
					if (updatedDepartment != null)
					{
						Department = updatedDepartment;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при сохранении отдела: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка сохранения отдела: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
			finally
			{
				IsLoading = false;
			}
		}

		public async Task LoadDepartmentAsync(int departmentId)
		{
			try
			{
				IsLoading = true;
				LoadingMessage = "Загрузка информации об отделе...";

				var department = await _departmentService.GetDepartmentByIdAsync(departmentId);
				if (department != null)
				{
					Department = department;
					await LoadEmployeesAsync();
					await LoadProjectsAsync();
				}
				else
				{
					MessageBox.Show("Отдел не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
					_navigationService.GoBack();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при загрузке отдела: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка загрузки отдела: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
				_navigationService.GoBack();
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task LoadEmployeesAsync()
		{
			if (Department == null || Department.DepartmentID == 0)
				return;

			try
			{
				IsLoading = true;
				LoadingMessage = "Загрузка сотрудников отдела...";

				var employees = await _departmentService.GetEmployeesInDepartmentAsync(Department.DepartmentID);
				Employees.Clear();
				foreach (var employee in employees)
				{
					Employees.Add(employee);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при загрузке сотрудников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка загрузки сотрудников: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task LoadProjectsAsync()
		{
			if (Department == null || Department.DepartmentID == 0)
				return;

			try
			{
				IsLoading = true;
				LoadingMessage = "Загрузка проектов отдела...";

				var projects = await _departmentService.GetProjectsInDepartmentAsync(Department.DepartmentID);
				Projects.Clear();
				foreach (var project in projects)
				{
					Projects.Add(project);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при загрузке проектов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка загрузки проектов: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task ManageEmployeesAsync()
		{
			if (Department == null || Department.DepartmentID == 0)
				return;

			try
			{
				IsLoading = true;
				LoadingMessage = "Загрузка доступных сотрудников...";

				var allEmployees = await _departmentService.GetAllEmployeesForSelectionAsync();

				// Фильтруем только тех сотрудников, которые не находятся в текущем отделе
				var departmentEmployeeIds = Employees.Select(e => e.EmployeeID).ToHashSet();
				AvailableEmployees.Clear();

				foreach (var employee in allEmployees.Where(e => !departmentEmployeeIds.Contains(e.EmployeeID)))
				{
					AvailableEmployees.Add(employee);
				}

				IsManagingEmployees = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при загрузке доступных сотрудников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка загрузки доступных сотрудников: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task ManageProjectsAsync()
		{
			if (Department == null || Department.DepartmentID == 0)
				return;

			// Navigate to the Add Project page
			_navigationService.NavigateTo("AddProjectPage", Department.DepartmentID);
		}

		private async Task SelectHeadAsync()
		{
			if (Department == null || Department.DepartmentID == 0)
				return;

			try
			{
				IsLoading = true;
				LoadingMessage = "Загрузка сотрудников...";

				var allEmployees = await _departmentService.GetAllEmployeesForSelectionAsync();
				AvailableEmployees.Clear();

				foreach (var employee in allEmployees)
				{
					AvailableEmployees.Add(employee);
				}

				// Предварительно выбираем текущего руководителя, если он есть
				if (Department.HeadEmployeeID.HasValue)
				{
					SelectedHeadEmployee = AvailableEmployees.FirstOrDefault(e => e.EmployeeID == Department.HeadEmployeeID);
				}

				IsSelectingHead = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при загрузке сотрудников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка загрузки сотрудников: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task AddEmployeeAsync(EmployeeViewModel employee)
		{
			try
			{
				IsLoading = true;
				LoadingMessage = "Добавление сотрудника в отдел...";

				bool success = await _departmentService.AddEmployeeToDepartmentAsync(Department.DepartmentID, employee.EmployeeID);
				if (success)
				{
					AvailableEmployees.Remove(employee);
					Employees.Add(employee);
					Department.EmployeesCount = Employees.Count;
					MessageBox.Show("Сотрудник успешно добавлен в отдел.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при добавлении сотрудника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка добавления сотрудника: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task RemoveEmployeeAsync(EmployeeViewModel employee)
		{
			try
			{
				if (MessageBox.Show($"Вы уверены, что хотите удалить сотрудника {employee.FullName} из отдела?",
					"Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
				{
					return;
				}

				IsLoading = true;
				LoadingMessage = "Удаление сотрудника из отдела...";

				bool success = await _departmentService.RemoveEmployeeFromDepartmentAsync(Department.DepartmentID, employee.EmployeeID);
				if (success)
				{
					Employees.Remove(employee);
					Department.EmployeesCount = Employees.Count;

					// Если удаляется руководитель отдела, обновляем информацию
					if (Department.HeadEmployeeID == employee.EmployeeID)
					{
						Department.HeadEmployeeName = "Не назначен";
						Department.HeadEmployeeID = null;
					}

					// Если включен режим управления сотрудниками, добавляем удаленного в список доступных
					if (IsManagingEmployees)
					{
						AvailableEmployees.Add(employee);
					}

					MessageBox.Show("Сотрудник успешно удален из отдела.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при удалении сотрудника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка удаления сотрудника: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task AddProjectAsync(ProjectViewModel project)
		{
			try
			{
				IsLoading = true;
				LoadingMessage = "Добавление проекта в отдел...";

				bool success = await _departmentService.AddProjectToDepartmentAsync(Department.DepartmentID, project);
				if (success)
				{
					// Перезагружаем проекты для обновления информации
					await LoadProjectsAsync();
					MessageBox.Show("Проект успешно добавлен в отдел.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при добавлении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка добавления проекта: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task RemoveProjectAsync(ProjectViewModel project)
		{
			try
			{
				if (MessageBox.Show($"Вы уверены, что хотите удалить проект '{project.Name}' из отдела? Это действие нельзя отменить.",
					"Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
				{
					return;
				}

				IsLoading = true;
				LoadingMessage = "Удаление проекта из отдела...";

				bool success = await _departmentService.RemoveProjectFromDepartmentAsync(Department.DepartmentID, project.ProjectID);
				if (success)
				{
					Projects.Remove(project);
					MessageBox.Show("Проект успешно удален из отдела.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при удалении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка удаления проекта: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task AssignHeadAsync(EmployeeViewModel employee)
		{
			try
			{
				IsLoading = true;
				LoadingMessage = "Назначение руководителя отдела...";

				bool success = await _departmentService.SetDepartmentHeadAsync(Department.DepartmentID, employee.EmployeeID);
				if (success)
				{
					// Обновляем информацию о руководителе отдела
					Department.HeadEmployeeName = employee.FullName;
					Department.HeadEmployeeID = employee.EmployeeID;

					MessageBox.Show($"Сотрудник {employee.FullName} успешно назначен руководителем отдела.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

					// Закрываем экран выбора руководителя
					IsSelectingHead = false;

					// Обновляем список сотрудников, поскольку назначенный руководитель автоматически переводится в отдел
					await LoadEmployeesAsync();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при назначении руководителя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка назначения руководителя: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private void ExecuteCancelManagement()
		{
			IsManagingEmployees = false;
			IsManagingProjects = false;
			IsSelectingHead = false;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// Clean up managed resources
					if (_department != null)
					{
						_department.PropertyChanged -= Department_PropertyChanged;
					}
				}

				// Clean up unmanaged resources if any

				_disposed = true;
			}
		}

		~DepartmentDetailsViewModel()
		{
			Dispose(false);
		}
	}
}
