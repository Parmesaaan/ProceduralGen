using UnityEngine;

public class MapPreview : MonoBehaviour {

    [Header("Modes")]
    public DrawMode drawMode;
    public bool autoUpdate;

    [Header("Mesh")]
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int editorLevelOfDetail;

    [Header("Settings")]
    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureSettings textureData;

    [Header("Misc")]
    public Material terrainMaterial;
    public Renderer textureRenderer;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;


    public void DrawTexture(Texture2D texture) {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        textureRenderer.gameObject.SetActive(true);
        meshRenderer.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData) {
        meshFilter.sharedMesh = meshData.CreateMesh();

        textureRenderer.gameObject.SetActive(false);
        meshRenderer.gameObject.SetActive(true);
    }

    public void DrawMapInEditor()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine, heightMapSettings, Vector2.zero);

        if (drawMode == DrawMode.NoiseMap)
        {
            DrawTexture(TextureGenerator.HeightMapTex(heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, editorLevelOfDetail, meshSettings));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            DrawTexture(TextureGenerator.HeightMapTex(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVerticesPerLine, heightMapSettings.falloffModifierA, heightMapSettings.falloffModifierB), 0, 1)));
        }
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    void OnValidate()
    {
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    public enum DrawMode
    {
        NoiseMap,
        Mesh,
        FalloffMap
    };
}
