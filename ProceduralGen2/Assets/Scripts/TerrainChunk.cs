using UnityEngine;

public class TerrainChunk
{
    const float colliderGenerationDistanceThresh = 5f;
    public event System.Action<TerrainChunk, bool> OnVisibilityChanged;

    public Vector2 coord;

    GameObject meshObject;
    Vector2 sampleCentre;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LoDInfo[] detailLevels;
    LoDMesh[] lodMeshes;
    int colliderLODIndex;
    bool hasSetCollider;

    HeightMap heightMap;
    bool heightMapReceived;
    int previousLODIndex = -1;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;

    Transform viewer;
    float maxViewDistance;

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LoDInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.viewer = viewer;
        maxViewDistance = detailLevels[detailLevels.Length - 1].distanceThreshold;

        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshFilter = meshObject.AddComponent<MeshFilter>();

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisibility(false);

        lodMeshes = new LoDMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LoDMesh(detailLevels[i].lod);
            lodMeshes[i].UpdateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex)
            {
                lodMeshes[i].UpdateCallback += UpdateCollisionMesh;
            }
        }
    }

    public void RequestHeightMapData()
    {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
    }

    void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;

        UpdateTerrainChunk();
    }

    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public void UpdateTerrainChunk()
    {
        if (!heightMapReceived)
        {
            return;
        }

        float viewerDistFromClosestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

        bool wasVisible = IsVisible();
        bool visible = viewerDistFromClosestEdge <= maxViewDistance;

        if (visible)
        {
            int lodIndex = 0;

            for (int i = 0; i < detailLevels.Length - 1; i++)
            {
                if (viewerDistFromClosestEdge > detailLevels[i].distanceThreshold)
                {
                    lodIndex = i + 1;
                }
                else
                {
                    break;
                }
            }

            if (lodIndex != previousLODIndex)
            {
                LoDMesh lodMesh = lodMeshes[lodIndex];
                if (lodMesh.hasMesh)
                {
                    previousLODIndex = lodIndex;
                    meshFilter.mesh = lodMesh.mesh;

                }
                else if (!lodMesh.hasRequestedMesh)
                {
                    lodMesh.RequestMesh(heightMap, meshSettings);
                }
            }
        }

        SetVisibility(visible);

        if (wasVisible != visible)
        {
            if(OnVisibilityChanged != null)
            {
                OnVisibilityChanged(this, visible);
            }
        }
    }

    public void UpdateCollisionMesh()
    {
        if (!hasSetCollider)
        {
            float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold)
            {
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
                {
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            if (sqrDstFromViewerToEdge < colliderGenerationDistanceThresh * colliderGenerationDistanceThresh)
            {
                if (lodMeshes[colliderLODIndex].hasMesh)
                {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                }
            }
        }
    }

    public void SetVisibility(bool visible)
    {
        meshObject.SetActive(visible);
    }

    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }
}

class LoDMesh
{
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    public event System.Action UpdateCallback;

    public LoDMesh(int lod)
    {
        this.lod = lod;
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, lod, meshSettings), OnMeshDataReceived);
    }

    void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;

        UpdateCallback();
    }
}

