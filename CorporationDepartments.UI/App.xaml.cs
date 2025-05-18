using CorporationDepartments.UI.Services;
using CorporationDepartments.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CorporationDepartments.UI
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	public partial class App : Application
	{
		private IServiceProvider _serviceProvider;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			var services = new ServiceCollection();
			ConfigureServices(services);

			_serviceProvider = services.BuildServiceProvider();
			ServiceProviderHelper.Initialize(_serviceProvider);

			try
			{
				// Создание и отображение главного окна
				var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
				mainWindow.Show();
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Ошибка при инициализации приложения: {ex.Message}\n\nDetails: {ex.InnerException?.Message}",
					"Ошибка",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
				Shutdown();
			}
		}

		private void ConfigureServices(IServiceCollection services)
		{
			// Регистрация всех сервисов
			services.AddServices();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			if (_serviceProvider is IDisposable disposable)
			{
				disposable.Dispose();
			}

			base.OnExit(e);
		}
	}
}
