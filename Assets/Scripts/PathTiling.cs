using UnityEngine;

/// <summary>
/// Da a todas las piezas la misma densidad de textura, tengan el tamano que
/// tengan.
///
/// El problema: las UV de un Plane van de 0 a 1 sobre toda su superficie, y el
/// tiling vive en el material, no en el objeto. Con un unico material compartido
/// por piezas de tamanos muy distintos, la textura sale estirada en las largas y
/// comprimida en las cortas.
///
/// La solucion: en vez de un numero de repeticiones fijo, se define cuantas
/// unidades de mundo ocupa UNA repeticion. Cada pieza calcula entonces las
/// repeticiones que le tocan segun su tamano real, y todas acaban con el mismo
/// tamano de texel. Se aplica con un MaterialPropertyBlock, que sobrescribe el
/// valor solo en este renderer sin instanciar un material nuevo.
///
/// OJO: ponlo solo en piezas cuya textura se REPITE (tramos rectos, cesped).
/// En una esquina o un borde, la textura es un dibujo unico pensado para cubrir
/// la pieza entera, y ahi lo que quieres es que se estire con ella.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PathTiling : MonoBehaviour
{
    [Tooltip("Unidades de mundo que ocupa una repeticion de la textura. " +
             "Tu camino mide 3 unidades de ancho, asi que con 3 la textura " +
             "entra justa a lo ancho. Sube el numero para ver la textura mas " +
             "grande, bajalo para que se repita mas veces. Manten los dos ejes " +
             "iguales si no quieres que los pixeles salgan deformados.")]
    public Vector2 worldUnitsPerTile = new Vector2(3f, 3f);

    // Shader.PropertyToID evita resolver la cadena en cada aplicacion.
    private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");
    private static readonly int MainTexST = Shader.PropertyToID("_MainTex_ST");

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MaterialPropertyBlock props;

    private Vector3 lastScale;
    private Vector2 lastUnitsPerTile;

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

        if (transform.lossyScale != lastScale || worldUnitsPerTile != lastUnitsPerTile)
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

        float sizeU = Mathf.Abs(meshSize.x * scale.x);

        // Plane: plano en XZ, la V corre por Z. Quad: plano en XY, corre por Y.
        float sizeV = Mathf.Approximately(meshSize.z, 0f)
            ? Mathf.Abs(meshSize.y * scale.y)
            : Mathf.Abs(meshSize.z * scale.z);

        float unitsU = Mathf.Max(0.0001f, worldUnitsPerTile.x);
        float unitsV = Mathf.Max(0.0001f, worldUnitsPerTile.y);

        // Repeticiones = cuanto mide la pieza entre lo que mide una repeticion.
        Vector4 st = new Vector4(sizeU / unitsU, sizeV / unitsV, 0f, 0f);

        if (props == null) props = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(props);

        props.SetVector(BaseMapST, st);   // URP Lit / Unlit
        props.SetVector(MainTexST, st);   // shaders antiguos, por si acaso

        meshRenderer.SetPropertyBlock(props);

        lastScale        = scale;
        lastUnitsPerTile = worldUnitsPerTile;
    }
}
