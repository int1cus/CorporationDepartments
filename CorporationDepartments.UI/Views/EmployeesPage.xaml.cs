﻿using CorporationDepartments.UI.ViewModels;
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
	/// Логика взаимодействия для EmployeesPage.xaml
	/// </summary>
	public partial class EmployeesPage : Page
	{
		public EmployeesPage(EmployeesViewModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
		}
	}
}
