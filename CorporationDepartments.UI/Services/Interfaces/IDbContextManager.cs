using CorporationDepartments.UI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.Services.Interfaces
{
	public interface IDbContextManager
	{
		CorporationDepartmentsEntities CreateContext();
		T ExecuteInContext<T>(Func<CorporationDepartmentsEntities, T> action);
		Task<T> ExecuteInContextAsync<T>(Func<CorporationDepartmentsEntities, Task<T>> action);
		T ExecuteInContextWithTransaction<T>(Func<CorporationDepartmentsEntities, T> action);
		Task<T> ExecuteInContextWithTransactionAsync<T>(Func<CorporationDepartmentsEntities, Task<T>> action);
	}
}
