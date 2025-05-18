using CorporationDepartments.UI.Data;
using CorporationDepartments.UI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.Services.Implementations
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly IDbContextManager _dbContextManager;
		private int? _currentUserId;
		private string _currentUserEmail;

		public bool IsAuthenticated => _currentUserId.HasValue;
		public int? CurrentUserId => _currentUserId;
		public string CurrentUserEmail => _currentUserEmail;

		public AuthenticationService(IDbContextManager dbContextManager)
		{
			_dbContextManager = dbContextManager;
		}

		public async Task<bool> LoginAsync(string username, string password)
		{
			var hashedPassword = HashPassword(password);

			var contact = await _dbContextManager.ExecuteInContextAsync(async context =>
				await context.EmployeeContacts
					.FirstOrDefaultAsync(e => e.Username == username && e.PasswordHash == hashedPassword));

			if (contact != null)
			{
				_currentUserId = contact.EmployeeID;
				_currentUserEmail = contact.Email;
				return true;
			}

			return false;
		}

		public async Task<(bool success, string username)> RegisterAsync(
			string firstName,
			string lastName,
			string patronymic,
			string email,
			string phoneNumber,
			string password)
		{
			return await _dbContextManager.ExecuteInContextWithTransactionAsync(async context =>
			{
				// Проверка существования email
				if (await context.EmployeeContacts.AnyAsync(e => e.Email == email))
				{
					return (false, string.Empty);
				}

				// Создаем имя пользователя из email (часть до @)
				var username = email.Split('@')[0];
				var baseUsername = username;
				var counter = 1;

				// Проверка уникальности имени пользователя
				while (await context.EmployeeContacts.AnyAsync(e => e.Username == username))
				{
					username = $"{baseUsername}{counter++}";
				}

				// Создание нового сотрудника
				var employee = new Employees
				{
					FirstName = firstName,
					LastName = lastName,
					Patronymic = patronymic,
					PositionID = await context.Positions
						.Where(p => p.Title == "New Employee")
						.Select(p => p.PositionID)
						.FirstOrDefaultAsync()
				};

				context.Employees.Add(employee);
				await context.SaveChangesAsync();

				// Создание контактной информации
				var contact = new EmployeeContacts
				{
					EmployeeID = employee.EmployeeID,
					Email = email,
					PhoneNumber = phoneNumber,
					Username = username,
					PasswordHash = HashPassword(password)
				};

				context.EmployeeContacts.Add(contact);
				await context.SaveChangesAsync();

				return (true, username);
			});
		}

		public void Logout()
		{
			_currentUserId = null;
			_currentUserEmail = null;
		}

		private string HashPassword(string password)
		{
			using (var sha256 = SHA256.Create())
			{
				var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
				return Convert.ToBase64String(hashedBytes);
			}
		}
	}
}
