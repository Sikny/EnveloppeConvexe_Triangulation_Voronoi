using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvexHull
{
    public List<Face> faces = new List<Face>();
    public List<Edge> edges = new List<Edge>();
    public List<Vertex> vertices = new List<Vertex>();

    public class Face
    {
        public Face(Vertex a, Vertex b, Vertex c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
        public Vertex a, b, c;
    }

    public class Edge
    {
        public Edge(Vertex v1, Vertex v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }
        public Vertex v1, v2;
        public Face f1, f2;
        public bool isOneVisibleAndInvisible;
    }

    public class Vertex
    {
        public Vertex(Vector3 pos, int index)
        {
            this.pos = pos;
            this.index = index;
        }
        public Vector3 pos;
        public int index;
        public bool isOneVisibleAndInvisible;
        public override string ToString()
        {
            return pos.ToString("f3");
        }
    }
    
}
