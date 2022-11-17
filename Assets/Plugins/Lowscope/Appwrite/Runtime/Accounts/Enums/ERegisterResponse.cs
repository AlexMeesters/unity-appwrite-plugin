namespace Lowscope.AppwritePlugin.Accounts.Enums
{
	public enum ERegisterResponse
	{
		Success,
		MissingCredentials,
		Failed,
		AlreadyLoggedIn,
		ServerBusy,
		NoConnection,
		Timeout,
		InvalidEmail
	}
}