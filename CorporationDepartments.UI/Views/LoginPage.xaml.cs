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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CorporationDepartments.UI.Views
{
	/// <summary>
	/// Логика взаимодействия для LoginPage.xaml
	/// </summary>
	public partial class LoginPage : Page
	{
		private readonly LoginViewModel _viewModel;

		public LoginPage(LoginViewModel viewModel)
		{
			InitializeComponent();

			_viewModel = viewModel;
			DataContext = _viewModel;

			Loaded += (s, e) =>
			{
				if (PasswordBox != null)
				{
					PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
				}
			};
		}

		private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			if (sender is System.Windows.Controls.PasswordBox passwordBox)
			{
				_viewModel.Password = passwordBox.Password;
			}
		}
	}
}
