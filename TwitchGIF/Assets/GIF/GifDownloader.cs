using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GifDownloader : MonoBehaviour
{
    public GifDecoder decoder;

    public InputField input;

    public void DownloadGifButton()
    {
        StartCoroutine(Download());
    }

    private IEnumerator Download()
    {
        string url = input.text;

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError("Error downloading from: " + url);
                yield break;
            }
            else
            {
                byte[] b = www.downloadHandler.data; //Downloaded bytes

                if (b[0] != 'G' || b[1] != 'I' || b[2] != 'F')
                {
                    Debug.LogError("Error. Downloaded file is not a GIF file");
                    yield break;
                }

                StartCoroutine(decoder.ProcessGif(b));
            }
        }
    }
}
