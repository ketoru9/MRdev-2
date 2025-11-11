using UnityEngine;

public class AudioControl : MonoBehaviour
{
    public AudioManager audioManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            audioManager.StartButton();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            audioManager.EndButton();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            audioManager.PlayButton();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            audioManager.SaveWav();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            audioManager.UploadToServer();
        }
    }
}
