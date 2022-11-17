using System.Text;
using Cysharp.Threading.Tasks;
using Lowscope.AppwritePlugin;
using Lowscope.AppwritePlugin.Accounts.Enums;
using Lowscope.AppwritePlugin.Accounts.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppwriteExampleUI : MonoBehaviour
{
	[SerializeField] private TMP_InputField inputFieldAccount;
	[SerializeField] private TMP_InputField inputFieldEmail;
	[SerializeField] private TMP_InputField inputFieldPassword;

	[SerializeField] private CanvasGroup canvasGroupInputFields;

	[SerializeField] private Button buttonLogin;
	[SerializeField] private Button buttonLogout;
	[SerializeField] private Button buttonGetJWT;
	[SerializeField] private Button buttonRefreshInfo;
	[SerializeField] private Button buttonRegister;
	[SerializeField] private Button buttonVerifyMail;

	[SerializeField] private TextMeshProUGUI infoText;

	private User user;

	private async void Start()
	{
		buttonLogin.onClick.AddListener(OnButtonLogin);
		buttonLogout.onClick.AddListener(OnButtonLogout);
		buttonGetJWT.onClick.AddListener(OnButtonGetJwt);
		buttonRefreshInfo.onClick.AddListener(OnButtonRefreshInfo);
		buttonVerifyMail.onClick.AddListener(OnButtonVerifyMail);
		buttonRegister.onClick.AddListener(OnButtonRegister);

		// Attempts to get/validate (saved) information from server
		await UpdateInfo(true);
	}

	private async void OnButtonLogin()
	{
		string email = inputFieldEmail.text;
		string password = inputFieldPassword.text;

		// Not using user directly here, because the GetUserInfo call provides more.
		var (result, response) = await Appwrite.Account.Login(email, password);

		if (response == ELoginResponse.Success)
			user = result;

		Debug.Log($"User login. Response: {response}");

		await UpdateInfo(false);
	}

	private async void OnButtonLogout()
	{
		bool hasLoggedOut = await Appwrite.Account.Logout();
		Debug.Log(!hasLoggedOut ? "Unable to logout. No session active." : "User logged out. Cookies cleared.");
		user = null;
		
		await UpdateInfo(false);
	}

	private async void OnButtonRegister()
	{
		string account = inputFieldAccount.text;
		string email = inputFieldEmail.text;
		string password = inputFieldPassword.text;

		// In this case both the userid and name are the same for simplicity sake.
		var (result, response) = await Appwrite.Account.Register(account, account, email, password);

		if (response == ERegisterResponse.Success)
			user = result;

		Debug.Log($"User registration. Response: {response}");

		await UpdateInfo(false);
	}

	private async void OnButtonGetJwt()
	{
		string jwt = await Appwrite.Account.ObtainJwt();
		Debug.Log(!string.IsNullOrEmpty(jwt) ? $"JWT: {jwt}" : "Unable to retrieve JWT");
	}

	private async void OnButtonRefreshInfo()
	{
		await UpdateInfo(true);
	}

	private async void OnButtonVerifyMail()
	{
		EEmailVerifyResponse response = await Appwrite.Account.RequestVerificationMail();
		Debug.Log($"Email response: {response}");
	}

	private async UniTask UpdateInfo(bool refreshFromServer)
	{
		if (refreshFromServer)
			user = await Appwrite.Account.GetUser();

		bool hasUser = user != null;
		
		if (hasUser)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"User: {user.Id}");
			sb.AppendLine($"Name: {user.Name}");
			sb.AppendLine($"Email: {user.Email}");
			sb.AppendLine($"Verified: {user.EmailVerified}");
			infoText.SetText(sb.ToString());
		}
		else
		{
			string alignment = @"""center""";
			infoText.SetText($"<align={alignment}>No active session");
		}

		buttonLogout.interactable = hasUser;
		buttonGetJWT.interactable = hasUser;
		buttonRefreshInfo.interactable = hasUser;
		buttonVerifyMail.interactable = hasUser && !user.EmailVerified;
		buttonRegister.interactable = !hasUser;

		canvasGroupInputFields.alpha = hasUser ? 0.75f : 1;
		canvasGroupInputFields.interactable = !hasUser;
	}
}