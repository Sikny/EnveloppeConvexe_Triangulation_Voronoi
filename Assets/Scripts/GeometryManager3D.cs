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

    public List<Vector3> CalculateNormals(List<ConvexHull.Face> faces, List<ConvexHull.Vertex> vertices)
    {
        var normals = new List<Vector3>();

        for(int i = 0; i < faces.Count; i++)
        {
            Vector3 ab = faces[i].b.pos - faces[i].a.pos;
            Vector3 ac = faces[i].c.pos - faces[i].a.pos;

            var normal1 = Vector3.Cross(ab, ac).normalized;
            var normal2 = -normal1;

            Vector3 normal;
            ConvexHull.Vertex otherPoint = vertices.FirstOrDefault(
                    v => !v.Equals(faces[i].a) && !v.Equals(faces[i].b) && !v.Equals(faces[i].c)
                );
            Vector3 dirToOtherPoint = otherPoint.pos - faces[i].a.pos;
            normal = (Vector3.Dot(normal1, dirToOtherPoint) > 0)? normal2 : normal1;

            normals.Add(normal);
        }

        return normals;
    }

    public Vector3 GetNormal(ConvexHull.Face faces, List<ConvexHull.Vertex> vertices)
    {
        Vector3 ab = faces.b.pos - faces.a.pos;
        Vector3 ac = faces.c.pos - faces.a.pos;

        var normal1 = Vector3.Cross(ab, ac).normalized;
        var normal2 = -normal1;

        Vector3 normal;
        ConvexHull.Vertex otherPoint = vertices.FirstOrDefault(
                v => !v.Equals(faces.a) && !v.Equals(faces.b) && !v.Equals(faces.c)
            );
        Vector3 dirToOtherPoint = otherPoint.pos - faces.a.pos;
        normal = (Vector3.Dot(normal1, dirToOtherPoint) > 0) ? normal2 : normal1;


        return normal;
    }

    private void Convex()
    {
        convex = CreateBase();

        mesh.vertices = SetVertices();
        mesh.triangles = GenerateTriangles(convex.faces);
        mesh.normals = CalculateNormals(convex.faces, convex.vertices).ToArray();

        Debug.Log("normales :" + mesh.normals.Length);
        Debug.Log("faces :" + mesh.triangles.Length/2);
        Debug.Log("vertices :" + mesh.vertices.Length);
        UpdateConvexHull();

        mesh.vertices = SetVertices();
        mesh.triangles = GenerateTriangles(convex.faces);
        mesh.normals = CalculateNormals(convex.faces, convex.vertices).ToArray();
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
        var normalesLocal = CalculateNormals(convex.faces, convex.vertices).ToArray();

        for (int i = 4; i < points.Count; i++)
        {
            var p = points[i];

            List<ConvexHull.Face> visible = new List<ConvexHull.Face>();
            for (int j = 0; j < convex.faces.Count; j++)
            {

                var dirToPoint = p.position - convex.faces[j].a.pos;
                if (Vector3.Dot(dirToPoint, normalesLocal[j]) >= 0)
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
                        visibleVertices.Add(edge.v1);
                        visibleVertices.Add(edge.v2);
                    }
                    else if (!f1Visible && !f2Visible)
                    {
                        edge.isOneVisibleAndInvisible = false;
                    }
                    else
                    {
                        edge.isOneVisibleAndInvisible = true;
                        edge.v1.isOneVisibleAndInvisible = true;
                        edge.v2.isOneVisibleAndInvisible = true;
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

                //Cr�er les nouvelles faces et les nouveaux points, alias nouvelles victimes
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

                    if(Vector3.Dot(normal, up) > 0)
                    {
                        newFace.c = newFace.b;
                        newFace.b = newP;
                    }

                    newFaces.Add(newFace);
                    convex.faces.Add(newFace);
                    if (visible.Contains(edge.f1)) edge.f1 = newFace;
                    else edge.f2 = newFace;
                }

                convex.vertices.Add(newP);

                foreach(var edge in newEdges)
                {
                    foreach(var face in newFaces)
                    {
                        var otherVertex = edge.v1.Equals(newP)? edge.v2 : edge.v1;
                        bool checkAB = CheckEdge(face.a, face.b, edge.v1, edge.v2);
                        bool checkBC = CheckEdge(face.b, face.c, edge.v1, edge.v2);
                        bool checkCA = CheckEdge(face.a, face.c, edge.v1, edge.v2);

                        if(!checkAB && !checkBC && !checkAB)
                        {
                            continue;
                        }
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