using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;

public class AssetBundleTest : MonoBehaviour
{
	private const int TRIAL_COUNT = 100;

	#if UNITY_ANDROID
	private const string testAssetBundlePath = "AssetBundle/android/test.unity3d";
	#endif
	#if UNITY_IOS
	private const string testAssetBundlePath = "AssetBundle/ios/test.unity3d";
	#endif

	[SerializeField] private RawImage rawImage;
	[SerializeField] private UIController uiController;

	private Texture2D defaultTexture;

	private void Start()
	{
		this.defaultTexture = Resources.Load<Texture2D>(Utility.defaultTexturePath);

		string filePath = Application.dataPath + "/" + testAssetBundlePath;
		byte[] byteData = File.ReadAllBytes(filePath);
		Utility.SaveByteData(Utility.persistentDataPath + "/" + testAssetBundlePath, byteData);
	}

	public void OnLoadAssetBundle()
	{
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
			AssetBundleCreateRequest createRequest = AssetBundle.LoadFromFileAsync(path);
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