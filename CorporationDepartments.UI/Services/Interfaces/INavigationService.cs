using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CorporationDepartments.UI.Services.Interfaces
{
	public interface INavigationService
	{
		event EventHandler<Page> OnPageChanged;
		void Initialize(Frame mainFrame);
		void NavigateToPage<T>() where T : Page;
		void NavigateToPage(Page page);
		void NavigateTo(string pageName, object parameter = null);
		bool CanGoBack { get; }
		void GoBack();
	}
}
