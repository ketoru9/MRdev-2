using System;
using System.IO;
using System.Collections;
using UnityEngine;
using NativeWebSocket;

public class AudioManager : MonoBehaviour
{
    AudioClip myclip;
    AudioSource audioSource;
    [SerializeField] string micName;
    int samplingFrequency = 44100;
    int maxTime = 15;

    public float recordStartTime;
    private AudioClip recordedClip;

    [Header("結果表示用（サーバーのレスポンス）")]
    public TextMesh resultText;

    [Header("ログ表示用（動作状況など）")]
    public TextMesh logText;

    WebSocket websocket;

    async void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            LogMessage("マイクが見つかりません。");
            return;
        }

        micName = Microphone.devices[0];
        LogMessage("使用マイク：" + micName);

        audioSource = gameObject.AddComponent<AudioSource>();

        // WebSocket 初期化
        websocket = new WebSocket("ws://172.21.1.123:8000/ws");

        websocket.OnOpen += () =>
        {
            LogMessage("✓ WebSocket 接続成功！");
        };

        websocket.OnError += (e) =>
        {
            LogMessage("✗ WebSocket エラー: " + e);
        };

        websocket.OnClose += (e) =>
        {
            LogMessage("WebSocket 接続閉じました。Code: " + e);
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            LogMessage("受信: " + message.Substring(0, Math.Min(100, message.Length)) + "...");
            HandleMessage(message);
        };

        await websocket.Connect();
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    // 録音開始
    public void StartButton()
    {
        LogMessage("🎤 録音開始！");
        myclip = Microphone.Start(micName, false, maxTime, samplingFrequency);
        recordStartTime = Time.time;
    }

    // 録音終了
    public void EndButton()
    {
        if (!Microphone.IsRecording(micName))
        {
            LogMessage("⚠️ 録音中ではありません。");
            return;
        }

        Microphone.End(micName);
        LogMessage("⏹️ 録音停止");

        float recordDuration = Time.time - recordStartTime;
        if (recordDuration > maxTime) recordDuration = maxTime;

        int sampleLength = (int)(recordDuration * samplingFrequency) * myclip.channels;
        float[] samples = new float[sampleLength];
        myclip.GetData(samples, 0);

        recordedClip = AudioClip.Create("RecordedClip", sampleLength / myclip.channels, myclip.channels, samplingFrequency, false);
        recordedClip.SetData(samples, 0);

        myclip = recordedClip;
        LogMessage("✓ 録音データ作成完了 (" + recordDuration.ToString("F2") + "秒)");
    }

    // 音声再生
    public void PlayButton()
    {
        if (myclip == null)
        {
            LogMessage("⚠️ 再生データがありません。");
            return;
        }

        audioSource.clip = myclip;
        audioSource.Play();
        LogMessage("▶️ 音声を再生中...");
    }

    // WAV保存
    public void SaveWav()
    {
        if (myclip == null)
        {
            LogMessage("⚠️ 録音データがありません。保存できません。");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, "myRecording.wav");
        WavUtility.FromAudioClip(myclip, path, true);
        LogMessage("💾 音声ファイルを保存: " + path);
    }

    // 音声データを WebSocket で送信（文字起こし用）
    public async void SendAudioViaWebSocket()
    {
        if (myclip == null)
        {
            LogMessage("⚠️ 送信する音声がありません。");
            return;
        }

        // WebSocket 接続チェック
        if (websocket == null || websocket.State != WebSocketState.Open)
        {
            LogMessage("✗ WebSocket が接続されていません。State: " + (websocket?.State.ToString() ?? "null"));
            return;
        }

        try
        {
            LogMessage("🔄 音声データを変換中...");

            // AudioClip を WAV バイト配列に変換
            byte[] wavData = ConvertAudioClipToWav(myclip);
            LogMessage("✓ WAV変換完了: " + wavData.Length + " bytes");

            // Base64エンコード
            string base64Audio = Convert.ToBase64String(wavData);
            LogMessage("✓ Base64エンコード完了: " + base64Audio.Length + " chars");

            // シリアライズ可能なクラスを使用
            AudioPayload payload = new AudioPayload
            {
                type = "audio",
                data = base64Audio
            };

            // JSON文字列に変換
            string json = JsonUtility.ToJson(payload);
            LogMessage("📤 送信中... (" + json.Length + " chars)");

            // 送信
            await websocket.SendText(json);
            LogMessage("✓ 音声データを送信しました");
        }
        catch (Exception e)
        {
            LogMessage("✗ 送信エラー: " + e.Message + "\n" + e.StackTrace);
        }
    }

    // サーバーからのメッセージを処理
    private void HandleMessage(string json)
    {
        try
        {
            // まず type だけ取得
            ServerResponse response = JsonUtility.FromJson<ServerResponse>(json);

            if (response == null || string.IsNullOrEmpty(response.type))
            {
                LogMessage("⚠️ 不明なメッセージ形式");
                return;
            }

            switch (response.type)
            {
                case "connection":
                    LogMessage("🔗 接続確認: " + response.message);
                    break;

                case "processing":
                    LogMessage("⏳ 処理中: " + response.message);
                    if (resultText != null) resultText.text = "処理中...";
                    break;

                case "transcription":
                    LogMessage("📝 文字起こし結果: " + response.text);
                    if (resultText != null) resultText.text = "認識: " + response.text;
                    break;

                case "response":
                    LogMessage("💬 応答: " + response.response);
                    if (resultText != null)
                    {
                        string display = "【応答】\n" + response.response;
                        if (!string.IsNullOrEmpty(response.transcribed))
                        {
                            display = "【認識】" + response.transcribed + "\n\n" + display;
                        }
                        resultText.text = display;
                    }
                    break;

                case "error":
                    LogMessage("✗ サーバーエラー: " + response.message);
                    if (resultText != null) resultText.text = "エラー: " + response.message;
                    break;

                case "pong":
                    LogMessage("🏓 Pong受信");
                    break;

                default:
                    LogMessage("📨 その他メッセージ: " + response.type);
                    break;
            }
        }
        catch (Exception e)
        {
            LogMessage("✗ JSON解析エラー: " + e.Message);
            LogMessage("受信データ: " + json.Substring(0, Math.Min(200, json.Length)));
        }
    }

    // Ping送信（接続確認用）
    public async void SendPing()
    {
        if (websocket == null || websocket.State != WebSocketState.Open)
        {
            LogMessage("✗ WebSocket未接続");
            return;
        }

        try
        {
            PingPayload payload = new PingPayload { type = "ping" };
            string json = JsonUtility.ToJson(payload);
            await websocket.SendText(json);
            LogMessage("🏓 Ping送信");
        }
        catch (Exception e)
        {
            LogMessage("✗ Ping送信エラー: " + e.Message);
        }
    }

    // AudioClip → WAVバイト配列
    private byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];

        int rescaleFactor = 32767;
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            BitConverter.GetBytes(intData[i]).CopyTo(bytesData, i * 2);
        }

        // WAV全体サイズ = ヘッダー(44) + 音声データ
        byte[] wav = new byte[44 + bytesData.Length];

        // WAVヘッダー（44バイト）
        int byteRate = clip.frequency * clip.channels * 2;
        short blockAlign = (short)(clip.channels * 2);

        // "RIFF" チャンク
        wav[0] = 0x52; wav[1] = 0x49; wav[2] = 0x46; wav[3] = 0x46; // "RIFF"
        BitConverter.GetBytes(36 + bytesData.Length).CopyTo(wav, 4); // ChunkSize
        wav[8] = 0x57; wav[9] = 0x41; wav[10] = 0x56; wav[11] = 0x45; // "WAVE"

        // "fmt " チャンク
        wav[12] = 0x66; wav[13] = 0x6D; wav[14] = 0x74; wav[15] = 0x20; // "fmt "
        BitConverter.GetBytes(16).CopyTo(wav, 16);  // Subchunk1Size (16 for PCM)
        BitConverter.GetBytes((short)1).CopyTo(wav, 20); // AudioFormat (1 = PCM)
        BitConverter.GetBytes((short)clip.channels).CopyTo(wav, 22); // NumChannels
        BitConverter.GetBytes(clip.frequency).CopyTo(wav, 24); // SampleRate
        BitConverter.GetBytes(byteRate).CopyTo(wav, 28); // ByteRate
        BitConverter.GetBytes(blockAlign).CopyTo(wav, 32); // BlockAlign
        BitConverter.GetBytes((short)16).CopyTo(wav, 34); // BitsPerSample

        // "data" チャンク
        wav[36] = 0x64; wav[37] = 0x61; wav[38] = 0x74; wav[39] = 0x61; // "data"
        BitConverter.GetBytes(bytesData.Length).CopyTo(wav, 40); // Subchunk2Size

        // 音声データをコピー
        Buffer.BlockCopy(bytesData, 0, wav, 44, bytesData.Length);

        return wav;
    }

    // ログ表示
    void LogMessage(string message)
    {
        Debug.Log("[AudioManager] " + message);

        if (logText != null)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            logText.text = $"[{timestamp}] {message}\n" + logText.text;
            
            string[] lines = logText.text.Split('\n');
            if (lines.Length > 15)
            {
                logText.text = string.Join("\n", lines, 0, 15);
            }
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            LogMessage("WebSocket接続を閉じています...");
            await websocket.Close();
        }
    }

    private void OnDestroy()
    {
        if (websocket != null)
        {
            websocket.CancelConnection();
        }
    }
}

// ========== シリアライズ可能なクラス定義 ==========

[Serializable]
public class AudioPayload
{
    public string type;
    public string data;
}

[Serializable]
public class PingPayload
{
    public string type;
}

[Serializable]
public class ServerResponse
{
    public string type;           // "connection", "processing", "transcription", "response", "error"
    public string status;         // "connected", "success", "error"
    public string message;        // 一般的なメッセージ
    public string text;           // transcription用
    public string transcribed;    // response用の文字起こしテキスト
    public string response;       // response用の応答テキスト
    public string timestamp;      // タイムスタンプ
    public string server_time;    // サーバー時刻
    
    // classification は複雑なのでここでは省略
    // 必要なら ClassificationData クラスを作成
}

[Serializable]
public class ClassificationData
{
    public string text;
    public string category;
    public string @event;  // C#の予約語なので @ をつける
    public string date;
    public string time;
}