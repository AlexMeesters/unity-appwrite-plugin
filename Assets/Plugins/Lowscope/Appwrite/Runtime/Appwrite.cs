using System;
using Lowscope.AppwritePlugin.Accounts;
using UnityEngine;

namespace Lowscope.AppwritePlugin
{
	public class Appwrite : MonoBehaviour
	{
		private static Appwrite instance;

		private AppwriteConfig config;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			string objectName = "Appwrite";
			var obj = new GameObject(objectName);
			obj.gameObject.SetActive(false);
			instance = obj.AddComponent<Appwrite>();
			DontDestroyOnLoad(instance.gameObject);

			instance.config = Resources.Load<AppwriteConfig>("Appwrite Config");

			// Generate configuration asset if not present.
#if UNITY_EDITOR
			if (instance.config == null)
			{
				instance.config = ScriptableObject.CreateInstance<AppwriteConfig>();

				if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
					UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");

				UnityEditor.AssetDatabase.CreateAsset(instance.config, 
					"Assets/Resources/Appwrite Config.asset");
				UnityEditor.AssetDatabase.SaveAssets();
			}
#endif
			// Avoids calling Awake() prematurely.
			obj.gameObject.SetActive(true);
		}

		private Account account;

		private void Awake()
		{
			account = new Account(config);
		}

		public static Account Account => instance.account;
	}
}