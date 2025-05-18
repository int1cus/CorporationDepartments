using CorporationDepartments.UI.Services.Interfaces;
using CorporationDepartments.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CorporationDepartments.UI.Views
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly INavigationService _navigationService;

		public MainWindow(INavigationService navigationService)
		{
			InitializeComponent();

			_navigationService = navigationService;

			Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			_navigationService.Initialize(MainFrame);
			_navigationService.OnPageChanged += NavigationService_OnPageChanged;

			// Navigate to login page by default
			_navigationService.NavigateToPage<LoginPage>();
		}

		private void NavigationService_OnPageChanged(object sender, Page e)
		{
			Title = e.Title ?? "Отделы кадров";
		}
	}
}
