using UnityEngine;

/// <summary>
/// Mantiene el sprite mirando a la camara, ignorando la rotacion del padre.
/// </summary>
public class SpriteRotationFix : MonoBehaviour
{
    private SpriteRenderer sr;
    private Transform camTransform;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Camera.main hace una busqueda por tag en cada llamada. Con decenas de
        // enemigos en pantalla eso eran cientos de busquedas por frame, asi que
        // se resuelve una vez y se guarda.
        if (camTransform == null)
        {
            if (Camera.main == null) return;
            camTransform = Camera.main.transform;
        }

        transform.rotation = camTransform.rotation;
    }

    public void SetDirection(Vector3 velocity)
    {
        if (sr == null) return;

        float dirX = velocity.x;

        if (dirX > 0.1f)
            sr.flipX = false;   // va a la derecha
        else if (dirX < -0.1f)
            sr.flipX = true;    // va a la izquierda
    }
}
