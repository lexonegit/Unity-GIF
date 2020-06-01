using System.Collections;
using UnityEngine;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System;
using System.Threading.Tasks;

[Serializable]
public class Gif
{
    public int frameCount = 0;
    public Texture2D packedTexture;

    public Rect[] atlas;
    public float[] delays;

    public Gif(int frameCount)
    {
        this.frameCount = frameCount;
        this.delays = new float[frameCount]; 
    }
}

public class GifDecoder : MonoBehaviour
{
    public GifPlayerExample example;

    public class GifInfo
    {
        public FrameInfo[] frames;
        public int frameCount = 0;

        public GifInfo(int frameCount)
        {
            this.frameCount = frameCount;
            this.frames = new FrameInfo[frameCount];
        }
    };

    public class FrameInfo
    {
        public Color32[] colors;
        public int width, height;
        public int delay = 0;
    };

    public IEnumerator ProcessGif(byte[] b)
    {
        GifInfo gifInfo = null;

        //Debug.Log("Gif decode task started");

        Task.Run(() => gifInfo = DecodeGIFTask(b)); //Start decoding gif
        yield return new WaitUntil(() => gifInfo != null); //Wait for decoding to finish

        //Debug.Log("Gif decode task completed");

        Gif gif = new Gif(gifInfo.frameCount);
        Texture2D[] textures = new Texture2D[gifInfo.frameCount];

        for (int i = 0; i < gifInfo.frameCount; ++i)
        {
            FrameInfo currentFrameInfo = gifInfo.frames[i];

            Texture2D frameTexture = new Texture2D(currentFrameInfo.width, currentFrameInfo.height, TextureFormat.RGBA32, false);

            frameTexture.SetPixels32(currentFrameInfo.colors);
            frameTexture.Apply();

            textures[i] = frameTexture;
            gif.delays[i] = currentFrameInfo.delay;

            yield return null;
        }

        int textureSize = 4096;
        Texture2D packTexture = new Texture2D(textureSize, textureSize);

        gif.atlas = packTexture.PackTextures(textures, 2, textureSize, true);
        gif.packedTexture = packTexture;

        //Play the animation
        example.PlayMaterialAnimation(gif);
    }

    public GifInfo DecodeGIFTask(byte[] b)
    {
        Debug.Log("Started decoding gif...");
        DateTime startTime = DateTime.Now;

        Image gifImage = Image.FromStream(new MemoryStream(b));
        FrameDimension dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
        int frameCount = gifImage.GetFrameCount(dimension);

        GifInfo gifInfo = new GifInfo(frameCount);

        int index = -4;
        for (int i = 0; i < frameCount; ++i)
        {
            gifImage.SelectActiveFrame(dimension, i);
            Bitmap bitmap = new Bitmap(gifImage.Width, gifImage.Height);
            System.Drawing.Graphics.FromImage(bitmap).DrawImage(gifImage, Point.Empty);

            FrameInfo frameInfo = new FrameInfo();

            frameInfo.width = bitmap.Width;
            frameInfo.height = bitmap.Height;
            frameInfo.delay = BitConverter.ToInt16(gifImage.GetPropertyItem(20736).Value, index += 4) * 10; //Get frame delay value

            LockBitmap lockBitmap = new LockBitmap(bitmap);
            lockBitmap.LockBits();

            frameInfo.colors = new Color32[lockBitmap.Height * lockBitmap.Width];

            for (int x = 0; x < lockBitmap.Width; ++x)
            {
                for (int y = 0; y < lockBitmap.Height; ++y)
                {
                    System.Drawing.Color sourceColor = lockBitmap.GetPixel(x, y);
                    frameInfo.colors[(lockBitmap.Height - y - 1) * lockBitmap.Width + x] = new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A);
                }
            }

            gifInfo.frames[i] = frameInfo;
        }

        Debug.Log("Finished decoding GIF. Elapsed time: " + (DateTime.Now - startTime).TotalSeconds + " seconds.");

        return gifInfo;
    }
}