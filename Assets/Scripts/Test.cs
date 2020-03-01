﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class Test : MonoBehaviour
{
	private const int TRIAL_COUNT = 100;

	[SerializeField] private Texture2D defaultTexture;
	[SerializeField] private RawImage rawImage;
	[SerializeField] private UIController uIController;


	public void OnLoadImage()
	{
		this.StartCoroutine(this.LoadImage());
	}

	public void OnAsyncLoadImage_NonCompression()
	{
		this.StartCoroutine(this.AsyncLoadImage(Utility.testImagePath_RGBA32, TextureFormat.RGBA32));
	}

	public void OnAsyncLoadImage()
	{
		#if UNITY_ANDROID
		this.StartCoroutine(this.AsyncLoadImage(Utility.testImagePath_Android, Utility.UseTextureFormat));
		#endif
		#if UNITY_IOS
		this.StartCoroutine(this.AsyncLoadImage(Utility.testImagePath_iOS, Utility.UseTextureFormat));
		#endif
	}


	private IEnumerator LoadImage()
	{
		this.rawImage.texture = this.defaultTexture;
		this.uIController.SetButtonInteractable(false);

		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
		sw.Start();

		for (int i = 0; i < TRIAL_COUNT; i++) {
			this.rawImage.texture = Utility.LoadTexture(Utility.testImagePath);
			this.rawImage.SetNativeSize();
			this.uIController.SetProgressText(TRIAL_COUNT, i + 1);
			yield return null;
		}

		sw.Stop();
		this.uIController.SetProcessedTime(sw.Elapsed.ToString());

		this.uIController.SetButtonInteractable(true);
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	private IEnumerator AsyncLoadImage(string filePath, TextureFormat textureFormat)
	{
		this.rawImage.texture = this.defaultTexture;
		this.uIController.SetButtonInteractable(false);

		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
		sw.Start();

		for (int i = 0; i < TRIAL_COUNT; i++) {
			AsyncTextureReader.Read(
				new AsyncTextureReader.TextureData {
					filePath = filePath,
					format = textureFormat,
					width = 1000,
					height = 1000
				},
				texture => {
					this.rawImage.texture = texture;
					this.rawImage.SetNativeSize();
					this.uIController.SetProgressText(
						AsyncTextureReader.Progress.totalCount,
						AsyncTextureReader.Progress.processedCount
					);
				}
			);
		}

		WaitUntil waitUntil = new WaitUntil(() => !AsyncTextureReader.Progress.IsRuntime);
		yield return waitUntil;

		sw.Stop();
		this.uIController.SetProcessedTime(sw.Elapsed.ToString());

		this.uIController.SetButtonInteractable(true);
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}
}