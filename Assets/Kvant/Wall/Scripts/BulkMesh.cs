//
// Bulk mesh handler
//
// Duplicate and combine given meshes into a single mesh. It duplicates the
// meshes until the number of vertices reaches to 64k or the number of copies
// reaches to 4k.
//
using UnityEngine;

namespace Kvant
{
    public partial class Wall
    {
        [System.Serializable]
        class BulkMesh
        {
            #region Properties

            Mesh _mesh;
            public Mesh mesh { get { return _mesh; } }

            #endregion

            #region Public Methods

            public BulkMesh(Mesh[] shapes, int columns)
            {
                CombineMeshes(shapes, columns);
            }

            public void Rebuild(Mesh[] shapes, int columns)
            {
                Release();
                CombineMeshes(shapes, columns);
            }

            public void Release()
            {
                if (_mesh) DestroyImmediate(_mesh);
            }

            #endregion

            #region Private Methods

            // Cache structure used to store the shape information.
            struct ShapeCacheData
            {
                Vector3[] vertices;
                Vector3[] normals;
                int[] indices;

                public ShapeCacheData(Mesh mesh)
                {
                    if (mesh)
                    {
                        vertices = mesh.vertices;
                        normals = mesh.normals;
                        indices = mesh.GetIndices(0);
                    }
                    else
                    {
                        // The source mesh is empty; replaces with a two-sided quad.
                        vertices = new Vector3[] {
                            new Vector3 (-1, +1, 0), new Vector3 (+1, +1, 0),
                            new Vector3 (-1, -1, 0), new Vector3 (+1, -1, 0),
                            new Vector3 (+1, +1, 0), new Vector3 (-1, +1, 0),
                            new Vector3 (+1, -1, 0), new Vector3 (-1, -1, 0)
                        };
                        normals = new Vector3[] {
                             Vector3.forward,  Vector3.forward,
                             Vector3.forward,  Vector3.forward,
                            -Vector3.forward, -Vector3.forward,
                            -Vector3.forward, -Vector3.forward,
                        };
                        indices = new int[] {0, 1, 2, 3, 2, 1, 4, 5, 6, 7, 6, 5};
                    }
                }

                public int VertexCount { get { return vertices.Length; } }
                public int IndexCount { get { return indices.Length; } }

                public void CopyVerticesTo(Vector3[] destination, int position)
                {
                    System.Array.Copy(vertices, 0, destination, position, vertices.Length);
                }

                public void CopyNormalsTo(Vector3[] destination, int position)
                {
                    System.Array.Copy(normals, 0, destination, position, normals.Length);
                }

                public void CopyIndicesTo(int[] destination, int position, int offset)
                {
                    for (var i = 0; i < indices.Length; i++)
                        destination[position + i] = offset + indices[i];
                }
            }

            // Mesh combiner functoin.
            void CombineMeshes(Mesh[] shapes, int copyCount)
            {
                ShapeCacheData[] cache;

                if (shapes == null || shapes.Length == 0)
                {
                    // The shape array is empty; use the default shape.
                    cache = new ShapeCacheData[1];
                    cache[0] = new ShapeCacheData(null);
                }
                else
                {
                    // Store the meshes into the shape cache.
                    cache = new ShapeCacheData[shapes.Length];
                    for (var i = 0; i < shapes.Length; i++)
                        cache[i] = new ShapeCacheData(shapes[i]);
                }

                // Count the number of vertices and indices in the shape cache.
                var vc_shapes = 0;
                var ic_shapes = 0;
                foreach (var s in cache) {
                    vc_shapes += s.VertexCount;
                    ic_shapes += s.IndexCount;
                }

                // If there is nothing, break.
                if (vc_shapes == 0) return;

                // Determine the number of vertices/indicies.
                var vc = 0;
                var ic = 0;
                for (var i = 0; i < copyCount; i++)
                {
                    var s = cache[i % cache.Length];
                    if (vc + s.VertexCount > 65535) break;
                    vc += s.VertexCount;
                    ic += s.IndexCount;
                }

                // Create vertex arrays.
                var va = new Vector3[vc];
                var na = new Vector3[vc];
                var ta = new Vector2[vc];
                var ia = new int[ic];

                for (int va_i = 0, ia_i = 0, e_i = 0; va_i < vc; e_i++)
                {
                    var s = cache[e_i % cache.Length];

                    s.CopyVerticesTo(va, va_i);
                    s.CopyNormalsTo(na, va_i);
                    s.CopyIndicesTo(ia, ia_i, va_i);

                    var uv = new Vector2((float)e_i / copyCount, 0);
                    for (var i = 0; i < s.VertexCount; i++) ta[va_i + i] = uv;

                    va_i += s.VertexCount;
                    ia_i += s.IndexCount;
                }

                // Create a mesh object.
                _mesh = new Mesh();

                _mesh.vertices = va;
                _mesh.normals = na;
                _mesh.uv = ta;

                _mesh.SetIndices(ia, MeshTopology.Triangles, 0);
                _mesh.Optimize();

                // This only for temporary use. Don't save.
                _mesh.hideFlags = HideFlags.DontSave;

                // Avoid being culled.
                _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100);
            }

            #endregion
        }
    }
}
