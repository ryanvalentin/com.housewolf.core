using UnityEngine;

public static class TextureExtensions
{
    public static Texture2D ToTexture2D(this RenderTexture rTex, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.name = rTex.name;
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        return tex;
    }
}
