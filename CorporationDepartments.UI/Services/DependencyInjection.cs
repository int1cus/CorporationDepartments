using CorporationDepartments.UI.Converters;
using CorporationDepartments.UI.Services.Implementations;
using CorporationDepartments.UI.Services.Interfaces;
using CorporationDepartments.UI.ViewModels;
using CorporationDepartments.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.Services
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddServices(this IServiceCollection services)
		{
			// Основные сервисы - регистрируем первыми, так как другие сервисы зависят от них
			services.AddSingleton<IDbContextManager, DbContextManager>();
			services.AddSingleton<IValidationService, ValidationService>();
			services.AddSingleton<IAuthenticationService, AuthenticationService>();
			services.AddSingleton<INavigationService, NavigationService>();

			// ViewModels - регистрируем перед соответствующими страницами
			services.AddTransient<LoginViewModel>();
			services.AddTransient<RegisterViewModel>();
			services.AddTransient<DashboardViewModel>();
			services.AddTransient<EmployeesViewModel>();
			services.AddTransient<DepartmentsViewModel>();
			services.AddTransient<DepartmentDetailsViewModel>();
			services.AddTransient<AddProjectViewModel>();
			services.AddTransient<ProjectsViewModel>();

			// Страницы - регистрируем после их ViewModels
			services.AddTransient<LoginPage>();
			services.AddTransient<RegisterPage>();
			services.AddTransient<DashboardPage>();
			services.AddTransient<EmployeesPage>();
			services.AddTransient<DepartmentsPage>();
			services.AddTransient<DepartmentDetailsPage>();
			services.AddTransient<AddProjectPage>();
			services.AddTransient<ProjectsPage>();

			// Главное окно - регистрируем последним, так как оно зависит от сервиса навигации
			services.AddSingleton<MainWindow>();

			// Сервисы для работы с данными
			services.AddTransient<IEmployeeService, EmployeeService>();
			services.AddTransient<IDepartmentService, DepartmentService>();

			return services;
		}
	}
}
