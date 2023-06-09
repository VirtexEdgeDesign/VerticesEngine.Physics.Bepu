﻿using System.Collections.Generic;
using BEPUphysics.BroadPhaseEntries;
using BEPUutilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.CollisionShapes;

using System;



namespace BEPUphysicsDrawer.Models
{
    /// <summary>
    /// Simple display object for triangles.
    /// </summary>
    public class DisplayTerrain : CustomMesh<Terrain>
    {
        /// <summary>
        /// Creates the display object for the entity.
        /// </summary>
        /// <param name="drawer">Drawer managing this display object.</param>
        /// <param name="displayedObject">Entity to draw.</param>
        public DisplayTerrain(ModelDrawer drawer, Terrain displayedObject)
            : base(drawer, displayedObject)
        {

        }

        public override int GetTriangleCountEstimate()
        {
            return DisplayedObject.Shape.Heights.Length * 2;
        }

        public override void GetMeshData(List<VertexPositionNormalTexture> vertices, List<ushort> indices)
        {
            int numColumns = DisplayedObject.Shape.Heights.GetLength(0);
            int numRows = DisplayedObject.Shape.Heights.GetLength(1);
            TerrainShape shape = DisplayedObject.Shape;

            //The terrain can be transformed arbitrarily.  However, the collision against the triangles is always oriented such that the transformed local
            //up vector points in the same direction as the collidable surfaces.
            //To make sure the graphics match the terrain collision, try transforming the local space up direction into world space. Treat it as a normal- it requires an adjugate transpose, not a regular transformation.

            var normalTransform = Matrix3x3.AdjugateTranspose(DisplayedObject.WorldTransform.LinearTransform);

            var reverseWinding = Vector3.Dot(normalTransform.Up, DisplayedObject.WorldTransform.LinearTransform.Up) < 0;


            for (int j = 0; j < numRows; j++)
            {
                for (int i = 0; i < numColumns; i++)
                {
                    VertexPositionNormalTexture v;
                    Vector3 position, n;
                    DisplayedObject.GetPosition(i, j, out position);
                    shape.GetLocalNormal(i, j, out n);
                    Matrix3x3.Transform(ref n, ref normalTransform, out n);
                    n.Normalize();
					v.Position = position;
					v.Normal = n;

                    if (reverseWinding)
                        Vector3.Negate(ref v.Normal, out v.Normal);
                    v.TextureCoordinate = new Vector2(i, j);

                    vertices.Add(v);

                    if (i < numColumns - 1 && j < numRows - 1)
                        if (shape.QuadTriangleOrganization == QuadTriangleOrganization.BottomLeftUpperRight)
                        {
                            //v3 v4
                            //v1 v2

                            //v1 v2 v3
                            indices.Add((ushort)(numColumns * j + i));
                            if (reverseWinding)
                            {
                                indices.Add((ushort)(numColumns * (j + 1) + i));
                                indices.Add((ushort)(numColumns * j + i + 1));
                            }
                            else
                            {
                                indices.Add((ushort)(numColumns * j + i + 1));
                                indices.Add((ushort)(numColumns * (j + 1) + i));
                            }

                            //v2 v4 v3
                            indices.Add((ushort)(numColumns * j + i + 1));
                            if (reverseWinding)
                            {
                                indices.Add((ushort)(numColumns * (j + 1) + i));
                                indices.Add((ushort)(numColumns * (j + 1) + i + 1));
                            }
                            else
                            {
                                indices.Add((ushort)(numColumns * (j + 1) + i + 1));
                                indices.Add((ushort)(numColumns * (j + 1) + i));
                            }
                        }
                        else if (shape.QuadTriangleOrganization == QuadTriangleOrganization.BottomRightUpperLeft)
                        {
                            //v1 v2 v4
                            indices.Add((ushort)(numColumns * j + i));
                            if (reverseWinding)
                            {
                                indices.Add((ushort)(numColumns * (j + 1) + i + 1));
                                indices.Add((ushort)(numColumns * j + i + 1));
                            }
                            else
                            {
                                indices.Add((ushort)(numColumns * j + i + 1));
                                indices.Add((ushort)(numColumns * (j + 1) + i + 1));
                            }

                            //v1 v4 v3
                            indices.Add((ushort)(numColumns * j + i));
                            if (reverseWinding)
                            {
                                indices.Add((ushort)(numColumns * (j + 1) + i));
                                indices.Add((ushort)(numColumns * (j + 1) + i + 1));
                            }
                            else
                            {
                                indices.Add((ushort)(numColumns * (j + 1) + i + 1));
                                indices.Add((ushort)(numColumns * (j + 1) + i));
                            }
                        }
                }

            }



        }

        public override void Update()
        {
            WorldTransform = Matrix.Identity; //The terrain mesh was created in world space to begin with.
        }
    }
}