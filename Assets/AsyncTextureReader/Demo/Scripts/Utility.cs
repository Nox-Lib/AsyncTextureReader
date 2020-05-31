using UnityEngine;
using System.IO;

public static class Utility
{
	#if UNITY_EDITOR
	public static readonly string developDataPath = Application.dataPath + "/../develop";
	#endif

	#if UNITY_EDITOR
	public static readonly string persistentDataPath = developDataPath + "/persistent";
	#else
	public static readonly string persistentDataPath = Application.persistentDataPath;
	#endif

	public static readonly string testImagePath = persistentDataPath + "/Image/test.png";
	public static readonly string testImagePath_RGBA32 = persistentDataPath + "/Image/test_rgba32.png";
	public static readonly string testImagePath_Android = persistentDataPath + "/Image/test_android_etc2.png";
	public static readonly string testImagePath_iOS = persistentDataPath + "/Image/test_ios_astc4x4.png";

	public static readonly string defaultTexturePath = "Image/transparent";

	public static TextureFormat UseTextureFormat {
		get {
			#if UNITY_ANDROID
			return TextureFormat.ETC2_RGBA8;
			#elif UNITY_IOS
			#if UNITY_2019_ON_NEWER
			return TextureFormat.ASTC;
			#else
			return TextureFormat.ASTC_RGBA_4x4;
			#endif
			#else
			return TextureFormat.RGBA32;
			#endif
		}
	}

	public static void CreateDirectory(string path)
	{
		if (Directory.Exists(path)) {
			return;
		}
		Directory.CreateDirectory(path);
	}

	public static void SaveByteData(string path, byte[] byteData)
	{
		CreateDirectory(Path.GetDirectoryName(path));
		File.WriteAllBytes(path, byteData);
	}

	public static byte[] LoadByteData(string path)
	{
		return File.Exists(path) ? File.ReadAllBytes(path) : null;
	}

	public static Texture2D LoadTexture(string path)
	{
		return CreateTexture(LoadByteData(path), Path.GetFileName(path));
	}

	public static Texture2D CreateTexture(byte[] byteData, string textureName = null)
	{
		Texture2D texture = new Texture2D(0, 0, TextureFormat.RGBA32, false);
		texture.LoadImage(byteData);
		texture.name = textureName;
		return texture;
	}
}