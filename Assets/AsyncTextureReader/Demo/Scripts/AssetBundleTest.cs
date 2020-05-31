using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Collections;

public class AssetBundleTest : MonoBehaviour
{
	private const int TRIAL_COUNT = 100;

	#if UNITY_ANDROID
	private const string testAssetBundlePath = "assetbundle/android/test.unity3d";
	#endif
	#if UNITY_IOS
	private const string testAssetBundlePath = "assetbundle/ios/test.unity3d";
	#else
	private const string testAssetBundlePath = "";
	#endif

	[SerializeField] private RawImage rawImage;
	[SerializeField] private UIController uiController;

	private Texture2D defaultTexture;
	private bool isInitialized;

	private void Start()
	{
		this.defaultTexture = Resources.Load<Texture2D>(Utility.defaultTexturePath);
		if (!string.IsNullOrEmpty(testAssetBundlePath)) {
			this.StartCoroutine(this.Initialize());
		}
	}

	private IEnumerator Initialize()
	{
		this.isInitialized = false;

		string filePath = Path.Combine(Application.streamingAssetsPath, testAssetBundlePath);
		byte[] byteData;

		#if UNITY_EDITOR || UNITY_IOS
		filePath = "file://" + filePath;
		#endif

		using (UnityWebRequest request = UnityWebRequest.Get(filePath)) {
			yield return request.SendWebRequest();
			byteData = request.downloadHandler.data;
		}

		Utility.SaveByteData(Utility.persistentDataPath + "/" + testAssetBundlePath, byteData);

		this.isInitialized = true;
		yield break;
	}

	public void OnLoadAssetBundle()
	{
		if (!this.isInitialized) {
			return;
		}
		this.StartCoroutine(this.LoadAssetBundle());
	}

	private IEnumerator LoadAssetBundle()
	{
		this.rawImage.texture = this.defaultTexture;
		this.uiController.SetButtonInteractable(false);

		string path = Utility.persistentDataPath + "/" + testAssetBundlePath;

		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
		sw.Start();

		for (int i = 0; i < TRIAL_COUNT; i++) {
			byte[] byteData = Utility.LoadByteData(path);
			// decryption
			AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(byteData);
			yield return createRequest;

			AssetBundle assetBundle = createRequest.assetBundle;

			AssetBundleRequest request = assetBundle.LoadAssetAsync<Texture2D>("test");
			yield return request;

			Texture2D texture = request.asset as Texture2D;

			assetBundle.Unload(false);

			this.rawImage.texture = texture;
			this.rawImage.SetNativeSize();

			this.uiController.SetProgressText(TRIAL_COUNT, i + 1);
		}

		sw.Stop();
		this.uiController.SetProcessedTime(sw.Elapsed.ToString());

		this.uiController.SetButtonInteractable(true);
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}
}