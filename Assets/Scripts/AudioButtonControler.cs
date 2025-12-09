using UnityEngine;

public class AudioButtonControler : MonoBehaviour
{
    public AudioManager audioManager;

    [Header("ボタン設定")]
    [Tooltip("録音開始ボタン（デフォルト: 右トリガー）")]
    public OVRInput.Button startRecordButton = OVRInput.Button.PrimaryIndexTrigger;
    public OVRInput.Controller startRecordController = OVRInput.Controller.RTouch;

    [Tooltip("録音停止ボタン（デフォルト: 左トリガー）")]
    public OVRInput.Button stopRecordButton = OVRInput.Button.PrimaryIndexTrigger;
    public OVRInput.Controller stopRecordController = OVRInput.Controller.LTouch;

    [Tooltip("再生ボタン（デフォルト: 左Aボタン）")]
    public OVRInput.Button playButton = OVRInput.Button.One;
    public OVRInput.Controller playController = OVRInput.Controller.LTouch;

    [Tooltip("保存ボタン（デフォルト: 右Aボタン）")]
    public OVRInput.Button saveButton = OVRInput.Button.One;
    public OVRInput.Controller saveController = OVRInput.Controller.RTouch;

    [Tooltip("送信ボタン（デフォルト: 右Bボタン）")]
    public OVRInput.Button sendButton = OVRInput.Button.Two;
    public OVRInput.Controller sendController = OVRInput.Controller.RTouch;

    [Tooltip("Ping送信ボタン（デフォルト: 左Bボタン）")]
    public OVRInput.Button pingButton = OVRInput.Button.Two;
    public OVRInput.Controller pingController = OVRInput.Controller.LTouch;

    [Header("ハプティックフィードバック")]
    public bool enableHaptics = true;
    public float hapticsFrequency = 0.5f;
    public float hapticsAmplitude = 0.5f;

    private bool isRecording = false;

    void Start()
    {
        if (audioManager == null)
        {
            Debug.LogError("[AudioButtonControler] AudioManagerが設定されていません！");
        }
    }

    void Update()
    {
        if (audioManager == null) return;

        // 録音開始（右トリガー）
        if (OVRInput.GetDown(startRecordButton, startRecordController))
        {
            audioManager.StartButton();
            isRecording = true;
            PlayHapticFeedback(startRecordController);
        }

        // 録音停止（左トリガー）
        if (OVRInput.GetDown(stopRecordButton, stopRecordController))
        {
            audioManager.EndButton();
            isRecording = false;
            PlayHapticFeedback(stopRecordController);
        }

        // 再生（左Aボタン）
        if (OVRInput.GetDown(playButton, playController))
        {
            audioManager.PlayButton();
            PlayHapticFeedback(playController);
        }

        // WAV保存（右Aボタン）
        if (OVRInput.GetDown(saveButton, saveController))
        {
            audioManager.SaveWav();
            PlayHapticFeedback(saveController);
        }

        // WebSocket送信（右Bボタン）
        if (OVRInput.GetDown(sendButton, sendController))
        {
            audioManager.SendAudioViaWebSocket();
            PlayHapticFeedback(sendController, 0.8f); // 送信時は強めの振動
        }

        // Ping送信（左Bボタン）
        if (OVRInput.GetDown(pingButton, pingController))
        {
            audioManager.SendPing();
            PlayHapticFeedback(pingController, 0.3f); // Pingは弱めの振動
        }
    }

    /// <summary>
    /// ハプティックフィードバックを再生
    /// </summary>
    private void PlayHapticFeedback(OVRInput.Controller controller, float amplitude = -1f)
    {
        if (!enableHaptics) return;

        float finalAmplitude = amplitude < 0 ? hapticsAmplitude : amplitude;
        OVRInput.SetControllerVibration(hapticsFrequency, finalAmplitude, controller);

        // 0.1秒後に振動を停止
        Invoke(nameof(StopHaptics), 0.1f);
    }

    private void StopHaptics()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }

    /// <summary>
    /// 現在の録音状態を取得
    /// </summary>
    public bool IsRecording()
    {
        return isRecording;
    }

    void OnDestroy()
    {
        // クリーンアップ：振動を停止
        StopHaptics();
    }
}