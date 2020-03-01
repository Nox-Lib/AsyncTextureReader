using UnityEngine;
using Unity.IO.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System;
using System.Collections.Generic;
using System.IO;

public sealed unsafe class AsyncTextureReader : MonoBehaviour
{
	private static AsyncTextureReader instance = null;
	private static AsyncTextureReader Instance {
		get {
			if (instance == null) {
				instance = new GameObject("AsyncTextureReader").AddComponent<AsyncTextureReader>();
				DontDestroyOnLoad(instance.gameObject);
			}
			return instance;
		}
	}

	public struct TextureData
	{
		public string filePath;
		public TextureFormat format;
		public int width;
		public int height;
	}

	[Serializable]
	public struct ProgressData
	{
		public bool IsRuntime => Instance.enabled;
		public int totalCount;
		public int processedCount;
	}

	private class Task
	{
		public ReadHandle readHandle;
		public NativeArray<ReadCommand> readCommands;
		public TextureData textureData;
		public Action<Texture2D> onComplete;
		public long fileSize;
	}

	private Queue<Task> tasks = new Queue<Task>();
	private Task currentTask;

	[SerializeField, ReadOnly] private ProgressData progressData;
	public static ProgressData Progress => Instance.progressData;


	public static void Read(TextureData textureData, Action<Texture2D> onComplete)
	{
		Instance.tasks.Enqueue(new Task { textureData = textureData, onComplete = onComplete });
		Instance.progressData.totalCount++;
		Instance.enabled = true;
	}


	public void Update()
	{
		if (this.tasks.Count <= 0 && this.currentTask == null) {
			Instance.progressData.totalCount = 0;
			Instance.progressData.processedCount = 0;
			this.enabled = false;
			return;
		}

		if (this.currentTask == null) {
			this.Next();
		}

		if (!this.currentTask.readHandle.IsValid()) {
			this.Result(null);
			return;
		}

		if (this.currentTask.readHandle.Status == ReadStatus.InProgress) {
			return;
		}
		if (this.currentTask.readHandle.Status != ReadStatus.Complete) {
			this.Result(null);
			return;
		}

		TextureData textureData = this.currentTask.textureData;
		Texture2D texture = new Texture2D(textureData.width, textureData.height, textureData.format, false);
		texture.LoadRawTextureData((IntPtr)this.currentTask.readCommands[0].Buffer, (int)this.currentTask.fileSize);
		texture.Apply();
		texture.name = Path.GetFileName(textureData.filePath);

		this.Result(texture);
	}


	private void Next()
	{
		Task task = this.tasks.Dequeue();

		FileInfo fileInfo = new FileInfo(task.textureData.filePath);
		task.fileSize = fileInfo.Length;

		task.readCommands = new NativeArray<ReadCommand>(1, Allocator.Persistent);
		task.readCommands[0] = new ReadCommand {
			Offset = 0,
			Size = task.fileSize,
			Buffer = UnsafeUtility.Malloc(task.fileSize, UnsafeUtility.AlignOf<byte>(), Allocator.Persistent)
		};
		task.readHandle = AsyncReadManager.Read(task.textureData.filePath, (ReadCommand*)task.readCommands.GetUnsafePtr(), 1);

		this.currentTask = task;
	}

	private void Result(Texture2D texture)
	{
		Instance.progressData.processedCount++;
		this.currentTask.onComplete?.Invoke(texture);

		this.currentTask.readHandle.Dispose();
		UnsafeUtility.Free(this.currentTask.readCommands[0].Buffer, Allocator.Persistent);
		this.currentTask.readCommands.Dispose();

		this.currentTask = null;
	}
}