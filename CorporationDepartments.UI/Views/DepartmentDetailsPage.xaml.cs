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
	/// Логика взаимодействия для DepartmentDetailsPage.xaml
	/// </summary>
	public partial class DepartmentDetailsPage : Page
	{
		private readonly DepartmentDetailsViewModel _viewModel;

		public DepartmentDetailsPage(DepartmentDetailsViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			DataContext = _viewModel;

			// Subscribe to the Unloaded event
			Unloaded += DepartmentDetailsPage_Unloaded;
		}

		private void DepartmentDetailsPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
		{
			// Clean up resources when the page is unloaded
			_viewModel.Dispose();
			Unloaded -= DepartmentDetailsPage_Unloaded;
		}

		public void Initialize(DepartmentViewModel department)
		{
			_viewModel.Initialize(department);
		}
	}
}
