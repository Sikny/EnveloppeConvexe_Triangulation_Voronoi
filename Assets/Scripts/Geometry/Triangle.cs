using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Geometry {
    [Serializable]
    public class Triangle {
        public Edge e1, e2, e3;

        public int[] Points {
            get {
                var s1 = e1.s1;
                var s2 = s1 == e2.s1 ? e2.s2 : e2.s1;
                var s3 = s2 == e3.s1 || s1 == e3.s1 ? (s1 == e3.s2 || s2 == e3.s2 ? e1.s2 : e3.s2) : e3.s1;
                return new[] { s1, s2, s3 };
            }
        }

        public Triangle(int a, int b, int c) {
            e1 = new Edge(a, b);
            e2 = new Edge(b, c);
            e3 = new Edge(c, a);
        }

        public Triangle(Edge[] edges) {
            Set(edges);
        }

        public void Set(Edge[] edges) {
            e1 = edges[0];
            e2 = edges[1];
            e3 = edges[2];
        }

        public bool Contains(Edge e) {
            return e1.Equals(e) || e2.Equals(e) || e3.Equals(e);
        }

        public static List<Triangle> ListFromIndices(int[] trianglesIndices) {
            var triangles = new List<Triangle>();
            int triCount = trianglesIndices.Length / 3;

            for (int i = 0; i < triCount; ++i) {
                int tri = i * 3;
                var edges = Edge.ListFromIndices(trianglesIndices.SubArray(tri, 3));
                triangles.Add(new Triangle(edges.ToArray()));
            }

            return triangles;
        }
    }

    [Serializable]
    public class Edge {
        public int s1, s2;

        public Edge(int a, int b) {
            Set(a, b);
        }

        public override string ToString() {
            return "(" + s1 + ", " + s2 + ")";
        }

        public override bool Equals(object e2) {
            var otherEdge = e2 as Edge;
            if (otherEdge == null) return false;
            bool result = Equals(otherEdge.s1, otherEdge.s2);
            return result;
        }

        public bool Equals(int a, int b) {
            return s1 == a && s2 == b || s2 == a && s1 == b;
        }

        public void Set(int a, int b) {
            s1 = a;
            s2 = b;
        }

        public static List<Edge> ListFromIndices(int[] indices) {
            var edges = new List<Edge>();
            int triCount = indices.Length / 3;

            for (int i = 0; i < triCount; ++i) {
                int tri = i * 3;
                var e1 = new Edge(indices[tri], indices[tri + 1]);
                var e2 = new Edge(indices[tri + 1], indices[tri + 2]);
                var e3 = new Edge(indices[tri + 2], indices[tri]);
                
                if(!edges.Contains(e1)) edges.Add(e1);
                if(!edges.Contains(e2)) edges.Add(e2);
                if(!edges.Contains(e3)) edges.Add(e3);
            }

            return edges;
        }
    }
}