namespace Lowscope.AppwritePlugin.Accounts.Enums
{
	public enum EEmailVerifyResponse
	{
		Sent,
		Failed,
		NotLoggedIn,
		AlreadyVerified,
		NoURLSpecified,
		ServerBusy,
		NoConnection,
		Timeout
	}
}