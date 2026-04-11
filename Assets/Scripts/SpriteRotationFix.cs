using UnityEngine;

public class SpriteRotationFix : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        // Siempre mirar hacia la cámara ignorando la rotación del padre
        transform.rotation = Quaternion.Euler(-40f, 0f, 0f); // mismo ángulo de la cámara
    }
}