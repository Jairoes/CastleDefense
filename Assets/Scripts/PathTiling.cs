using UnityEngine;

/// <summary>
/// Mantiene constante la densidad de textura de una pieza, la estires como la
/// estires.
///
/// El problema: las UV de un Plane van de 0 a 1 sobre toda su superficie, asi
/// que al escalarlo de forma desigual la textura se aplasta. Y el tiling es una
/// propiedad del material, no del objeto, asi que tocarlo afecta a todas las
/// piezas que comparten ese material.
///
/// La solucion: tomar como referencia el tiling que ya tiene el material,
/// multiplicarlo por el tamano real de la pieza y aplicar el resultado con un
/// MaterialPropertyBlock, que sobrescribe el valor solo en este renderer sin
/// instanciar un material nuevo.
///
/// Por eso una pieza sin escalar se ve identica a como se ve hoy: no hay nada
/// que configurar.
///
/// OJO: ponlo solo en piezas cuya textura se REPITE (tramos rectos, cesped).
/// En una esquina o un borde, la textura es un dibujo unico pensado para cubrir
/// la pieza entera, y ahi lo que quieres es justamente que se estire con ella.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PathTiling : MonoBehaviour
{
    [Tooltip("Multiplicador extra sobre el tiling del material. Dejalo en 1,1 " +
             "para conservar la densidad actual. Subelo para que el patron se " +
             "repita mas veces, bajalo para verlo mas grande.")]
    public Vector2 textureScale = Vector2.one;

    // Shader.PropertyToID evita resolver la cadena en cada aplicacion.
    private static readonly int BaseMapID = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");
    private static readonly int MainTexST = Shader.PropertyToID("_MainTex_ST");

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MaterialPropertyBlock props;

    private Vector3 lastScale;
    private Vector2 lastTextureScale;

    void OnEnable()
    {
        Apply();
    }

    void OnValidate()
    {
        Apply();
    }

#if UNITY_EDITOR
    void Update()
    {
        // Mientras decoras, la escala se cambia arrastrando el gizmo y eso no
        // dispara OnValidate. En build no hace falta: las piezas no se escalan
        // en ejecucion, asi que basta con el OnEnable.
        if (Application.isPlaying) return;

        if (transform.lossyScale != lastScale || textureScale != lastTextureScale)
            Apply();
    }
#endif

    [ContextMenu("Aplicar ahora")]
    public void Apply()
    {
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

        if (meshRenderer == null || meshFilter == null || meshFilter.sharedMesh == null)
            return;

        // Tamano de la malla en sus propios ejes, ya escalado. Se usan los ejes
        // locales y no los bounds del mundo para que rotar la pieza (tus piezas
        // giradas -90 en Y) no intercambie los ejes de la textura.
        Vector3 meshSize = meshFilter.sharedMesh.bounds.size;
        Vector3 scale    = transform.lossyScale;

        float sizeU = meshSize.x * scale.x;

        // Plane: plano en XZ, la V corre por Z. Quad: plano en XY, corre por Y.
        float sizeV = Mathf.Approximately(meshSize.z, 0f)
            ? meshSize.y * scale.y
            : meshSize.z * scale.z;

        // El tiling del material es la referencia de densidad: con la pieza sin
        // escalar el factor vale 1 y el resultado es el del material tal cual.
        Vector2 baseTiling = Vector2.one;
        Material mat = meshRenderer.sharedMaterial;
        if (mat != null && mat.HasProperty(BaseMapID))
            baseTiling = mat.GetTextureScale(BaseMapID);

        // El Plane de Unity mide 10x10 unidades sin escalar.
        float tileU = baseTiling.x * (sizeU / 10f) * textureScale.x;
        float tileV = baseTiling.y * (sizeV / 10f) * textureScale.y;

        if (props == null) props = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(props);

        Vector4 st = new Vector4(tileU, tileV, 0f, 0f);
        props.SetVector(BaseMapST, st);   // URP Lit / Unlit
        props.SetVector(MainTexST, st);   // shaders antiguos, por si acaso

        meshRenderer.SetPropertyBlock(props);

        lastScale        = scale;
        lastTextureScale = textureScale;
    }
}
