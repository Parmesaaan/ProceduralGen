using UnityEngine;

[CreateAssetMenu]
public class MeshSettings : UpdatableData
{
    public float meshScale = 2f;
    public bool useFlatshading;

    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9;
    public const int numSupportedFlatshadedChunkSizes = 3;
    public static readonly int[] supportedChunkSizes = {48, 72, 96, 120, 144, 168, 192, 216, 240};
    public static readonly int[] supportedFlatshadedChunkSizes = {48, 72, 96};

    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;
    [Range(0, numSupportedFlatshadedChunkSizes - 1)]
    public int flatshadedChunkSizeIndex;

    // The number of vertices per line of a mesh rendered at LOD = 0
    // Includes +2 used for calculating normals, -1 as index starts from 0
    public int numVerticesPerLine
    {
        get
        {
            return supportedChunkSizes[(useFlatshading) ? flatshadedChunkSizeIndex : chunkSizeIndex] + 5;
        }
    }
    
    public float meshWorldSize
    {
        get
        {
            return (numVerticesPerLine - 3) * meshScale;
        }
    }
}
