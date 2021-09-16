using UnityEngine;

public class VisibilityOnPlay : MonoBehaviour
{
    public bool visibleOnPlay;

    void Start()
    {
        gameObject.SetActive(visibleOnPlay);
    }
}
