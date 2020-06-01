using System.Collections;
using UnityEngine;

public class GifPlayerExample : MonoBehaviour
{
    public Material material;

    public void PlayMaterialAnimation(Gif gif)
    {
        StopAllCoroutines();
        StartCoroutine(FrameLoop(gif));
    }

    private IEnumerator FrameLoop(Gif gif)
    {
        material.mainTexture = gif.packedTexture;
        int i = 0;

        while (true)
        {
            material.mainTextureScale = new Vector2(gif.atlas[i].width, gif.atlas[i].height);
            material.mainTextureOffset = new Vector2(gif.atlas[i].x, gif.atlas[i].y);

            if (i < gif.frameCount - 1)
                i++;
            else
                i = 0;

            yield return new WaitForSeconds(gif.delays[i] / 1000);
        }
    }
}
