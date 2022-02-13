using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GeometryManager3D : MonoBehaviour
{
    public ConvexHull convex;

    public static GeometryManager3D instance;

    public List<Transform> points;
    public List<Vector3> convexHull;
    public List<int> index;
    public List<Vector3> normals;

    private MeshRenderer render;
    private MeshFilter filter;
    public Mesh mesh;

    [SerializeField]
    private Material material;
    
    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        instance = this;
        
        points = new List<Transform>();
        convexHull = new List<Vector3>();
        index = new List<int>();
        normals = new List<Vector3>();
        
        mesh = new Mesh();
        mesh.name = "convexHull";
        
        if (!filter)
        {
            filter = transform.gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;
        }
        if(!render)
            render = transform.gameObject.AddComponent<MeshRenderer>();
        render.material = material;
    }

    public void InitConvexHull()
    {
        Convex();
        //return;
        mesh.vertices = SetVertices();
        mesh.triangles = GenerateTriangles(convex.faces);
        //mesh.normals = CalculateNormals(convex.faces).ToArray();
        mesh.RecalculateNormals();
    }

    //The Jokes on You
    public void Voronoi()
    {
        List<Vector3> centers = new List<Vector3>();
        foreach (var f in convex.faces)
        {
            /*
            var middle = (f.a.pos + f.b.pos + f.c.pos)/ 3;
            Debug.DrawLine(middle, (f.a.pos + f.b.pos)/2, Color.red);
            Debug.DrawLine(middle, (f.a.pos + f.c.pos)/2, Color.red);
            Debug.DrawLine(middle, (f.b.pos + f.c.pos)/2, Color.red);
            */

            Vector3 center = new Vector3();

            var v1 = f.b.pos - f.a.pos;
            var v2 = f.c.pos - f.a.pos;

            var k1 = 0.5 * Vector3.Dot(v2, v2) * (Vector3.Dot(v1, v1) - Vector3.Dot(v1, v2)) / (Vector3.Dot(v1, v1) * Vector3.Dot(v2, v2) - Vector3.Dot(v1, v2)* Vector3.Dot(v1, v2));
            var k2 = 0.5 * Vector3.Dot(v1, v1) * (Vector3.Dot(v2, v2) - Vector3.Dot(v1, v2)) / (Vector3.Dot(v1, v1) * Vector3.Dot(v2, v2) - Vector3.Dot(v1, v2)* Vector3.Dot(v1, v2));

            center.x = (float)(f.a.pos.x + (k1 * v1.x) + (k2 * v2.x));
            center.y = (float)(f.a.pos.y + (k1 * v1.y) + (k2 * v2.y));
            center.z = (float)(f.a.pos.z + (k1 * v1.z) + (k2 * v2.z));

            Debug.DrawLine((f.a.pos + f.b.pos)/2, center, Color.red);
            Debug.DrawLine((f.a.pos + f.c.pos)/2, center, Color.red);
            Debug.DrawLine((f.b.pos + f.c.pos)/2, center, Color.red);

            centers.Add(center);
            Debug.Log(center);
        }
    }

    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1,
        Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < 0.0001f
                && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2)
                    / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

    public Vector3[] SetVertices()
    {
        convexHull.Clear();

        foreach(ConvexHull.Vertex vertex in convex.vertices)
        { 
            convexHull.Add(vertex.pos);
        }
        
        return convexHull.ToArray();
    }

    public int[] GenerateTriangles(List<ConvexHull.Face> faces)
    {
        index.Clear();

        for (int i = 0; i < faces.Count; i++)
        {
            index.Add(faces[i].a.index);
            index.Add(faces[i].b.index);
            index.Add(faces[i].c.index);

            index.Add(faces[i].b.index);
            index.Add(faces[i].a.index);
            index.Add(faces[i].c.index);
        }
        return index.ToArray();
    }

    public List<Vector3> CalculateNormals(List<ConvexHull.Face> faces)
    {
        normals.Clear();

        for(int i = 0; i < faces.Count; i++)
        {
            var n = GetNormal(faces[i], convex.vertices);
            normals.Add(n);
        }

        return normals;
    }

    public Vector3 GetNormal(ConvexHull.Face face, List<ConvexHull.Vertex> vertices)
    {
        Vector3 ab = face.b.pos - face.a.pos;
        Vector3 ac = face.c.pos - face.a.pos;

        var normal1 = Vector3.Cross(ab, ac).normalized;
        var normal2 = Vector3.Cross(ac,ab).normalized;

        Vector3 normal;
        ConvexHull.Vertex otherPoint = vertices.FirstOrDefault(
                v => !v.Equals(face.a) && !v.Equals(face.b) && !v.Equals(face.c)
            );
        Vector3 dirToOtherPoint = otherPoint.pos - face.a.pos;
        
        normal = (Vector3.Dot(normal1, dirToOtherPoint) >= 0) ? normal2 : normal1;
        
        return normal;
    }

    private void Convex()
    {
        convex = CreateBase();

        /*
        Debug.Log("normales :" + normals.Count);
        Debug.Log("faces :" + mesh.triangles.Length);
        Debug.Log("vertices :" + convex.vertices.Count);
        */

        UpdateConvexHull();
        for (int i = 0; i < convex.vertices.Count; i++)
        {
            convex.vertices[i].index = i;
        }
        Voronoi();
    }

    private ConvexHull CreateBase()
    {
        ConvexHull hull = new ConvexHull();

        List<Vector3> basePoints = points.Select(p => p.position).Take(4).ToList();
        List<ConvexHull.Vertex> vertices = basePoints.Select((pos,i) => new ConvexHull.Vertex(pos, i)).ToList();

        ConvexHull.Edge e01 = new ConvexHull.Edge(vertices[0], vertices[1]);
        ConvexHull.Edge e02 = new ConvexHull.Edge(vertices[0], vertices[2]);
        ConvexHull.Edge e12 = new ConvexHull.Edge(vertices[1], vertices[2]);
        ConvexHull.Edge e30 = new ConvexHull.Edge(vertices[3], vertices[0]);
        ConvexHull.Edge e23 = new ConvexHull.Edge(vertices[2], vertices[3]);
        ConvexHull.Edge e31 = new ConvexHull.Edge(vertices[3], vertices[1]);

        ConvexHull.Face f012 = new ConvexHull.Face(vertices[0], vertices[1], vertices[2]);
        ConvexHull.Face f013 = new ConvexHull.Face(vertices[0], vertices[1], vertices[3]);
        ConvexHull.Face f123 = new ConvexHull.Face(vertices[1], vertices[2], vertices[3]);
        ConvexHull.Face f023 = new ConvexHull.Face(vertices[0], vertices[2], vertices[3]);

        e01.f1 = f012;
        e01.f2 = f013;
        e02.f1 = f012;
        e02.f2 = f023;
        e12.f1 = f012;
        e12.f2 = f123;
        e30.f1 = f013;
        e30.f2 = f023;
        e23.f1 = f023;
        e23.f2 = f123;
        e31.f1 = f013;
        e31.f2 = f123;

        hull.vertices = vertices;
        hull.edges = new List<ConvexHull.Edge> {e01, e02, e12, e30, e23, e31};
        hull.faces = new List<ConvexHull.Face> {f012,f013,f123,f023};

        return hull;
    }

    private void UpdateConvexHull()
    {
        
        for (int i = 4; i < points.Count; i++)
        {
            //var normalesLocal = CalculateNormals(convex.faces, convex.vertices).ToArray();
            var p = points[i];

            List<ConvexHull.Face> visible = new List<ConvexHull.Face>();
            for (int j = 0; j < convex.faces.Count; j++)
            {

                var dirToPoint = p.position - convex.faces[j].a.pos;
                var normaleFace = GetNormal(convex.faces[j],convex.vertices);
                if (Vector3.Dot(dirToPoint, normaleFace) >= 0)
                {
                    visible.Add(convex.faces[j]);
                }
            }

            bool isInside = (visible.Count == 0);
            if (!isInside)
            {
                List<ConvexHull.Edge> visibleEdges = new List<ConvexHull.Edge>();
                List<ConvexHull.Vertex> visibleVertices = new List<ConvexHull.Vertex>();

                foreach(var edge in convex.edges)
                {
                    bool f1Visible = visible.Contains(edge.f1);
                    bool f2Visible = visible.Contains(edge.f2);


                    if (f1Visible && f2Visible)
                    {
                        visibleEdges.Add(edge);

                        if (!visibleVertices.Contains(edge.v1) && !edge.v1.isOneVisibleAndInvisible) visibleVertices.Add(edge.v1);
                        if (!visibleVertices.Contains(edge.v2) && !edge.v2.isOneVisibleAndInvisible) visibleVertices.Add(edge.v2);
                    }
                    if (!f1Visible && !f2Visible)
                    {
                        edge.isOneVisibleAndInvisible = false;

                    }
                    else
                    {
                        edge.isOneVisibleAndInvisible = true;
                        edge.v1.isOneVisibleAndInvisible = true;
                        edge.v2.isOneVisibleAndInvisible = true;

                        visibleVertices.Remove(edge.v1);
                        visibleVertices.Remove(edge.v2);
                    }
                }

                //Thanos
                for (int findex = 0; findex < visible.Count; findex++)
                {
                    convex.faces.Remove(visible[findex]);
                }
                for(int eindex = 0; eindex < visibleEdges.Count; eindex++)
                {
                    convex.edges.Remove(visibleEdges[eindex]);
                }
                for(int vindex = 0; vindex < visibleVertices.Count; vindex++)
                {
                    convex.vertices.Remove(visibleVertices[vindex]);
                }

                //Créer les nouvelles faces et les nouveaux points, alias nouvelles victimes
                List<ConvexHull.Edge> isOneVisibleEdge = convex.edges.Where(edge => edge.isOneVisibleAndInvisible).ToList();
                List<ConvexHull.Vertex> isOneVisibleVertex = convex.vertices.Where(vertex => vertex.isOneVisibleAndInvisible).ToList();
                List<ConvexHull.Edge> newEdges = new List<ConvexHull.Edge>();
                List<ConvexHull.Face> newFaces = new List<ConvexHull.Face>();

                ConvexHull.Vertex newP = new ConvexHull.Vertex(p.position, i);
                foreach(var vertex in isOneVisibleVertex)
                {
                    ConvexHull.Edge edge = new ConvexHull.Edge(newP, vertex);
                    newEdges.Add(edge);
                    convex.edges.Add(edge);
                }

                foreach(var edge in isOneVisibleEdge)
                {
                    ConvexHull.Face newFace = new ConvexHull.Face(edge.v1, edge.v2, newP);
                    Vector3 center = (newFace.a.pos + newFace.b.pos + newFace.c.pos) / 3;

                    Vector3 pointToLarry = center - newFace.a.pos;
                    Vector3 pointToBateau = newFace.b.pos - newFace.a.pos;

                    Vector3 normal = GetNormal(newFace, convex.vertices);
                    Vector3 up = Vector3.Cross(pointToLarry, pointToBateau);


                    if (Vector3.Dot(normal, up) >= 0)
                    {
                        newFace.c = newFace.b;
                        newFace.b = newP;
                    }

                    if (visible.Contains(edge.f1)) edge.f1 = newFace;
                    else edge.f2 = newFace;

                    newFaces.Add(newFace);
                    
                    convex.faces.Add(newFace);
                }

                convex.vertices.Add(newP);

                foreach (var edge in newEdges)
                {
                    foreach(var face in newFaces)
                    {
                        var otherVertex = edge.v1.Equals(newP)? edge.v2 : edge.v1;
                        bool checkAB = CheckEdge(face.a, face.b, edge.v1, edge.v2);
                        bool checkBC = CheckEdge(face.b, face.c, edge.v1, edge.v2);
                        bool checkCA = CheckEdge(face.a, face.c, edge.v1, edge.v2);

                        /*if(!checkAB && !checkBC && !checkAB)
                        {
                            continue;
                        }*/
                        if (checkAB)
                        {
                            if (newP == face.a) edge.f1 = face;
                            else edge.f2 = face;
                        }
                        if (checkBC)
                        {
                            if (newP == face.b) edge.f1 = face;
                            else edge.f2 = face;
                        }
                        if (checkCA)
                        {
                            if (newP == face.c) edge.f1 = face;
                            else edge.f2 = face;
                        }
                    }
                }
            }
            
        }
        
    }

    private bool CheckEdge(ConvexHull.Vertex a, ConvexHull.Vertex b, ConvexHull.Vertex c, ConvexHull.Vertex d)
    {
        return ((a == c && b == d) || (a ==d) && (b==c));
    }
}
