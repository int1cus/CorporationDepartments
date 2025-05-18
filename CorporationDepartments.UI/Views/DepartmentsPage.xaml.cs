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
	/// Логика взаимодействия для DepartmentsPage.xaml
	/// </summary>
	public partial class DepartmentsPage : Page
	{
		public DepartmentsPage(DepartmentsViewModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
		}

		private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var viewModel = DataContext as DepartmentsViewModel;
			if (viewModel?.SelectedDepartment != null && viewModel.ViewDetailsCommand.CanExecute(viewModel.SelectedDepartment))
			{
				viewModel.ViewDetailsCommand.Execute(viewModel.SelectedDepartment);
			}
		}
	}
}
