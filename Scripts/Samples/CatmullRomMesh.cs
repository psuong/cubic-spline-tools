﻿using CommonStructures;
using UnityEngine;

namespace Curves {

    public class CatmullRomMesh : BaseMesh {

        public CatmullRom catmullRom;
        public float width = 1f;
        public int resolution = 1;
        public int segments = 10;

        private Tuple<Vector3, Vector3>[] vertices;

        private void Start() {
            GenerateMesh();
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.cyan;
            vertices = catmullRom.SampleCatmullRomSpline(segments, width);
            for (int i = 1; i < vertices.Length; i++) {
                var start = vertices[i - 1];
                var end = vertices[i];

                Gizmos.DrawLine(start.item1, end.item1);
                Gizmos.DrawLine(start.item2, end.item2);
            }
        }

        protected override void GenerateTriangles() {
            var ySize = (segments + 1) * catmullRom.points.Length;
            triangles = new int[ySize * resolution * 6];
            var index = 0;
            for (int ti = 0, vi = 0, y = 0; y < vertices.Length; y++, vi++) {
                for (int x = 0; x < resolution; x++, ti += 6, vi++) {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + resolution + 1;
                    triangles[ti + 5] = vi + resolution + 2;
                    index++;
                }
            }
        }

        public override void GenerateMesh() {
            vertices = catmullRom.SampleCatmullRomSpline(segments, width);

            meshGenerator = meshGenerator?? new MeshGenerator();
            meshFilter = GetComponent<MeshFilter>();

            var size = vertices.Length;
            var mVertices = new Vector3[size * (resolution + 1)];

            for (int y = 0, i = 0; y < size; y++) {
                var tuple = vertices[y];
                for (int x = 0; x <= resolution; x++, i++) {
                    var t = (float) x / (float) resolution;
                    var pt = Vector3.Lerp(tuple.item1, tuple.item2, t);
                    mVertices[i] = pt;
                }
            }

            GenerateTriangles();
            meshGenerator.AddVertices(mVertices);
            meshGenerator.AddTriangles(triangles);

            var mesh = meshGenerator.CreateMesh();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            mesh.name = name;
            meshFilter.sharedMesh = mesh;
        }
    }
}
