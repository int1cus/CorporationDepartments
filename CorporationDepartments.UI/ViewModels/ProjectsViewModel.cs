using CorporationDepartments.UI.Commands;
using CorporationDepartments.UI.Services.Interfaces;
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
	public class ProjectsViewModel : ViewModelBase
	{
		private readonly IDepartmentService _departmentService;
		private readonly INavigationService _navigationService;
		private ObservableCollection<ProjectViewModel> _projects;
		private ProjectViewModel _selectedProject;
		private bool _isLoading;
		private bool _showEmptyState;
		private bool _showProjects;
		private string _loadingMessage;
		private string _searchQuery;

		public ObservableCollection<ProjectViewModel> Projects
		{
			get => _projects;
			set => SetProperty(ref _projects, value);
		}

		public ProjectViewModel SelectedProject
		{
			get => _selectedProject;
			set => SetProperty(ref _selectedProject, value);
		}

		public bool IsLoading
		{
			get => _isLoading;
			set
			{
				if (SetProperty(ref _isLoading, value))
				{
					UpdateViewState();
				}
			}
		}

		public bool ShowEmptyState
		{
			get => _showEmptyState;
			private set => SetProperty(ref _showEmptyState, value);
		}

		public bool ShowProjects
		{
			get => _showProjects;
			private set => SetProperty(ref _showProjects, value);
		}

		public string LoadingMessage
		{
			get => _loadingMessage;
			set => SetProperty(ref _loadingMessage, value);
		}

		public string SearchQuery
		{
			get => _searchQuery;
			set
			{
				if (SetProperty(ref _searchQuery, value))
				{
					FilterProjects();
				}
			}
		}

		public ICommand RefreshCommand { get; }
		public ICommand ViewDetailsCommand { get; }
		public ICommand AddCommand { get; }
		public ICommand EditCommand { get; }
		public ICommand DeleteCommand { get; }
		public ICommand NavigateToDashboardCommand { get; }

		private ObservableCollection<ProjectViewModel> _allProjects;

		public ProjectsViewModel(IDepartmentService departmentService, INavigationService navigationService)
		{
			_departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
			_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

			Projects = new ObservableCollection<ProjectViewModel>();
			_allProjects = new ObservableCollection<ProjectViewModel>();

			RefreshCommand = new AsyncRelayCommand(LoadProjectsAsync);
			ViewDetailsCommand = new AsyncRelayCommand<ProjectViewModel>(ViewProjectDetailsAsync, p => p != null);
			AddCommand = new RelayCommand(ExecuteAddProject);
			EditCommand = new AsyncRelayCommand<ProjectViewModel>(EditProjectAsync, p => p != null);
			DeleteCommand = new AsyncRelayCommand<ProjectViewModel>(DeleteProjectAsync, p => p != null);
			NavigateToDashboardCommand = new RelayCommand(ExecuteNavigateToDashboard);

			// Initialize data when ViewModel is created
			InitializeAsync();
		}

		private async void InitializeAsync()
		{
			await LoadProjectsAsync();
		}

		private void UpdateViewState()
		{
			ShowEmptyState = !IsLoading && Projects.Count == 0;
			ShowProjects = !IsLoading && Projects.Count > 0;
		}

		private void FilterProjects()
		{
			if (string.IsNullOrWhiteSpace(SearchQuery))
			{
				Projects.Clear();
				foreach (var project in _allProjects)
				{
					Projects.Add(project);
				}
			}
			else
			{
				var query = SearchQuery.ToLower();
				var filtered = _allProjects.Where(p =>
					p.Name.ToLower().Contains(query) ||
					p.Description?.ToLower().Contains(query) == true ||
					p.DepartmentName?.ToLower().Contains(query) == true ||
					p.Status?.ToLower().Contains(query) == true
				).ToList();

				Projects.Clear();
				foreach (var project in filtered)
				{
					Projects.Add(project);
				}
			}

			UpdateViewState();
		}

		public async Task LoadProjectsAsync()
		{
			try
			{
				IsLoading = true;
				LoadingMessage = "Загрузка проектов...";

				var projects = await _departmentService.GetAllProjectsAsync();

				_allProjects.Clear();
				Projects.Clear();

				foreach (var project in projects)
				{
					_allProjects.Add(project);
					Projects.Add(project);
				}

				FilterProjects();
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

		private async Task ViewProjectDetailsAsync(ProjectViewModel project)
		{
			if (project == null) return;

			try
			{
				// Создаем страницу деталей проекта
				MessageBox.Show("Просмотр деталей проекта пока не реализован", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

				// Здесь будет навигация к странице деталей проекта
				// _navigationService.NavigateTo("ProjectDetailsPage", project.ProjectID);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при открытии деталей проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка при открытии деталей проекта: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
		}

		private void ExecuteAddProject()
		{
			try
			{
				// При нажатии на кнопку Добавить переходим на страницу создания проекта
				_navigationService.NavigateTo("AddProjectPage", null);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при создании проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка при создании проекта: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
		}

		private async Task EditProjectAsync(ProjectViewModel project)
		{
			if (project == null) return;

			try
			{
				// Здесь будет переход к странице редактирования проекта
				MessageBox.Show("Редактирование проекта пока не реализовано", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

				// _navigationService.NavigateTo("EditProjectPage", project.ProjectID);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при редактировании проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка при редактировании проекта: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
		}

		private async Task DeleteProjectAsync(ProjectViewModel project)
		{
			if (project == null) return;

			try
			{
				if (MessageBox.Show($"Вы уверены, что хотите удалить проект '{project.Name}'?",
					"Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					IsLoading = true;
					LoadingMessage = "Удаление проекта...";

					bool success = await _departmentService.DeleteProjectAsync(project.ProjectID);
					if (success)
					{
						_allProjects.Remove(project);
						Projects.Remove(project);
						UpdateViewState();

						MessageBox.Show("Проект успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при удалении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine($"Ошибка при удалении проекта: {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private void ExecuteNavigateToDashboard()
		{
			_navigationService.NavigateToPage<Views.DashboardPage>();
		}
	}
}
