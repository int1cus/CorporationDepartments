using CorporationDepartments.UI.Commands;
using CorporationDepartments.UI.Services;
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
	public class DepartmentsViewModel : ViewModelBase
	{
		private readonly IDepartmentService _departmentService;
		private readonly INavigationService _navigationService;
		private ObservableCollection<DepartmentViewModel> _departments;
		private DepartmentViewModel _selectedDepartment;
		private string _searchText;
		private bool _isLoading;
		private string _loadingMessage;
		private bool _showEmptyState;
		private bool _showDepartments;

		public ObservableCollection<DepartmentViewModel> Departments
		{
			get => _departments;
			set
			{
				if (SetProperty(ref _departments, value))
				{
					UpdateVisibilityStates();
				}
			}
		}

		public DepartmentViewModel SelectedDepartment
		{
			get => _selectedDepartment;
			set => SetProperty(ref _selectedDepartment, value);
		}

		public string SearchText
		{
			get => _searchText;
			set
			{
				if (SetProperty(ref _searchText, value))
				{
					FilterDepartments();
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

		public bool ShowDepartments
		{
			get => _showDepartments;
			private set => SetProperty(ref _showDepartments, value);
		}

		public string LoadingMessage
		{
			get => _loadingMessage;
			set => SetProperty(ref _loadingMessage, value);
		}

		public ICommand RefreshCommand { get; }
		public ICommand ViewDetailsCommand { get; }
		public ICommand AddCommand { get; }
		public ICommand EditCommand { get; }
		public ICommand DeleteCommand { get; }
		public ICommand NavigateToDashboardCommand { get; }

		private ObservableCollection<DepartmentViewModel> _allDepartments;

		public DepartmentsViewModel(IDepartmentService departmentService, INavigationService navigationService)
		{
			_departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
			_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

			Departments = new ObservableCollection<DepartmentViewModel>();
			_allDepartments = new ObservableCollection<DepartmentViewModel>();

			RefreshCommand = new AsyncRelayCommand(LoadDepartmentsAsync);
			ViewDetailsCommand = new AsyncRelayCommand<DepartmentViewModel>(ViewDepartmentDetailsAsync, d => d != null);
			AddCommand = new RelayCommand(ExecuteAddDepartment);
			EditCommand = new AsyncRelayCommand<DepartmentViewModel>(EditDepartmentAsync, d => d != null);
			DeleteCommand = new AsyncRelayCommand<DepartmentViewModel>(DeleteDepartmentAsync, d => d != null);
			NavigateToDashboardCommand = new RelayCommand(ExecuteNavigateToDashboard);

			InitializeAsync();
		}

		private void UpdateVisibilityStates()
		{
			Debug.WriteLine($"Обновление состояний видимости: Загрузка={IsLoading}, Количество отделов={Departments?.Count ?? 0}");
			ShowEmptyState = !IsLoading && (Departments == null || !Departments.Any());
			ShowDepartments = !IsLoading && Departments != null && Departments.Any();
			Debug.WriteLine($"Обновление состояний видимости: Пустое состояние={ShowEmptyState}, Показать отделы={ShowDepartments}");
		}

		private async void InitializeAsync()
		{
			Debug.WriteLine("Инициализация: Начало загрузки отделов");
			await LoadDepartmentsAsync();
		}

		private void ExecuteNavigateToDashboard()
		{
			_navigationService.NavigateToPage<DashboardPage>();
		}

		private async Task LoadDepartmentsAsync()
		{
			Debug.WriteLine("Загрузка отделов: Начало");
			try
			{
				IsLoading = true;
				LoadingMessage = "Загрузка отделов...";
				Debug.WriteLine("Загрузка отделов: Установлено состояние загрузки");

				var departments = await _departmentService.GetAllDepartmentsAsync();
				Debug.WriteLine($"Загрузка отделов: Получено {departments?.Count ?? 0} отделов из сервиса");

				if (departments != null && departments.Any())
				{
					Debug.WriteLine("Загрузка отделов: Обработка данных отделов");
					_allDepartments = new ObservableCollection<DepartmentViewModel>(departments);
					FilterDepartments();
				}
				else
				{
					Debug.WriteLine("Загрузка отделов: Данные об отделах не получены");
					_allDepartments = new ObservableCollection<DepartmentViewModel>();
					Departments = new ObservableCollection<DepartmentViewModel>();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Загрузка отделов ОШИБКА: {ex.Message}\n\nСтек вызовов: {ex.StackTrace}");
				_allDepartments = new ObservableCollection<DepartmentViewModel>();
				Departments = new ObservableCollection<DepartmentViewModel>();
			}
			finally
			{
				IsLoading = false;
				Debug.WriteLine($"Загрузка отделов: Завершено. Пустое состояние={ShowEmptyState}, Показать отделы={ShowDepartments}");
			}
		}

		private void FilterDepartments()
		{
			try
			{
				Debug.WriteLine("Фильтрация отделов: Начало");
				if (string.IsNullOrWhiteSpace(SearchText))
				{
					Departments = new ObservableCollection<DepartmentViewModel>(_allDepartments);
					Debug.WriteLine($"Фильтрация отделов: Фильтр не применен. Всего отделов: {Departments.Count}");
					return;
				}

				var searchText = SearchText.ToLower();
				var filtered = _allDepartments.Where(d =>
					d.Name.ToLower().Contains(searchText) ||
					(d.Description != null && d.Description.ToLower().Contains(searchText)) ||
					(d.Location != null && d.Location.ToLower().Contains(searchText)) ||
					(d.Building != null && d.Building.ToLower().Contains(searchText))
				);

				Departments = new ObservableCollection<DepartmentViewModel>(filtered);
				Debug.WriteLine($"Фильтрация отделов: Фильтр применен. Количество после фильтрации: {Departments.Count}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Фильтрация отделов ОШИБКА: {ex.Message}\n\nСтек вызовов: {ex.StackTrace}");
			}
		}

		private async Task ViewDepartmentDetailsAsync(DepartmentViewModel department)
		{
			if (department == null) return;

			// Получаем актуальные данные об отделе
			try
			{
				var updatedDepartment = await _departmentService.GetDepartmentByIdAsync(department.DepartmentID);
				if (updatedDepartment != null)
				{
					// Создаем страницу и инициализируем её вручную
					var detailsPage = ServiceProviderHelper.GetService<DepartmentDetailsPage>();
					var detailsViewModel = ServiceProviderHelper.GetService<DepartmentDetailsViewModel>();

					// Инициализируем ViewModel в режиме просмотра (без возможности редактирования)
					detailsViewModel.Initialize(updatedDepartment, false);

					// Устанавливаем ViewModel как DataContext для страницы
					detailsPage.DataContext = detailsViewModel;

					// Навигация к странице деталей отдела
					_navigationService.NavigateToPage(detailsPage);
				}
				else
				{
					MessageBox.Show("Не удалось загрузить информацию об отделе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Просмотр деталей отдела ОШИБКА: {ex.Message}\n\nСтек вызовов: {ex.StackTrace}");
				MessageBox.Show($"Произошла ошибка при загрузке данных отдела: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void ExecuteAddDepartment()
		{
			var newDepartment = new DepartmentViewModel();

			// Создаем страницу и инициализируем её вручную
			var detailsPage = ServiceProviderHelper.GetService<DepartmentDetailsPage>();
			var detailsViewModel = ServiceProviderHelper.GetService<DepartmentDetailsViewModel>();

			// Инициализируем ViewModel с возможностью редактирования
			detailsViewModel.Initialize(newDepartment, true);

			// Устанавливаем ViewModel как DataContext для страницы
			detailsPage.DataContext = detailsViewModel;

			// Навигация к странице деталей отдела
			_navigationService.NavigateToPage(detailsPage);
		}

		private async Task EditDepartmentAsync(DepartmentViewModel department)
		{
			if (department == null) return;

			try
			{
				// Получаем актуальные данные об отделе перед редактированием
				var departmentToEdit = await _departmentService.GetDepartmentByIdAsync(department.DepartmentID);
				if (departmentToEdit != null)
				{
					// Создаем страницу и инициализируем её вручную
					var detailsPage = ServiceProviderHelper.GetService<DepartmentDetailsPage>();
					var detailsViewModel = ServiceProviderHelper.GetService<DepartmentDetailsViewModel>();

					// Инициализируем ViewModel с возможностью редактирования
					detailsViewModel.Initialize(departmentToEdit, true);

					// Устанавливаем режим редактирования
					detailsViewModel.IsEditing = true;

					// Устанавливаем ViewModel как DataContext для страницы
					detailsPage.DataContext = detailsViewModel;

					// Навигация к странице деталей отдела
					_navigationService.NavigateToPage(detailsPage);
				}
				else
				{
					MessageBox.Show("Не удалось загрузить информацию об отделе для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Ошибка при редактировании отдела: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
				MessageBox.Show($"Произошла ошибка при попытке редактирования отдела: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async Task DeleteDepartmentAsync(DepartmentViewModel department)
		{
			if (department == null) return;

			var result = MessageBox.Show(
				$"Вы уверены, что хотите удалить отдел '{department.Name}'?",
				"Подтверждение удаления",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question);

			if (result != MessageBoxResult.Yes) return;

			try
			{
				Debug.WriteLine($"Удаление отдела: Попытка удаления отдела {department.DepartmentID}");
				var success = await _departmentService.DeleteDepartmentAsync(department.DepartmentID);
				if (success)
				{
					_allDepartments.Remove(department);
					Departments.Remove(department);
					UpdateVisibilityStates();
					Debug.WriteLine("Удаление отдела: Отдел успешно удален");
				}
				else
				{
					Debug.WriteLine("Удаление отдела: Не удалось удалить отдел");
				}
			}
			catch (InvalidOperationException ex)
			{
				Debug.WriteLine($"Удаление отдела ОШИБКА: {ex.Message}\n\nСтек вызовов: {ex.StackTrace}");
				MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Удаление отдела ОШИБКА: {ex.Message}\n\nСтек вызовов: {ex.StackTrace}");
				MessageBox.Show($"Произошла ошибка при удалении отдела: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
