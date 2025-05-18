using CorporationDepartments.UI.Services.Interfaces;
using CorporationDepartments.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.Services.Implementations
{
	public class EmployeeService : IEmployeeService
	{
		private readonly IDbContextManager _dbContextManager;

		public EmployeeService(IDbContextManager dbContextManager)
		{
			_dbContextManager = dbContextManager ?? throw new ArgumentNullException(nameof(dbContextManager));
		}

		public async Task<List<EmployeeViewModel>> GetAllEmployeesAsync()
		{
			try
			{
				Debug.WriteLine("Получение всех сотрудников: Начало загрузки");
				return await _dbContextManager.ExecuteInContextAsync(async context =>
				{
					// Сначала проверяем, есть ли сотрудники вообще
					var employeeCount = await context.Employees.CountAsync();
					Debug.WriteLine($"Получение всех сотрудников: Общее количество в базе данных: {employeeCount}");

					if (employeeCount == 0)
					{
						Debug.WriteLine("Получение всех сотрудников: Сотрудники не найдены в базе данных");
						return new List<EmployeeViewModel>();
					}

					var employees = await context.Employees
						.Include(e => e.Positions)
						.Include(e => e.EmployeeContacts)
						.ToListAsync();

					Debug.WriteLine($"Получение всех сотрудников: Успешно загружено {employees.Count} сотрудников с их связями");

					var result = employees.Select(e => new EmployeeViewModel
					{
						EmployeeID = e.EmployeeID,
						FullName = $"{e.LastName} {e.FirstName} {e.Patronymic}".Trim(),
						Position = e.Positions?.Title ?? "Не указана",
						Email = e.EmployeeContacts.FirstOrDefault()?.Email ?? "Не указан",
						PhoneNumber = e.EmployeeContacts.FirstOrDefault()?.PhoneNumber ?? "Не указан",
						CreatedDate = e.CreatedDate,
						ModifiedDate = e.ModifiedDate
					}).ToList();

					Debug.WriteLine($"Получение всех сотрудников: Успешно преобразовано {result.Count} сотрудников в модели представления");
					return result;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Получение всех сотрудников ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Получение всех сотрудников СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Получение всех сотрудников ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
					Debug.WriteLine($"Получение всех сотрудников ВНУТРЕННИЙ СТЕК ВЫЗОВОВ: {ex.InnerException.StackTrace}");
				}
				throw;
			}
		}

		public async Task<EmployeeViewModel> GetEmployeeByIdAsync(int id)
		{
			try
			{
				Debug.WriteLine($"Получение сотрудника: Начало загрузки сотрудника с ID {id}");
				return await _dbContextManager.ExecuteInContextAsync(async context =>
				{
					var employee = await context.Employees
						.Include(e => e.Positions)
						.Include(e => e.EmployeeContacts)
						.FirstOrDefaultAsync(e => e.EmployeeID == id);

					if (employee == null)
					{
						Debug.WriteLine($"Получение сотрудника: Сотрудник с ID {id} не найден");
						return null;
					}

					var result = new EmployeeViewModel
					{
						EmployeeID = employee.EmployeeID,
						FullName = $"{employee.LastName} {employee.FirstName} {employee.Patronymic}".Trim(),
						Position = employee.Positions?.Title ?? "Не указана",
						Email = employee.EmployeeContacts.FirstOrDefault()?.Email ?? "Не указан",
						PhoneNumber = employee.EmployeeContacts.FirstOrDefault()?.PhoneNumber ?? "Не указан",
						CreatedDate = employee.CreatedDate,
						ModifiedDate = employee.ModifiedDate
					};

					Debug.WriteLine($"Получение сотрудника: Успешно загружен и преобразован сотрудник с ID {id}");
					return result;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Получение сотрудника ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Получение сотрудника СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Получение сотрудника ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<bool> UpdateEmployeeAsync(EmployeeViewModel employee)
		{
			if (employee == null)
				throw new ArgumentNullException(nameof(employee));

			try
			{
				Debug.WriteLine($"Обновление сотрудника: Начало обновления сотрудника с ID {employee.EmployeeID}");
				return await _dbContextManager.ExecuteInContextWithTransactionAsync(async context =>
				{
					var existingEmployee = await context.Employees
						.Include(e => e.EmployeeContacts)
						.FirstOrDefaultAsync(e => e.EmployeeID == employee.EmployeeID);

					if (existingEmployee == null)
					{
						Debug.WriteLine($"Обновление сотрудника: Сотрудник с ID {employee.EmployeeID} не найден");
						return false;
					}

					// Разбить полное имя на составляющие (предполагается формат: "Фамилия Имя Отчество")
					var nameParts = employee.FullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (nameParts.Length >= 1) existingEmployee.LastName = nameParts[0];
					if (nameParts.Length >= 2) existingEmployee.FirstName = nameParts[1];
					if (nameParts.Length >= 3) existingEmployee.Patronymic = nameParts[2];

					// Обновить контактную информацию
					var contact = existingEmployee.EmployeeContacts.FirstOrDefault();
					if (contact != null)
					{
						contact.Email = employee.Email;
						contact.PhoneNumber = employee.PhoneNumber;
						contact.ModifiedDate = DateTime.Now;
					}

					// Обновить дату модификации
					existingEmployee.ModifiedDate = DateTime.Now;

					await context.SaveChangesAsync();
					Debug.WriteLine($"Обновление сотрудника: Успешно обновлен сотрудник с ID {employee.EmployeeID}");
					return true;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Обновление сотрудника ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Обновление сотрудника СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Обновление сотрудника ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<bool> DeleteEmployeeAsync(int id)
		{
			try
			{
				Debug.WriteLine($"Удаление сотрудника: Начало удаления сотрудника с ID {id}");
				return await _dbContextManager.ExecuteInContextWithTransactionAsync(async context =>
				{
					var employee = await context.Employees
						.Include(e => e.EmployeeContacts)
						.Include(e => e.EmployeeCompensation)
						.Include(e => e.EmployeeDepartments)
						.Include(e => e.EmployeeProfilePictures)
						.Include(e => e.EmployeeProjects)
						.FirstOrDefaultAsync(e => e.EmployeeID == id);

					if (employee == null)
					{
						Debug.WriteLine($"Удаление сотрудника: Сотрудник с ID {id} не найден");
						return false;
					}

					// Удаление связанных записей
					Debug.WriteLine($"Удаление сотрудника: Удаление связанных записей для сотрудника с ID {id}");

					// Удаление контактов
					foreach (var contact in employee.EmployeeContacts.ToList())
					{
						context.EmployeeContacts.Remove(contact);
					}

					// Удаление сведений о компенсации
					foreach (var compensation in employee.EmployeeCompensation.ToList())
					{
						context.EmployeeCompensation.Remove(compensation);
					}

					// Удаление связей с отделами
					foreach (var department in employee.EmployeeDepartments.ToList())
					{
						context.EmployeeDepartments.Remove(department);
					}

					// Удаление фотографий профиля
					foreach (var picture in employee.EmployeeProfilePictures.ToList())
					{
						context.EmployeeProfilePictures.Remove(picture);
					}

					// Удаление связей с проектами
					foreach (var project in employee.EmployeeProjects.ToList())
					{
						context.EmployeeProjects.Remove(project);
					}

					// Сохраняем изменения по удалению связанных записей
					await context.SaveChangesAsync();

					// Теперь удаляем самого сотрудника
					context.Employees.Remove(employee);
					await context.SaveChangesAsync();

					Debug.WriteLine($"Удаление сотрудника: Успешно удален сотрудник с ID {id} со всеми связанными записями");
					return true;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Удаление сотрудника ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Удаление сотрудника СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Удаление сотрудника ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}
	}
}
