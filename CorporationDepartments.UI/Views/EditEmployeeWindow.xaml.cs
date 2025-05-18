using CorporationDepartments.UI.Services;
using CorporationDepartments.UI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
	/// Логика взаимодействия для EditEmployeeWindow.xaml
	/// </summary>
	public partial class EditEmployeeWindow : Window
	{
		private readonly IValidationService _validationService;

		public EditEmployeeWindow()
		{
			InitializeComponent();
			_validationService = ServiceProviderHelper.GetService<IValidationService>();
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Получаем значения из привязки для дальнейшей валидации
				var fullName = ((dynamic)DataContext).FullName as string;
				var email = ((dynamic)DataContext).Email as string;
				var phoneNumber = ((dynamic)DataContext).PhoneNumber as string;

				// Валидация ФИО
				var fullNameValidation = _validationService.ValidateRequiredField(fullName, "ФИО");
				if (!fullNameValidation.isValid)
				{
					ShowError(fullNameValidation.error);
					return;
				}

				// Дополнительная валидация формата ФИО
				var nameParts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (nameParts.Length < 2)
				{
					ShowError("ФИО должно содержать как минимум фамилию и имя");
					return;
				}

				foreach (var part in nameParts)
				{
					if (part.Length < 2)
					{
						ShowError("Каждая часть ФИО должна содержать не менее 2 символов");
						return;
					}

					if (!Regex.IsMatch(part, @"^[А-Яа-яЁё]+$"))
					{
						ShowError("ФИО должно содержать только кириллические символы");
						return;
					}
				}

				// Валидация Email
				var emailValidation = _validationService.ValidateEmail(email);
				if (!emailValidation.isValid)
				{
					ShowError(emailValidation.error);
					return;
				}

				// Валидация телефона (если указан)
				if (!string.IsNullOrWhiteSpace(phoneNumber))
				{
					var phoneValidation = _validationService.ValidatePhoneNumber(phoneNumber);
					if (!phoneValidation.isValid)
					{
						ShowError(phoneValidation.error);
						return;
					}
				}

				// Если валидация успешна, закрываем с результатом true
				DialogResult = true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Ошибка при сохранении сотрудника: {ex.Message}");
				ShowError($"Произошла ошибка: {ex.Message}");
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void ShowError(string message)
		{
			StatusMessage.Text = message;
			StatusMessage.Foreground = System.Windows.Media.Brushes.Red;
			StatusMessage.Visibility = Visibility.Visible;
		}
	}
}
