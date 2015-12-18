using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TurboPort
{
    internal static class MeshUtil
    {
        public static Vector3[] FindCollisionPoints(ModelMeshCollection mesh, BoundingBox boundingBox)
        {
            Vector3 inset = (boundingBox.Max - boundingBox.Min)*0.01f;
            boundingBox.Min += inset;
            boundingBox.Max -= inset;

            Vector3[] collisionPoints = new Vector3[6];
            int[] numberAdded = new int[6];

            foreach (ModelMesh modelMesh in mesh)
            {
                foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
                {
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;

                    Vector3[] vertices = new Vector3[meshPart.NumVertices];
                    meshPart.VertexBuffer.GetData(0, vertices, 0, meshPart.NumVertices, vertexStride);

                    foreach (Vector3 vertex in vertices)
                    {
                        if (vertex.X < boundingBox.Min.X)
                        {
                            collisionPoints[0] += vertex;
                            numberAdded[0]++;
                        }
                        else if (vertex.X > boundingBox.Max.X)
                        {
                            collisionPoints[1] += vertex;
                            numberAdded[1]++;
                        }
                        if (vertex.Y < boundingBox.Min.Y)
                        {
                            collisionPoints[2] += vertex;
                            numberAdded[2]++;
                        }
                        else if (vertex.Y > boundingBox.Max.Y)
                        {
                            collisionPoints[3] += vertex;
                            numberAdded[3]++;
                        }
                        if (vertex.Z < boundingBox.Min.Z)
                        {
                            collisionPoints[4] += vertex;
                            numberAdded[4]++;
                        }
                        else if (vertex.Z > boundingBox.Max.Z)
                        {
                            collisionPoints[5] += vertex;
                            numberAdded[5]++;
                        }
                    }
                }
            }
            for (int i = 0; i < collisionPoints.Length; i++)
            {
                collisionPoints[i] *= 1f/numberAdded[i];
            }
            collisionPoints[0].X = boundingBox.Min.X;
            collisionPoints[1].X = boundingBox.Max.X;
            collisionPoints[2].Y = boundingBox.Min.Y;
            collisionPoints[3].Y = boundingBox.Max.Y;
            collisionPoints[4].Z = boundingBox.Min.Z;
            collisionPoints[5].Z = boundingBox.Max.Z;

            return collisionPoints;
        }

        public static void GetBoundingFromMeshes(IList<ModelMesh> meshes, float scale,
            out BoundingSphere boundingSphere)
        {
            boundingSphere = meshes[0].BoundingSphere;
            for (int i = 1; i < meshes.Count; i++)
                boundingSphere = BoundingSphere.CreateMerged(boundingSphere, meshes[i].BoundingSphere);


            boundingSphere = boundingSphere.Transform(Matrix.CreateScale(scale));
        }

        public static BoundingBox CreateBoundingBox(IList<ModelMesh> meshes)
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
            Vector3 max = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);

            // For each mesh of the model
            foreach (ModelMesh mesh in meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;

                    Vector3[] vertexData = new Vector3[meshPart.NumVertices];
                    meshPart.VertexBuffer.GetData(0, vertexData, 0, meshPart.NumVertices, vertexStride);

                    for (int i = 0; i < meshPart.NumVertices; i++)
                    {
                        Vector3 transformedPosition = vertexData[i];

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }
            return new BoundingBox(min, max);
        }
    }
}