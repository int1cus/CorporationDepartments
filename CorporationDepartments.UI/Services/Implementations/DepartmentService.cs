using CorporationDepartments.UI.Data;
using CorporationDepartments.UI.Services.Interfaces;
using CorporationDepartments.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace CorporationDepartments.UI.Services.Implementations
{
	public class DepartmentService : IDepartmentService
	{
		private readonly IDbContextManager _dbContextManager;

		public DepartmentService(IDbContextManager dbContextManager)
		{
			_dbContextManager = dbContextManager ?? throw new ArgumentNullException(nameof(dbContextManager));
		}

		public async Task<List<DepartmentViewModel>> GetAllDepartmentsAsync()
		{
			try
			{
				Debug.WriteLine("Получение всех отделов: Начало загрузки");
				return await _dbContextManager.ExecuteInContextAsync(async context =>
				{
					// Сначала проверяем, есть ли отделы вообще
					var departmentCount = await context.Departments.CountAsync();
					Debug.WriteLine($"Получение всех отделов: Общее количество в базе данных: {departmentCount}");

					if (departmentCount == 0)
					{
						Debug.WriteLine("Получение всех отделов: Отделы не найдены в базе данных");
						return new List<DepartmentViewModel>();
					}

					var departments = await context.Departments
						.Include(d => d.DepartmentLocations)
						.Include(d => d.DepartmentBudgets)
						.Include(d => d.Employees)
						.ToListAsync();

					Debug.WriteLine($"Получение всех отделов: Успешно загружено {departments.Count} отделов с их связями");

					var result = departments.Select(d => MapToViewModel(d, context)).ToList();
					Debug.WriteLine($"Получение всех отделов: Успешно преобразовано {result.Count} отделов в модели представления");
					return result;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Получение всех отделов ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Получение всех отделов СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Получение всех отделов ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<DepartmentViewModel> GetDepartmentByIdAsync(int id)
		{
			try
			{
				Debug.WriteLine($"Получение отдела: Начало загрузки отдела с ID {id}");
				return await _dbContextManager.ExecuteInContextAsync(async context =>
				{
					var department = await context.Departments
						.Include(d => d.DepartmentLocations)
						.Include(d => d.DepartmentBudgets)
						.Include(d => d.Employees)
						.FirstOrDefaultAsync(d => d.DepartmentID == id);

					if (department == null)
					{
						Debug.WriteLine($"Получение отдела: Отдел с ID {id} не найден");
						return null;
					}

					var result = MapToViewModel(department, context);
					Debug.WriteLine($"Получение отдела: Успешно загружен и преобразован отдел с ID {id}");
					return result;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Получение отдела ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Получение отдела СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Получение отдела ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<int> CreateDepartmentAsync(DepartmentViewModel department)
		{
			if (department == null)
				throw new ArgumentNullException(nameof(department));

			try
			{
				Debug.WriteLine("Создание отдела: Начало создания нового отдела");
				return await _dbContextManager.ExecuteInContextWithTransactionAsync(async context =>
				{
					// Проверка на уникальность названия отдела
					var existingDepartment = await context.Departments
						.FirstOrDefaultAsync(d => d.Name == department.Name);

					if (existingDepartment != null)
					{
						Debug.WriteLine($"Создание отдела: Отдел с названием '{department.Name}' уже существует");
						throw new InvalidOperationException($"Отдел с названием '{department.Name}' уже существует");
					}

					// Создание нового отдела
					var newDepartment = new Departments
					{
						Name = department.Name,
						Description = department.Description,
						CreatedDate = DateTime.Now,
						ModifiedDate = DateTime.Now
					};

					context.Departments.Add(newDepartment);
					await context.SaveChangesAsync();

					// Добавление информации о местоположении, если указано
					if (!string.IsNullOrWhiteSpace(department.Location) ||
						!string.IsNullOrWhiteSpace(department.Building) ||
						!string.IsNullOrWhiteSpace(department.Floor))
					{
						var location = new DepartmentLocations
						{
							DepartmentID = newDepartment.DepartmentID,
							Location = department.Location,
							Building = department.Building,
							Floor = department.Floor,
							CreatedDate = DateTime.Now,
							ModifiedDate = DateTime.Now
						};

						context.DepartmentLocations.Add(location);
					}

					// Добавление информации о бюджете, если указано
					if (department.Budget.HasValue)
					{
						var budget = new DepartmentBudgets
						{
							DepartmentID = newDepartment.DepartmentID,
							Budget = department.Budget,
							FiscalYear = department.FiscalYear ?? DateTime.Now.Year,
							LastReviewDate = DateTime.Now,
							CreatedDate = DateTime.Now,
							ModifiedDate = DateTime.Now
						};

						context.DepartmentBudgets.Add(budget);
					}

					await context.SaveChangesAsync();
					Debug.WriteLine($"Создание отдела: Успешно создан новый отдел с ID {newDepartment.DepartmentID}");

					return newDepartment.DepartmentID;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Создание отдела ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Создание отдела СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Создание отдела ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<bool> UpdateDepartmentAsync(DepartmentViewModel department)
		{
			if (department == null)
				throw new ArgumentNullException(nameof(department));

			try
			{
				Debug.WriteLine($"Обновление отдела: Начало обновления отдела с ID {department.DepartmentID}");
				return await _dbContextManager.ExecuteInContextWithTransactionAsync(async context =>
				{
					// Проверка на уникальность названия отдела
					var nameConflict = await context.Departments
						.FirstOrDefaultAsync(d => d.Name == department.Name && d.DepartmentID != department.DepartmentID);

					if (nameConflict != null)
					{
						Debug.WriteLine($"Обновление отдела: Отдел с названием '{department.Name}' уже существует");
						throw new InvalidOperationException($"Отдел с названием '{department.Name}' уже существует");
					}

					// Получаем существующий отдел
					var existingDepartment = await context.Departments
						.Include(d => d.DepartmentLocations)
						.Include(d => d.DepartmentBudgets)
						.FirstOrDefaultAsync(d => d.DepartmentID == department.DepartmentID);

					if (existingDepartment == null)
					{
						Debug.WriteLine($"Обновление отдела: Отдел с ID {department.DepartmentID} не найден");
						return false;
					}

					// Обновляем основную информацию
					existingDepartment.Name = department.Name;
					existingDepartment.Description = department.Description;
					existingDepartment.ModifiedDate = DateTime.Now;

					// Обновляем местоположение
					var location = existingDepartment.DepartmentLocations.FirstOrDefault();
					if (location != null)
					{
						location.Location = department.Location;
						location.Building = department.Building;
						location.Floor = department.Floor;
						location.ModifiedDate = DateTime.Now;
					}
					else if (!string.IsNullOrWhiteSpace(department.Location) ||
							 !string.IsNullOrWhiteSpace(department.Building) ||
							 !string.IsNullOrWhiteSpace(department.Floor))
					{
						// Создаем новое местоположение, если его нет
						var newLocation = new DepartmentLocations
						{
							DepartmentID = department.DepartmentID,
							Location = department.Location,
							Building = department.Building,
							Floor = department.Floor,
							CreatedDate = DateTime.Now,
							ModifiedDate = DateTime.Now
						};
						context.DepartmentLocations.Add(newLocation);
					}

					// Обновляем бюджет
					var budget = existingDepartment.DepartmentBudgets
						.OrderByDescending(b => b.FiscalYear)
						.FirstOrDefault();

					if (budget != null && department.Budget.HasValue)
					{
						// Если бюджет существует и новое значение указано, обновляем
						budget.Budget = department.Budget;
						budget.FiscalYear = department.FiscalYear ?? DateTime.Now.Year;
						budget.LastReviewDate = DateTime.Now;
						budget.ModifiedDate = DateTime.Now;
					}
					else if (department.Budget.HasValue)
					{
						// Создаем новый бюджет, если его нет или не указан новый
						var newBudget = new DepartmentBudgets
						{
							DepartmentID = department.DepartmentID,
							Budget = department.Budget,
							FiscalYear = department.FiscalYear ?? DateTime.Now.Year,
							LastReviewDate = DateTime.Now,
							CreatedDate = DateTime.Now,
							ModifiedDate = DateTime.Now
						};
						context.DepartmentBudgets.Add(newBudget);
					}

					await context.SaveChangesAsync();
					Debug.WriteLine($"Обновление отдела: Успешно обновлен отдел с ID {department.DepartmentID}");
					return true;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Обновление отдела ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Обновление отдела СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Обновление отдела ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<bool> DeleteDepartmentAsync(int id)
		{
			try
			{
				Debug.WriteLine($"Удаление отдела: Начало удаления отдела с ID {id}");
				return await _dbContextManager.ExecuteInContextWithTransactionAsync(async context =>
				{
					var department = await context.Departments
						.Include(d => d.DepartmentLocations)
						.Include(d => d.DepartmentBudgets)
						.Include(d => d.EmployeeDepartments)
						.Include(d => d.Employees)
						.FirstOrDefaultAsync(d => d.DepartmentID == id);

					if (department == null)
					{
						Debug.WriteLine($"Удаление отдела: Отдел с ID {id} не найден");
						return false;
					}

					// Проверяем, есть ли сотрудники в отделе
					if (department.Employees.Any())
					{
						Debug.WriteLine($"Удаление отдела: Невозможно удалить отдел с ID {id}, так как в нем есть сотрудники");
						throw new InvalidOperationException("Невозможно удалить отдел, в котором есть сотрудники. Переведите сотрудников в другой отдел перед удалением.");
					}

					// Удаляем связанные записи
					Debug.WriteLine($"Удаление отдела: Удаление связанных записей для отдела с ID {id}");

					// Удаляем местоположения
					foreach (var location in department.DepartmentLocations.ToList())
					{
						context.DepartmentLocations.Remove(location);
					}

					// Удаляем бюджеты
					foreach (var budget in department.DepartmentBudgets.ToList())
					{
						context.DepartmentBudgets.Remove(budget);
					}

					// Удаляем истории принадлежности к отделам
					foreach (var deptHistory in department.EmployeeDepartments.ToList())
					{
						context.EmployeeDepartments.Remove(deptHistory);
					}

					// Сохраняем изменения перед удалением самого отдела
					await context.SaveChangesAsync();

					// Удаляем отдел
					context.Departments.Remove(department);
					await context.SaveChangesAsync();

					Debug.WriteLine($"Удаление отдела: Успешно удален отдел с ID {id}");
					return true;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Удаление отдела ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Удаление отдела СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Удаление отдела ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<List<EmployeeViewModel>> GetEmployeesInDepartmentAsync(int departmentId)
		{
			try
			{
				Debug.WriteLine($"Получение сотрудников отдела: Начало загрузки для отдела с ID {departmentId}");
				return await _dbContextManager.ExecuteInContextAsync(async context =>
				{
					var employees = await context.Employees
						.Include(e => e.Positions)
						.Include(e => e.EmployeeContacts)
						.Where(e => e.CurrentDepartmentID == departmentId)
						.ToListAsync();

					Debug.WriteLine($"Получение сотрудников отдела: Найдено {employees.Count} сотрудников");

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

					return result;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Получение сотрудников отдела ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Получение сотрудников отдела СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Получение сотрудников отдела ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		// Вспомогательный метод для преобразования модели данных в ViewModel
		private DepartmentViewModel MapToViewModel(Departments department, CorporationDepartmentsEntities context)
		{
			var location = department.DepartmentLocations.FirstOrDefault();
			var budget = department.DepartmentBudgets.OrderByDescending(b => b.FiscalYear).FirstOrDefault();

			// Находим руководителя отдела - теперь используем прямую связь
			string headEmployeeName = "Не назначен";
			int? headEmployeeId = department.HeadEmployeeID;
			try
			{
				var headEmployee = headEmployeeId.HasValue
					? department.HeadEmployee ?? context.Employees.FirstOrDefault(e => e.EmployeeID == headEmployeeId.Value)
					: null;

				if (headEmployee != null)
				{
					headEmployeeName = $"{headEmployee.LastName} {headEmployee.FirstName} {headEmployee.Patronymic}".Trim();
					headEmployeeId = headEmployee.EmployeeID;
				}
			}
			catch
			{
				// Если что-то пошло не так, используем значение по умолчанию
			}

			return new DepartmentViewModel
			{
				DepartmentID = department.DepartmentID,
				Name = department.Name,
				Description = department.Description,
				Location = location?.Location,
				Building = location?.Building,
				Floor = location?.Floor,
				Budget = budget?.Budget,
				FiscalYear = budget?.FiscalYear,
				EmployeesCount = department.Employees.Count,
				HeadEmployeeName = headEmployeeName,
				HeadEmployeeID = headEmployeeId,
				CreatedDate = department.CreatedDate,
				ModifiedDate = department.ModifiedDate
			};
		}

		// Вспомогательный метод для преобразования проекта в ViewModel
		private ProjectViewModel MapProjectToViewModel(Projects project)
		{
			var timeline = project.ProjectTimelines.OrderBy(t => t.StartDate).FirstOrDefault();
			var budget = project.ProjectBudgets.OrderByDescending(b => b.CreatedDate).FirstOrDefault();

			return new ProjectViewModel
			{
				ProjectID = project.ProjectID,
				Name = project.Name,
				Description = project.Description,
				DepartmentID = project.DepartmentID,
				Budget = budget?.Budget,
				StartDate = timeline?.StartDate,
				EndDate = timeline?.EndDate,
				EmployeesCount = project.EmployeeProjects.Count,
				Status = DetermineProjectStatus(timeline?.StartDate, timeline?.EndDate),
				CreatedDate = project.CreatedDate,
				ModifiedDate = project.ModifiedDate
			};
		}

		// Вспомогательный метод для определения статуса проекта
		private string DetermineProjectStatus(DateTime? startDate, DateTime? endDate)
		{
			if (!startDate.HasValue || !endDate.HasValue)
				return "Не определен";

			var today = DateTime.Today;
			if (today < startDate)
				return "Запланирован";
			if (today > endDate)
				return "Завершен";
			return "В процессе";
		}

		public async Task<List<ProjectViewModel>> GetProjectsInDepartmentAsync(int departmentId)
		{
			try
			{
				Debug.WriteLine($"Получение проектов отдела: Начало загрузки для отдела с ID {departmentId}");
				return await _dbContextManager.ExecuteInContextAsync(async context =>
				{
					var projects = await context.Projects
						.Include(p => p.ProjectBudgets)
						.Include(p => p.ProjectTimelines)
						.Include(p => p.EmployeeProjects)
						.Where(p => p.DepartmentID == departmentId)
						.ToListAsync();

					Debug.WriteLine($"Получение проектов отдела: Найдено {projects.Count} проектов");

					var result = projects.Select(p => MapProjectToViewModel(p)).ToList();
					return result;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Получение проектов отдела ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Получение проектов отдела СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Получение проектов отдела ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<List<EmployeeViewModel>> GetAllEmployeesForSelectionAsync()
		{
			try
			{
				Debug.WriteLine("Получение всех сотрудников для выбора: Начало загрузки");
				return await _dbContextManager.ExecuteInContextAsync(async context =>
				{
					var employees = await context.Employees
						.Include(e => e.Positions)
						.Include(e => e.EmployeeContacts)
						.ToListAsync();

					Debug.WriteLine($"Получение всех сотрудников для выбора: Найдено {employees.Count} сотрудников");

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

					return result;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Получение всех сотрудников для выбора ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Получение всех сотрудников для выбора СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Получение всех сотрудников для выбора ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<bool> SetDepartmentHeadAsync(int departmentId, int employeeId)
		{
			try
			{
				Debug.WriteLine($"Назначение руководителя отдела: Отдел ID {departmentId}, Сотрудник ID {employeeId}");
				return await _dbContextManager.ExecuteInContextWithTransactionAsync(async context =>
				{
					// Проверяем существование отдела
					var department = await context.Departments
						.Include(d => d.Employees)
						.FirstOrDefaultAsync(d => d.DepartmentID == departmentId);

					if (department == null)
					{
						Debug.WriteLine($"Назначение руководителя отдела: Отдел с ID {departmentId} не найден");
						return false;
					}

					// Проверяем существование сотрудника
					var employee = await context.Employees
						.Include(e => e.Positions)
						.FirstOrDefaultAsync(e => e.EmployeeID == employeeId);

					if (employee == null)
					{
						Debug.WriteLine($"Назначение руководителя отдела: Сотрудник с ID {employeeId} не найден");
						return false;
					}

					// Устанавливаем сотрудника руководителем отдела
					department.HeadEmployeeID = employeeId;

					// Проверяем, что сотрудник в этом отделе
					if (employee.CurrentDepartmentID != departmentId)
					{
						// Переводим сотрудника в этот отдел, если он не там
						employee.CurrentDepartmentID = departmentId;

						// Создаем историю перевода
						var deptHistory = new EmployeeDepartments
						{
							EmployeeID = employeeId,
							DepartmentID = departmentId,
							StartDate = DateTime.Now,
							CreatedDate = DateTime.Now,
							ModifiedDate = DateTime.Now
						};
						context.EmployeeDepartments.Add(deptHistory);
					}

					// Проверяем должность - если не руководящая, меняем
					var positionTitle = employee.Positions.Title.ToLower();
					if (!positionTitle.Contains("руководител") &&
						!positionTitle.Contains("директор") &&
						!positionTitle.Contains("начальник"))
					{
						// Находим руководящую должность
						var managerPosition = await context.Positions
							.FirstOrDefaultAsync(p =>
								p.Title.Contains("руководител") ||
								p.Title.Contains("директор") ||
								p.Title.Contains("начальник"));

						if (managerPosition != null)
						{
							employee.PositionID = managerPosition.PositionID;
						}
					}

					employee.ModifiedDate = DateTime.Now;
					department.ModifiedDate = DateTime.Now;

					await context.SaveChangesAsync();
					Debug.WriteLine($"Назначение руководителя отдела: Успешно назначен руководитель для отдела с ID {departmentId}");
					return true;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Назначение руководителя отдела ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Назначение руководителя отдела СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Назначение руководителя отдела ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<bool> AddEmployeeToDepartmentAsync(int departmentId, int employeeId)
		{
			try
			{
				Debug.WriteLine($"Добавление сотрудника в отдел: Отдел ID {departmentId}, Сотрудник ID {employeeId}");
				return await _dbContextManager.ExecuteInContextWithTransactionAsync(async context =>
				{
					// Проверяем существование отдела
					var department = await context.Departments
						.FirstOrDefaultAsync(d => d.DepartmentID == departmentId);

					if (department == null)
					{
						Debug.WriteLine($"Добавление сотрудника в отдел: Отдел с ID {departmentId} не найден");
						return false;
					}

					// Проверяем существование сотрудника
					var employee = await context.Employees
						.FirstOrDefaultAsync(e => e.EmployeeID == employeeId);

					if (employee == null)
					{
						Debug.WriteLine($"Добавление сотрудника в отдел: Сотрудник с ID {employeeId} не найден");
						return false;
					}

					// Если сотрудник уже в этом отделе, ничего не делаем
					if (employee.CurrentDepartmentID == departmentId)
					{
						Debug.WriteLine($"Добавление сотрудника в отдел: Сотрудник уже в этом отделе");
						return true;
					}

					// Переводим сотрудника в этот отдел
					employee.CurrentDepartmentID = departmentId;

					// Создаем историю перевода
					var deptHistory = new EmployeeDepartments
					{
						EmployeeID = employeeId,
						DepartmentID = departmentId,
						StartDate = DateTime.Now,
						CreatedDate = DateTime.Now,
						ModifiedDate = DateTime.Now
					};
					context.EmployeeDepartments.Add(deptHistory);

					employee.ModifiedDate = DateTime.Now;
					department.ModifiedDate = DateTime.Now;

					await context.SaveChangesAsync();
					Debug.WriteLine($"Добавление сотрудника в отдел: Успешно добавлен сотрудник в отдел с ID {departmentId}");
					return true;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Добавление сотрудника в отдел ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Добавление сотрудника в отдел СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Добавление сотрудника в отдел ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<bool> RemoveEmployeeFromDepartmentAsync(int departmentId, int employeeId)
		{
			try
			{
				Debug.WriteLine($"Удаление сотрудника из отдела: Отдел ID {departmentId}, Сотрудник ID {employeeId}");
				return await _dbContextManager.ExecuteInContextWithTransactionAsync(async context =>
				{
					// Проверяем существование отдела
					var department = await context.Departments
						.FirstOrDefaultAsync(d => d.DepartmentID == departmentId);

					if (department == null)
					{
						Debug.WriteLine($"Удаление сотрудника из отдела: Отдел с ID {departmentId} не найден");
						return false;
					}

					// Проверяем существование сотрудника
					var employee = await context.Employees
						.FirstOrDefaultAsync(e => e.EmployeeID == employeeId && e.CurrentDepartmentID == departmentId);

					if (employee == null)
					{
						Debug.WriteLine($"Удаление сотрудника из отдела: Сотрудник с ID {employeeId} не найден в отделе {departmentId}");
						return false;
					}

					// Если сотрудник является руководителем отдела, удаляем эту связь
					if (department.HeadEmployeeID == employeeId)
					{
						department.HeadEmployeeID = null;
						Debug.WriteLine($"Удаление сотрудника из отдела: Сотрудник с ID {employeeId} был руководителем отдела - связь удалена");
					}

					// Находим запись в истории перевода и закрываем её
					var deptHistory = await context.EmployeeDepartments
						.Where(ed => ed.EmployeeID == employeeId && ed.DepartmentID == departmentId && ed.EndDate == null)
						.OrderByDescending(ed => ed.StartDate)
						.FirstOrDefaultAsync();

					if (deptHistory != null)
					{
						deptHistory.EndDate = DateTime.Now;
						deptHistory.ModifiedDate = DateTime.Now;
					}

					// Устанавливаем текущий отдел в null
					employee.CurrentDepartmentID = null;
					employee.ModifiedDate = DateTime.Now;
					department.ModifiedDate = DateTime.Now;

					await context.SaveChangesAsync();
					Debug.WriteLine($"Удаление сотрудника из отдела: Успешно удален сотрудник из отдела с ID {departmentId}");
					return true;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Удаление сотрудника из отдела ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Удаление сотрудника из отдела СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Удаление сотрудника из отдела ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<bool> AddProjectToDepartmentAsync(int departmentId, ProjectViewModel project)
		{
			try
			{
				Debug.WriteLine($"Добавление проекта в отдел: Отдел ID {departmentId}");
				return await _dbContextManager.ExecuteInContextWithTransactionAsync(async context =>
				{
					// Проверяем существование отдела
					var department = await context.Departments
						.FirstOrDefaultAsync(d => d.DepartmentID == departmentId);

					if (department == null)
					{
						Debug.WriteLine($"Добавление проекта в отдел: Отдел с ID {departmentId} не найден");
						return false;
					}

					// Создаем новый проект
					var newProject = new Projects
					{
						Name = project.Name,
						Description = project.Description,
						DepartmentID = departmentId,
						CreatedDate = DateTime.Now,
						ModifiedDate = DateTime.Now
					};

					context.Projects.Add(newProject);
					await context.SaveChangesAsync();

					// Если указан бюджет проекта
					if (project.Budget.HasValue)
					{
						var projectBudget = new ProjectBudgets
						{
							ProjectID = newProject.ProjectID,
							Budget = project.Budget,
							CreatedDate = DateTime.Now,
							ModifiedDate = DateTime.Now
						};

						context.ProjectBudgets.Add(projectBudget);
					}

					// Если указаны даты проекта
					if (project.StartDate.HasValue || project.EndDate.HasValue)
					{
						var projectTimeline = new ProjectTimelines
						{
							ProjectID = newProject.ProjectID,
							StartDate = project.StartDate ?? DateTime.Now,
							EndDate = project.EndDate,
							CreatedDate = DateTime.Now,
							ModifiedDate = DateTime.Now
						};

						context.ProjectTimelines.Add(projectTimeline);
					}

					await context.SaveChangesAsync();
					Debug.WriteLine($"Добавление проекта в отдел: Успешно добавлен проект в отдел с ID {departmentId}");
					return true;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Добавление проекта в отдел ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Добавление проекта в отдел СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Добавление проекта в отдел ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<bool> RemoveProjectFromDepartmentAsync(int departmentId, int projectId)
		{
			try
			{
				Debug.WriteLine($"Удаление проекта из отдела: Отдел ID {departmentId}, Проект ID {projectId}");
				return await _dbContextManager.ExecuteInContextWithTransactionAsync(async context =>
				{
					// Проверяем существование проекта
					var project = await context.Projects
						.Include(p => p.ProjectBudgets)
						.Include(p => p.ProjectTimelines)
						.Include(p => p.EmployeeProjects)
						.FirstOrDefaultAsync(p => p.ProjectID == projectId && p.DepartmentID == departmentId);

					if (project == null)
					{
						Debug.WriteLine($"Удаление проекта из отдела: Проект с ID {projectId} не найден в отделе {departmentId}");
						return false;
					}

					// Удаляем связанные записи
					foreach (var budget in project.ProjectBudgets.ToList())
					{
						context.ProjectBudgets.Remove(budget);
					}

					foreach (var timeline in project.ProjectTimelines.ToList())
					{
						context.ProjectTimelines.Remove(timeline);
					}

					foreach (var empProject in project.EmployeeProjects.ToList())
					{
						context.EmployeeProjects.Remove(empProject);
					}

					// Удаляем сам проект
					context.Projects.Remove(project);

					await context.SaveChangesAsync();
					Debug.WriteLine($"Удаление проекта из отдела: Успешно удален проект из отдела с ID {departmentId}");
					return true;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Удаление проекта из отдела ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Удаление проекта из отдела СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Удаление проекта из отдела ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<List<ProjectViewModel>> GetAllProjectsAsync()
		{
			try
			{
				Debug.WriteLine("Получение всех проектов: Начало загрузки");
				return await _dbContextManager.ExecuteInContextAsync(async context =>
				{
					var projects = await context.Projects
						.Include(p => p.ProjectBudgets)
						.Include(p => p.ProjectTimelines)
						.Include(p => p.EmployeeProjects)
						.Include(p => p.Departments)
						.ToListAsync();

					Debug.WriteLine($"Получение всех проектов: Найдено {projects.Count} проектов");

					var result = projects.Select(p =>
					{
						var vm = MapProjectToViewModel(p);
						vm.DepartmentName = p.Departments?.Name; // Добавляем название отдела
						return vm;
					}).ToList();

					return result;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Получение всех проектов ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Получение всех проектов СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Получение всех проектов ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}

		public async Task<bool> DeleteProjectAsync(int projectId)
		{
			try
			{
				Debug.WriteLine($"Удаление проекта: Проект ID {projectId}");
				return await _dbContextManager.ExecuteInContextWithTransactionAsync(async context =>
				{
					// Проверяем существование проекта
					var project = await context.Projects
						.Include(p => p.ProjectBudgets)
						.Include(p => p.ProjectTimelines)
						.Include(p => p.EmployeeProjects)
						.FirstOrDefaultAsync(p => p.ProjectID == projectId);

					if (project == null)
					{
						Debug.WriteLine($"Удаление проекта: Проект с ID {projectId} не найден");
						return false;
					}

					// Удаляем связанные записи
					foreach (var budget in project.ProjectBudgets.ToList())
					{
						context.ProjectBudgets.Remove(budget);
					}

					foreach (var timeline in project.ProjectTimelines.ToList())
					{
						context.ProjectTimelines.Remove(timeline);
					}

					foreach (var empProject in project.EmployeeProjects.ToList())
					{
						context.EmployeeProjects.Remove(empProject);
					}

					// Удаляем сам проект
					context.Projects.Remove(project);

					await context.SaveChangesAsync();
					Debug.WriteLine($"Удаление проекта: Успешно удален проект с ID {projectId}");
					return true;
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Удаление проекта ОШИБКА: {ex.Message}");
				Debug.WriteLine($"Удаление проекта СТЕК ВЫЗОВОВ: {ex.StackTrace}");
				if (ex.InnerException != null)
				{
					Debug.WriteLine($"Удаление проекта ВНУТРЕННЯЯ ОШИБКА: {ex.InnerException.Message}");
				}
				throw;
			}
		}
	}
}
