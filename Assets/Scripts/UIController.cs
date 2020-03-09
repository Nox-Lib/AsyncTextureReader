using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
	[SerializeField] private Transform loadingObject;
	[SerializeField] private Text testButtonText;
	[SerializeField] private Text fpsText;
	[SerializeField] private Text progressText;

	[Header("### Buttons")]
	[SerializeField] private Button button1;
	[SerializeField] private Button button2;
	[SerializeField] private Button button3;
	[SerializeField] private Button button4;

	private int frameCount = 0;
	private float prevTime = 0f;


	private void Start()
	{
		#if !UNITY_EDITOR
		this.testButtonText.text = Utility.UseTextureFormat.ToString() + " RAW TEXTURE";
		#endif
		this.SetButtonInteractable(true);
	}

	private void Update()
	{
		this.loadingObject.Rotate(0f, 0f, -5f);
		this.UpdateFPS();
	}

	private void UpdateFPS()
	{
		this.frameCount++;
		float elapsed = Time.realtimeSinceStartup - this.prevTime;

		if (elapsed >= 0.5f) {
			this.fpsText.text = (this.frameCount / elapsed).ToString("F1") + " FPS";
			this.frameCount = 0;
			this.prevTime = Time.realtimeSinceStartup;
		}
	}


	public void SetProgressText(int total, int progress)
	{
		this.progressText.text = progress + "/" + total;
	}

	public void SetProcessedTime(string time)
	{
		this.progressText.text += string.Format(" ({0})", time);
	}

	public void SetButtonInteractable(bool interactable)
	{
		this.button1.interactable = interactable;
		this.button2.interactable = interactable;
		#if UNITY_EDITOR
		this.button3.interactable = false;
		#else
		this.button3.interactable = interactable;
		#endif
		this.button4.interactable = interactable;
	}
}