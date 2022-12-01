using System;
using System.Net.Mail;

namespace Lowscope.AppwritePlugin.Utils
{
	public static class WebUtilities
	{
		// Very basic validation check
		public static bool IsEmailValid(string email)
		{
			try
			{
				MailAddress m = new MailAddress(email);

				return true;
			}
			catch (FormatException)
			{
				return false;
			}
		}
	}
}