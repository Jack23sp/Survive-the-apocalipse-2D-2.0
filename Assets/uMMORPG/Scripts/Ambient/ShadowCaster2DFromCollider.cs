using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(ShadowCaster2D))]
[DefaultExecutionOrder(100)]
public class ShadowCaster2DFromCollider : MonoBehaviour
{

    static readonly FieldInfo _meshField;
    static readonly FieldInfo _shapePathField;
    static readonly MethodInfo _generateShadowMeshMethod;

    ShadowCaster2D _shadowCaster;

    EdgeCollider2D _edgeCollider;
    PolygonCollider2D _polygonCollider;

    static ShadowCaster2DFromCollider()
    {
        _meshField = typeof(ShadowCaster2D).GetField("m_Mesh", BindingFlags.NonPublic | BindingFlags.Instance);
        _shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);

        _generateShadowMeshMethod = typeof(ShadowCaster2D)
                                    .Assembly
                                    .GetType("UnityEngine.Rendering.Universal.ShadowUtility")
                                    .GetMethod("GenerateShadowMesh", BindingFlags.Public | BindingFlags.Static);
    }

    private void Start()
    {
        _shadowCaster = GetComponent<ShadowCaster2D>();
        _edgeCollider = GetComponent<EdgeCollider2D>();

        if (_edgeCollider == null)
            _polygonCollider = GetComponent<PolygonCollider2D>();

        UpdateShadow();
    }

    public void UpdateShadow()
    {
        var points = _polygonCollider == null
            ? _edgeCollider.points
            : _polygonCollider.points;

        Vector3[] vertices3D = System.Array.ConvertAll<Vector2, Vector3>(points, v => v);
        _shapePathField.SetValue(_shadowCaster, vertices3D);
        _meshField.SetValue(_shadowCaster, new Mesh());
        _generateShadowMeshMethod.Invoke(_shadowCaster, new object[] { _meshField.GetValue(_shadowCaster), _shapePathField.GetValue(_shadowCaster) });
    }
}