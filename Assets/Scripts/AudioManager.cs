using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class AudioManager : MonoBehaviour
{
    AudioClip myclip;
    AudioSource audioSource;
    [SerializeField] string micName;
    int samplingFrequency = 44100;
    int maxTime = 15;

    public float recordStartTime;
    private AudioClip recordedClip;

    public TextMeshProUGUI resultText;

    void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.Log("マイクが見つかりません");
            return;
        }

        micName = Microphone.devices[0];
        Debug.Log("使用マイク：" + micName);

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void StartButton()
    {
        Debug.Log("recording start!");
        myclip = Microphone.Start(micName, false, maxTime, samplingFrequency);
        recordStartTime = Time.time;
    }

    public void EndButton()
    {
        if (Microphone.IsRecording(micName))
        {
            Debug.Log("recording stopped");
            Microphone.End(micName);

            float recordDuration = Time.time - recordStartTime;
            if (recordDuration > maxTime) recordDuration = maxTime;

            int sampleLength = (int)(recordDuration * samplingFrequency) * myclip.channels;
            float[] samples = new float[sampleLength];
            myclip.GetData(samples, 0);

            recordedClip = AudioClip.Create("RecordedClip", sampleLength / myclip.channels, myclip.channels, samplingFrequency, false);
            recordedClip.SetData(samples, 0);

            myclip = recordedClip;
        }
    }

    public void SaveWav()
    {
        if (myclip == null)
        {
            Debug.LogWarning("録音データがありません.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, "myRecording.wav");
        WavUtility.FromAudioClip(myclip, path, true);
        Debug.Log("音声ファイルを保存しました：" + path);
    }

    public void PlayButton()
    {
        if (myclip == null)
        {
            Debug.LogWarning("再生データがありません.");
            return;
        }

        Debug.Log("再生開始");
        audioSource.clip = myclip;
        audioSource.Play();
    }

    public void UploadToServer()
    {
        string path = Path.Combine(Application.persistentDataPath, "myRecording.wav");
        if (File.Exists(path))
        {
            StartCoroutine(UploadCoroutine(path));
        }
        else
        {
            Debug.LogError("ファイルが見つかりません: " + path);
        }
    }

    IEnumerator UploadCoroutine(string filePath)
    {
        byte[] bytes = File.ReadAllBytes(filePath);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", bytes, "voice.wav", "audio/wav");
        form.AddField("include_audio", "true");

        using (UnityWebRequest www = UnityWebRequest.Post("https://172.21.1.123:8000/transcribe", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("送信失敗: " + www.error);
                if (resultText != null)
                    resultText.text = "送信失敗: " + www.error;
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("送信成功! レスポンス: " + responseText);

                // 🔽 TextMeshに表示
                if (resultText != null)
                {
                    resultText.text = responseText;
                }
            }
        }
    }
}
