using UnityEngine;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {
    const float chunkUpdateThreshByViewer = 25f;
    const float squaredChunkUpdateThreshByViewer = chunkUpdateThreshByViewer * chunkUpdateThreshByViewer;

    [Header("LoD")]
    public int colliderLODIndex;
    public LoDInfo[] detailLevels;

    [Header("Game Objects")]
    public Transform viewer;
    public Material terrainMaterial;
    public WaterPosition waterPosition;

    [Header("Topography")]
    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureSettings textureSettings;

    Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    float meshWorldSize;
    int chunksVisible;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private void Start() {
        textureSettings.ApplyToMaterial(terrainMaterial);
        textureSettings.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDistance = detailLevels[detailLevels.Length - 1].distanceThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisible = Mathf.RoundToInt(maxViewDistance / meshWorldSize);

        waterPosition.SetWaterLevel(heightMapSettings.waterLevel);

        UpdateVisibleChunks();
    }

    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if(viewerPosition != viewerPositionOld)
        {
            foreach(TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if((viewerPositionOld - viewerPosition).sqrMagnitude > squaredChunkUpdateThreshByViewer) {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks() {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--) {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int y = -chunksVisible; y <= chunksVisible; y++) {
            for (int x = -chunksVisible; x <= chunksVisible; x++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkX + x, currentChunkY + y);

                if(!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if (terrainChunkDict.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, terrainMaterial);
                        terrainChunkDict.Add(viewedChunkCoord, newChunk);
                        newChunk.OnVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.RequestHeightMapData();
                    }
                }
            }
        }
    }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if(isVisible)
        {
            visibleTerrainChunks.Add(chunk);
        } else
        {
            visibleTerrainChunks.Remove(chunk);
        }
    }
}

[System.Serializable]
public struct LoDInfo
{
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int lod;
    public float distanceThreshold;

    public float sqrVisibleDstThreshold
    {
        get
        {
            return distanceThreshold * distanceThreshold;
        }
    }
}