using CorporationDepartments.UI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.Services.Implementations
{
	public class ValidationService : IValidationService
	{
		// Регулярные выражения для валидации
		private static readonly Regex EmailRegex = new Regex(
			@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
			RegexOptions.Compiled);

		private static readonly Regex PhoneRegex = new Regex(
			@"^\+?[78][-\(]?\d{3}\)?-?\d{3}-?\d{2}-?\d{2}$",
			RegexOptions.Compiled);

		private static readonly Regex PasswordRegex = new Regex(
			@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
			RegexOptions.Compiled);

		public (bool isValid, string error) ValidateEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return (false, "Email не может быть пустым");

			if (!EmailRegex.IsMatch(email))
				return (false, "Неверный формат email");

			return (true, string.Empty);
		}

		public (bool isValid, string error) ValidatePhoneNumber(string phoneNumber)
		{
			if (string.IsNullOrWhiteSpace(phoneNumber))
				return (false, "Номер телефона не может быть пустым");

			// Удаляем все не цифровые символы для проверки
			var cleanPhone = Regex.Replace(phoneNumber, @"[^\d]", "");

			if (!PhoneRegex.IsMatch(phoneNumber))
				return (false, "Неверный формат номера телефона. Используйте формат: +7(XXX)XXX-XX-XX");

			return (true, string.Empty);
		}

		public (bool isValid, string error) ValidatePassword(string password)
		{
			if (string.IsNullOrWhiteSpace(password))
				return (false, "Пароль не может быть пустым");

			if (password.Length < 8)
				return (false, "Пароль должен содержать минимум 8 символов");

			if (!PasswordRegex.IsMatch(password))
				return (false, "Пароль должен содержать хотя бы одну заглавную букву, одну строчную букву и одну цифру");

			return (true, string.Empty);
		}

		public (bool isValid, string error) ValidateRequiredField(string value, string fieldName)
		{
			if (string.IsNullOrWhiteSpace(value))
				return (false, $"Поле '{fieldName}' обязательно для заполнения");

			return (true, string.Empty);
		}

		public (bool isValid, string error) ValidatePasswordMatch(string password, string confirmPassword)
		{
			if (password != confirmPassword)
				return (false, "Пароли не совпадают");

			return (true, string.Empty);
		}
	}
}
