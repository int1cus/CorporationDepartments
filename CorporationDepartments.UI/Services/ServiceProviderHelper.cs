using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.Services
{
	public static class ServiceProviderHelper
	{
		private static IServiceProvider _serviceProvider;

		public static void Initialize(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public static T GetService<T>() where T : class
		{
			if (_serviceProvider == null)
			{
				throw new InvalidOperationException("ServiceProvider не был инициализирован.");
			}

			return _serviceProvider.GetService<T>();
		}
	}
}
