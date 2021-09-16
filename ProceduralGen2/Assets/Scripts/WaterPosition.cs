using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPosition : MonoBehaviour
{
    public GameObject parent;
    float waterLevel;

    private void Update()
    {
        this.transform.position = new Vector3(parent.transform.position.x, waterLevel, parent.transform.position.z);
    }

    public void SetWaterLevel(float waterLevel)
    {
        this.waterLevel = waterLevel;
    }
}
