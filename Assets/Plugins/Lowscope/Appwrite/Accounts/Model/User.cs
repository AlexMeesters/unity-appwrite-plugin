using System;
using Newtonsoft.Json;

namespace Lowscope.Appwrite.Accounts.Model
{
	public class User
	{
		public string Cookie { get; set; }
		public string Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public bool EmailVerified { get; set; }
		public DateTime LastEmailRequestDate { get; set; }

		[JsonIgnore] public string Jwt { get; set; }
		[JsonIgnore] public DateTime JwtProvideDate { get; set; }
	}
}