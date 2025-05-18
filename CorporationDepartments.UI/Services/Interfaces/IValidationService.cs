using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporationDepartments.UI.Services.Interfaces
{
	public interface IValidationService
	{
		(bool isValid, string error) ValidateEmail(string email);
		(bool isValid, string error) ValidatePhoneNumber(string phoneNumber);
		(bool isValid, string error) ValidatePassword(string password);
		(bool isValid, string error) ValidateRequiredField(string value, string fieldName);
		(bool isValid, string error) ValidatePasswordMatch(string password, string confirmPassword);
	}
}
