using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.Services.Interfaces
{
	public interface IAuthenticationService
	{
		Task<bool> LoginAsync(string username, string password);
		Task<(bool success, string username)> RegisterAsync(
			string firstName,
			string lastName,
			string patronymic,
			string email,
			string phoneNumber,
			string password);
		void Logout();
		bool IsAuthenticated { get; }
		int? CurrentUserId { get; }
		string CurrentUserEmail { get; }
	}
}
