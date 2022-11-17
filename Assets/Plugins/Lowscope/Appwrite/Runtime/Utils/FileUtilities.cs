using System;
using System.IO;
using Lowscope.AppwritePlugin.Accounts.Encryption;
using Newtonsoft.Json;
using UnityEngine;

namespace Lowscope.AppwritePlugin.Utils
{
	public class FileUtilities
	{
		public static void Write<T>(T data, string path, AppwriteConfig config) where T : class
		{
			if (data == null)
				return;

			string json = JsonConvert.SerializeObject(data);

			if (!config.UseEncryption)
			{
				File.WriteAllText(path, json);
				return;
			}

			try
			{
				byte[] encrypt = AesEncryption.Encrypt(json, config.EncryptionKey, config.EncryptionIV);
				File.WriteAllBytes(path, encrypt);
			}
			catch (Exception e)
			{
				Debug.Log(e);
			}
		}

		public static T Read<T>(string path, AppwriteConfig config) where T : class
		{
			if (!File.Exists(path))
				return null;

			try
			{
				if (!config.UseEncryption)
					return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));

				byte[] bytes = File.ReadAllBytes(path);
				string data = AesEncryption.Decrypt(bytes, config.EncryptionKey, config.EncryptionIV);
				return JsonConvert.DeserializeObject<T>(data);
			}
			catch (Exception e)
			{
				Debug.Log(e);
				if (config.RemoveFileIfDecryptionFails)
					File.Delete(path);
				return null;
			}
		}
	}
}