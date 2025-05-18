using CorporationDepartments.UI.Commands;
using CorporationDepartments.UI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CorporationDepartments.UI.ViewModels
{
	public class AddProjectViewModel : ViewModelBase
	{
		private readonly IDepartmentService _departmentService;
		private readonly INavigationService _navigationService;
		private ProjectViewModel _project;
		private bool _isLoading;
		private string _loadingMessage;
		private int _departmentId;

		public ProjectViewModel Project
		{
			get => _project;
			set => SetProperty(ref _project, value);
		}

		public bool IsLoading
		{
			get => _isLoading;
			set => SetProperty(ref _isLoading, value);
		}

		public string LoadingMessage
		{
			get => _loadingMessage;
			set => SetProperty(ref _loadingMessage, value);
		}

		public ICommand SaveCommand { get; }
		public ICommand CancelCommand { get; }
		public ICommand BackCommand { get; }

		public AddProjectViewModel(IDepartmentService departmentService, INavigationService navigationService)
		{
			_departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
			_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

			SaveCommand = new AsyncRelayCommand(SaveProjectAsync, CanSaveProject);
			CancelCommand = new RelayCommand(ExecuteCancel);
			BackCommand = new RelayCommand(ExecuteCancel);

			// Initialize a new project
			Project = new ProjectViewModel
			{
				StartDate = DateTime.Today,
				EndDate = DateTime.Today.AddMonths(3)
			};
		}

		public void Initialize(int departmentId)
		{
			_departmentId = departmentId;
			Project.DepartmentID = departmentId;
		}

		private bool CanSaveProject()
		{
			return !IsLoading && Project != null && !string.IsNullOrWhiteSpace(Project.Name);
		}

		private async Task SaveProjectAsync()
		{
			try
			{
				IsLoading = true;
				LoadingMessage = "Сохранение проекта...";

				bool success = await _departmentService.AddProjectToDepartmentAsync(_departmentId, Project);
				if (success)
				{
					MessageBox.Show("Проект успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
					_navigationService.GoBack();
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

		private void ExecuteCancel()
		{
			_navigationService.GoBack();
		}
	}
}
