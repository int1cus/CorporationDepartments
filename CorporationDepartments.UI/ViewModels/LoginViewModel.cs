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
	public class LoginViewModel : ViewModelBase
	{
		private readonly IAuthenticationService _authService;
		private readonly IValidationService _validationService;
		private readonly INavigationService _navigationService;
		private string _username;
		private string _password;
		private string _errorMessage;
		private bool _isLoading;


		public string Username
		{
			get => _username;
			set => SetProperty(ref _username, value);
		}

		public string Password
		{
			get => _password;
			set => SetProperty(ref _password, value);
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

		public ICommand LoginCommand { get; }
		public ICommand NavigateToRegisterCommand { get; }

		public LoginViewModel(
			IAuthenticationService authService,
			IValidationService validationService,
			INavigationService navigationService)
		{
			_authService = authService;
			_validationService = validationService;
			_navigationService = navigationService;

			LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync, CanExecuteLogin);
			NavigateToRegisterCommand = new RelayCommand(ExecuteNavigateToRegister);
		}

		private bool CanExecuteLogin()
		{
			return !string.IsNullOrWhiteSpace(Username) &&
				   !string.IsNullOrWhiteSpace(Password) &&
				   !IsLoading;
		}

		private bool ValidateInput()
		{
			// Проверка имени пользователя
			var usernameValidation = _validationService.ValidateRequiredField(Username, "Имя пользователя");
			if (!usernameValidation.isValid)
			{
				ErrorMessage = usernameValidation.error;
				return false;
			}

			// Проверка пароля
			var passwordValidation = _validationService.ValidateRequiredField(Password, "Пароль");
			if (!passwordValidation.isValid)
			{
				ErrorMessage = passwordValidation.error;
				return false;
			}

			return true;
		}

		private async Task ExecuteLoginAsync()
		{
			try
			{
				IsLoading = true;
				ErrorMessage = string.Empty;

				if (!ValidateInput())
				{
					return;
				}

				if (await _authService.LoginAsync(Username, Password))
				{
					NavigateToDashboard();
				}
				else
				{
					ErrorMessage = "Неверно введены имя пользователя или пароль";
				}
			}
			finally
			{
				IsLoading = false;
			}
		}

		private void ExecuteNavigateToRegister()
		{
			_navigationService.NavigateToPage<RegisterPage>();
		}

		private void NavigateToDashboard()
		{
			_navigationService.NavigateToPage<DashboardPage>();
		}
	}
}
