using UnityEngine;
using System.IO;

public class Initializer : MonoBehaviour
{
	[SerializeField] private Texture2D testImage;
	[SerializeField] private Texture2D testImageAndroid;
	[SerializeField] private Texture2D testImageIOS;

	private void Awake()
	{
		Application.targetFrameRate = 60;

		if (!File.Exists(Utility.testImagePath)) {
			Utility.SaveByteData(Utility.testImagePath, this.testImage.EncodeToPNG());
		}
		if (!File.Exists(Utility.testImagePath_RGBA32)) {
			Utility.SaveByteData(Utility.testImagePath_RGBA32, this.testImage.GetRawTextureData());
		}
		#if UNITY_ANDROID
		if (!File.Exists(Utility.testImagePath_Android)) {
			Utility.SaveByteData(Utility.testImagePath_Android, this.testImageAndroid.GetRawTextureData());
		}
		#endif
		#if UNITY_IOS
		if (!File.Exists(Utility.testImagePath_iOS)) {
			Utility.SaveByteData(Utility.testImagePath_iOS, this.testImageIOS.GetRawTextureData());
		}
		#endif
	}
}