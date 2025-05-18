using CorporationDepartments.UI.Services;
using CorporationDepartments.UI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CorporationDepartments.UI.Converters
{
	public class SelfActionToolTipConverter : IValueConverter
	{
		private IAuthenticationService AuthenticationService =>
			ServiceProviderHelper.GetService<IAuthenticationService>();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string email)
			{
				if (AuthenticationService?.CurrentUserEmail == email)
				{
					return "Нельзя редактировать или удалять свой профиль";
				}
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
