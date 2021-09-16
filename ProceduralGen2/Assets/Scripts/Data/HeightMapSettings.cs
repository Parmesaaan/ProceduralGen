using UnityEngine;

[CreateAssetMenu]
public class HeightMapSettings : UpdatableData
{
    [Header("Noise")]
    public NoiseSettings noiseSettings;

    [Header("Falloff")]
    public bool useFalloff;
    public float falloffModifierA = 3f;
    public float falloffModifierB = 2.2f;

    [Header("Mesh")]
    public AnimationCurve heightCurve;
    public float heightMultiplier;

    [Header("Misc")]
    public float waterLevel;

    public float minHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(0);
        }
    }

    public float maxHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(1);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        noiseSettings.ValidateValues();
        base.OnValidate();
    }
#endif
}
