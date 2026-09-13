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
/// La solucion: calcular el tiling a partir del tamano real de la pieza y
/// aplicarlo con un MaterialPropertyBlock, que sobrescribe el valor solo en
/// este renderer sin crear un material nuevo.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PathTiling : MonoBehaviour
{
    [Tooltip("Densidad de la textura. 1 = la pieza sin escalar se ve igual que " +
             "ahora. Subelo para que el patron se repita mas veces (textura mas " +
             "pequena), bajalo para verlo mas grande.")]
    public float textureScale = 1f;

    // Los nombres de propiedad se resuelven una vez: Shader.PropertyToID evita
    // buscar la cadena en cada aplicacion.
    private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");
    private static readonly int MainTexST = Shader.PropertyToID("_MainTex_ST");

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MaterialPropertyBlock props;

    private Vector3 lastScale;
    private float lastTextureScale;

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
        // en tiempo de ejecucion, asi que basta con el OnEnable.
        if (Application.isPlaying) return;

        if (transform.lossyScale != lastScale ||
            !Mathf.Approximately(textureScale, lastTextureScale))
        {
            Apply();
        }
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

        // El Plane de Unity mide 10x10 sin escalar, y con tiling 1 la textura
        // lo cubre entero. Dividir entre 10 hace que textureScale = 1 deje las
        // piezas sin escalar exactamente como se ven hoy.
        float tileU = (sizeU / 10f) * textureScale;
        float tileV = (sizeV / 10f) * textureScale;

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
