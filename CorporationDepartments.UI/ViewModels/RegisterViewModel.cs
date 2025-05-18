using CorporationDepartments.UI.Commands;
using CorporationDepartments.UI.Services.Interfaces;
using CorporationDepartments.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CorporationDepartments.UI.ViewModels
{
	public class RegisterViewModel : ViewModelBase
	{
		private readonly IAuthenticationService _authService;
		private readonly IValidationService _validationService;
		private readonly INavigationService _navigationService;
		private string _firstName;
		private string _lastName;
		private string _patronymic;
		private string _email;
		private string _phoneNumber;
		private string _password;
		private string _confirmPassword;
		private string _errorMessage;
		private bool _isLoading;

		public string FirstName
		{
			get => _firstName;
			set => SetProperty(ref _firstName, value);
		}

		public string LastName
		{
			get => _lastName;
			set => SetProperty(ref _lastName, value);
		}

		public string Patronymic
		{
			get => _patronymic;
			set => SetProperty(ref _patronymic, value);
		}

		public string Email
		{
			get => _email;
			set => SetProperty(ref _email, value);
		}

		public string PhoneNumber
		{
			get => _phoneNumber;
			set => SetProperty(ref _phoneNumber, value);
		}

		public string Password
		{
			get => _password;
			set => SetProperty(ref _password, value);
		}

		public string ConfirmPassword
		{
			get => _confirmPassword;
			set => SetProperty(ref _confirmPassword, value);
		}

		public string ErrorMessage
		{
			get => _errorMessage;
			set => SetProperty(ref _errorMessage, value);
		}

		public bool IsLoading
		{
			get => _isLoading;
			set => SetProperty(ref _isLoading, value);
		}

		public ICommand RegisterCommand { get; }
		public ICommand NavigateToLoginCommand { get; }

		public RegisterViewModel(
			IAuthenticationService authService,
			IValidationService validationService,
			INavigationService navigationService)
		{
			_authService = authService;
			_validationService = validationService;
			_navigationService = navigationService;

			RegisterCommand = new AsyncRelayCommand(ExecuteRegisterAsync, CanExecuteRegister);
			NavigateToLoginCommand = new RelayCommand(ExecuteNavigateToLogin);
		}

		private bool CanExecuteRegister()
		{
			return !string.IsNullOrWhiteSpace(FirstName) &&
				   !string.IsNullOrWhiteSpace(LastName) &&
				   !string.IsNullOrWhiteSpace(Email) &&
				   !string.IsNullOrWhiteSpace(Password) &&
				   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
				   !IsLoading;
		}

		private bool ValidateInput()
		{
			// Проверка обязательных полей
			var lastNameValidation = _validationService.ValidateRequiredField(LastName, "Фамилия");
			if (!lastNameValidation.isValid)
			{
				ErrorMessage = lastNameValidation.error;
				return false;
			}

			var firstNameValidation = _validationService.ValidateRequiredField(FirstName, "Имя");
			if (!firstNameValidation.isValid)
			{
				ErrorMessage = firstNameValidation.error;
				return false;
			}

			// Валидация email
			var emailValidation = _validationService.ValidateEmail(Email);
			if (!emailValidation.isValid)
			{
				ErrorMessage = emailValidation.error;
				return false;
			}

			// Валидация номера телефона
			var phoneValidation = _validationService.ValidatePhoneNumber(PhoneNumber);
			if (!phoneValidation.isValid)
			{
				ErrorMessage = phoneValidation.error;
				return false;
			}

			// Валидация пароля
			var passwordValidation = _validationService.ValidatePassword(Password);
			if (!passwordValidation.isValid)
			{
				ErrorMessage = passwordValidation.error;
				return false;
			}

			// Проверка совпадения паролей
			var passwordMatchValidation = _validationService.ValidatePasswordMatch(Password, ConfirmPassword);
			if (!passwordMatchValidation.isValid)
			{
				ErrorMessage = passwordMatchValidation.error;
				return false;
			}

			return true;
		}

		private async Task ExecuteRegisterAsync()
		{
			try
			{
				IsLoading = true;
				ErrorMessage = string.Empty;

				if (!ValidateInput())
				{
					return;
				}

				var (success, username) = await _authService.RegisterAsync(
					FirstName,
					LastName,
					Patronymic,
					Email,
					PhoneNumber,
					Password);

				if (success)
				{
					// После успешной регистрации выполняем вход и переходим на главную страницу
					if (await _authService.LoginAsync(username, Password))
					{
						_navigationService.NavigateToPage<DashboardPage>();
					}
				}
				else
				{
					ErrorMessage = "Пользователь с таким email уже существует";
				}
			}
			finally
			{
				IsLoading = false;
			}
		}

		private void ExecuteNavigateToLogin()
		{
			NavigateToLogin();
		}

		private void NavigateToLogin()
		{
			_navigationService.NavigateToPage<LoginPage>();
		}
	}
}
