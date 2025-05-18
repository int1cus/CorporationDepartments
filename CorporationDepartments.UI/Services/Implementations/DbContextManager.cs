using CorporationDepartments.UI.Data;
using CorporationDepartments.UI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.Services.Implementations
{
	public class DbContextManager : IDbContextManager
	{
		public CorporationDepartmentsEntities CreateContext()
		{
			return new CorporationDepartmentsEntities();
		}

		public T ExecuteInContext<T>(Func<CorporationDepartmentsEntities, T> action)
		{
			using (var context = CreateContext())
			{
				return action(context);
			}
		}

		public async Task<T> ExecuteInContextAsync<T>(Func<CorporationDepartmentsEntities, Task<T>> action)
		{
			using (var context = CreateContext())
			{
				return await action(context);
			}
		}

		public T ExecuteInContextWithTransaction<T>(Func<CorporationDepartmentsEntities, T> action)
		{
			using (var context = CreateContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					try
					{
						var result = action(context);
						transaction.Commit();
						return result;
					}
					catch
					{
						transaction.Rollback();
						throw;
					}
				}
			}
		}

		public async Task<T> ExecuteInContextWithTransactionAsync<T>(
			Func<CorporationDepartmentsEntities, Task<T>> action)
		{
			using (var context = CreateContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					try
					{
						var result = await action(context);
						transaction.Commit();
						return result;
					}
					catch
					{
						transaction.Rollback();
						throw;
					}
				}
			}
		}
	}
}
