using CorporationDepartments.UI.Services.Interfaces;
using CorporationDepartments.UI.ViewModels;
using CorporationDepartments.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CorporationDepartments.UI.Services.Implementations
{
	public class NavigationService : INavigationService
	{
		private readonly IServiceProvider _serviceProvider;
		private Frame _mainFrame;

		public event EventHandler<Page> OnPageChanged;

		public NavigationService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public void Initialize(Frame mainFrame)
		{
			_mainFrame = mainFrame ?? throw new ArgumentNullException(nameof(mainFrame));
		}

		public void NavigateToPage<T>() where T : Page
		{
			if (_mainFrame == null)
				throw new InvalidOperationException("Navigation service not initialized. Call Initialize first.");

			try
			{
				var page = _serviceProvider.GetRequiredService<T>();
				NavigateToPage(page);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create page of type {typeof(T).Name}. Make sure it's registered in DI container.", ex);
			}
		}

		public void NavigateToPage(Page page)
		{
			if (_mainFrame == null)
				throw new InvalidOperationException("Navigation service not initialized. Call Initialize first.");

			if (page == null)
				throw new ArgumentNullException(nameof(page));

			_mainFrame.Navigate(page);
			OnPageChanged?.Invoke(this, page);
		}

		public void NavigateTo(string pageName, object parameter = null)
		{
			if (_mainFrame == null)
				throw new InvalidOperationException("Navigation service not initialized. Call Initialize first.");

			switch (pageName)
			{
				case "AddProjectPage":
					var addProjectPage = _serviceProvider.GetRequiredService<AddProjectPage>();
					var addProjectViewModel = _serviceProvider.GetRequiredService<AddProjectViewModel>();

					if (parameter is int departmentId)
					{
						addProjectViewModel.Initialize(departmentId);
					}

					addProjectPage.DataContext = addProjectViewModel;
					NavigateToPage(addProjectPage);
					break;

				case "ProjectsPage":
					var projectsPage = _serviceProvider.GetRequiredService<ProjectsPage>();
					var projectsViewModel = _serviceProvider.GetRequiredService<ProjectsViewModel>();

					projectsPage.DataContext = projectsViewModel;
					NavigateToPage(projectsPage);
					break;

				default:
					throw new ArgumentException($"Page '{pageName}' is not supported for navigation with parameters.");
			}
		}

		public bool CanGoBack => _mainFrame?.CanGoBack ?? false;

		public void GoBack()
		{
			if (_mainFrame == null)
				throw new InvalidOperationException("Navigation service not initialized. Call Initialize first.");

			if (CanGoBack)
			{
				_mainFrame.GoBack();
				if (_mainFrame.Content is Page currentPage)
				{
					OnPageChanged?.Invoke(this, currentPage);
				}
			}
		}
	}
}
