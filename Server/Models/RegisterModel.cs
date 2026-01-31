using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
	public class RegisterModel
	{
		[Required(ErrorMessage = "User Name is reuired")]
		public string? Username { get; set; }

		[EmailAddress]
		[Required(ErrorMessage = "Email is required")]
		public string? Email { get; set; }

		[Required(ErrorMessage = "Password is required")]
		public string? Password { get; set; }
	}
}