using UnityEngine;

public class SpriteRotationFix : MonoBehaviour
{
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Ignorar rotación del padre completamente
        transform.rotation = Camera.main.transform.rotation;
    }

    public void SetDirection(Vector3 velocity)
    {
        if (sr == null) return;

        Vector3 dir = velocity.normalized;
        // Debug.Log("dirX: " + dir.x);

        // Flip según dirección X
        if (dir.x > 0.1f)
            sr.flipX = false; // va a la derecha
        else if (dir.x < -0.1f)
            sr.flipX = true;  // va a la izquierda
    }
}