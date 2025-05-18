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
	/// Логика взаимодействия для RegisterPage.xaml
	/// </summary>
	public partial class RegisterPage : Page
	{
		private readonly RegisterViewModel _viewModel;

		public RegisterPage(RegisterViewModel viewModel)
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

				if (ConfirmPasswordBox != null)
				{
					ConfirmPasswordBox.PasswordChanged += ConfirmPasswordBox_PasswordChanged;
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

		private void ConfirmPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			if (sender is System.Windows.Controls.PasswordBox passwordBox)
			{
				_viewModel.ConfirmPassword = passwordBox.Password;
			}
		}
	}
}
