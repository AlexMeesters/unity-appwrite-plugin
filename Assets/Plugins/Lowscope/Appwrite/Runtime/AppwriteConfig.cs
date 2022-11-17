using System;
using UnityEngine;

namespace Lowscope.Appwrite
{
	public class AppwriteConfig : ScriptableObject
	{
		[Header("API")]
		public string AppwriteURL = "https://myappwriteserver.com/v1";

		public string AppwriteProjectID = "MyAppwriteProjectID";

		[Header("Account")]
		public string VerifyEmailURL = "MyVerifyMailEndpoint";

		public float JwtExpireMinutes = 15;
		public float VerifyEmailTimeoutMinutes = 5;
		public float RegisterTimeoutMinutes = 1;

		[Header("Encryption (Basic)")]
		public bool UseEncryption = true;
		public bool RemoveFileIfDecryptionFails = false;
		public bool GenerateKeyAndIV = true;

		public string EncryptionKey;
		public string EncryptionIV;

#if UNITY_EDITOR
		// Not the best protection, but anything is better then plaintext.
		public void OnValidate()
		{
			if (EncryptionKey.Length != 44 || EncryptionIV.Length != 24)
				GenerateKeyAndIV = true;

			if (!GenerateKeyAndIV)
				return;

			using (System.Security.Cryptography.Aes myAes = System.Security.Cryptography.Aes.Create())
			{
				EncryptionKey = Convert.ToBase64String(myAes.Key);
				EncryptionIV = Convert.ToBase64String(myAes.IV);
			}
			
			GenerateKeyAndIV = false;
			
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}
#endif
	}
}