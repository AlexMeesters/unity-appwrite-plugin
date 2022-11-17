using System.Text.RegularExpressions;

namespace Lowscope.AppwritePlugin.Utils
{
	public static class WebUtilities
	{
		public static bool IsEmailValid(string email)
		{
			Regex regex =
				new Regex(
					@"^[-!#$%&'*+\/0-9=?A-Z^_a-z`{|}~](\.?[-!#$%&'*+\/0-9=?A-Z^_a-z`{|}~])*@[a-zA-Z0-9](-*\.?[a-zA-Z0-9])*\.[a-zA-Z](-?[a-zA-Z0-9])+$");
			return regex.Match(email).Success;
		}
	}
}