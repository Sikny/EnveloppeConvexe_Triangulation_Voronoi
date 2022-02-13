using Geometry;
using UnityEditor;
using UnityEngine;

public class GeometryTests : MonoBehaviour {
    public Circle _circle;

    [Header("Triangle")] public Transform a;
    public Transform b;
    public Transform c;
    
    [ContextMenu("Draw Circumcircle")]
    private void DrawCircumcircle() {
        _circle = Circle.Circumcircle(a.position, b.position, c.position);
    }

    private void OnDrawGizmos() {
        if (a == null || b == null || c == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(a.position, b.position);
        Gizmos.DrawLine(b.position, c.position);
        Gizmos.DrawLine(a.position, c.position);
        
        if(_circle == null) return;
        Handles.color = Color.cyan;
        Handles.DrawWireDisc(_circle.center, Vector3.up, _circle.radius);
    }
}