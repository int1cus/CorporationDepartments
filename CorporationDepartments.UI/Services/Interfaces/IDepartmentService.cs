using CorporationDepartments.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.Services.Interfaces
{
	public interface IDepartmentService
	{
		/// <summary>
		/// Получает список всех отделов
		/// </summary>
		Task<List<DepartmentViewModel>> GetAllDepartmentsAsync();

		/// <summary>
		/// Получает информацию об отделе по ID
		/// </summary>
		Task<DepartmentViewModel> GetDepartmentByIdAsync(int id);

		/// <summary>
		/// Создает новый отдел
		/// </summary>
		Task<int> CreateDepartmentAsync(DepartmentViewModel department);

		/// <summary>
		/// Обновляет существующий отдел
		/// </summary>
		Task<bool> UpdateDepartmentAsync(DepartmentViewModel department);

		/// <summary>
		/// Удаляет отдел по ID
		/// </summary>
		Task<bool> DeleteDepartmentAsync(int id);

		/// <summary>
		/// Получает список сотрудников в отделе
		/// </summary>
		Task<List<EmployeeViewModel>> GetEmployeesInDepartmentAsync(int departmentId);

		/// <summary>
		/// Получает список проектов отдела
		/// </summary>
		Task<List<ProjectViewModel>> GetProjectsInDepartmentAsync(int departmentId);

		/// <summary>
		/// Получает список всех проектов
		/// </summary>
		Task<List<ProjectViewModel>> GetAllProjectsAsync();

		/// <summary>
		/// Получает список всех сотрудников для выбора руководителя отдела
		/// </summary>
		Task<List<EmployeeViewModel>> GetAllEmployeesForSelectionAsync();

		/// <summary>
		/// Назначает руководителя отдела
		/// </summary>
		Task<bool> SetDepartmentHeadAsync(int departmentId, int employeeId);

		/// <summary>
		/// Добавляет сотрудника в отдел
		/// </summary>
		Task<bool> AddEmployeeToDepartmentAsync(int departmentId, int employeeId);

		/// <summary>
		/// Удаляет сотрудника из отдела
		/// </summary>
		Task<bool> RemoveEmployeeFromDepartmentAsync(int departmentId, int employeeId);

		/// <summary>
		/// Добавляет проект в отдел
		/// </summary>
		Task<bool> AddProjectToDepartmentAsync(int departmentId, ProjectViewModel project);

		/// <summary>
		/// Удаляет проект из отдела
		/// </summary>
		Task<bool> RemoveProjectFromDepartmentAsync(int departmentId, int projectId);

		/// <summary>
		/// Удаляет проект по ID
		/// </summary>
		Task<bool> DeleteProjectAsync(int projectId);
	}
}
