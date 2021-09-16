using UnityEngine;

public static class TextureGenerator {
    public static Texture2D ColourMapTex(Color[] colourMap, int width, int height) {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D HeightMapTex(HeightMap heightMap) {
        int width = heightMap.values.GetLength(0);
        int height = heightMap.values.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(heightMap.minValue, heightMap.maxValue, heightMap.values[x, y]));    // Lerp will linearly interpolate between colours
            }
        }

        return ColourMapTex(colourMap, width, height);
    }
}
