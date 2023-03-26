using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using Microsoft.Xna.Framework;


namespace BEPUphysicsDrawer.Models
{
    /// <summary>
    /// Helper class that can create shape mesh data.
    /// </summary>
    public static class DisplayCompoundBody
    {

        public static void GetShapeMeshData(EntityCollidable collidable, List<VertexPositionNormalTexture> vertices, List<ushort> indices)
        {
            var compoundCollidable = collidable as CompoundCollidable;
            if (compoundCollidable == null)
                throw new ArgumentException("Wrong shape type.");
            var tempIndices = new List<ushort>();
            var tempVertices = new List<VertexPositionNormalTexture>();
            for (int i = 0; i < compoundCollidable.Children.Count; i++)
            {
                var child = compoundCollidable.Children[i];
                ModelDrawer.ShapeMeshGetter shapeMeshGetter;
                if (ModelDrawer.ShapeMeshGetters.TryGetValue(child.CollisionInformation.GetType(), out shapeMeshGetter))
                {
                    shapeMeshGetter(child.CollisionInformation, tempVertices, tempIndices);

                    for (int j = 0; j < tempIndices.Count; j++)
                    {
                        indices.Add((ushort)(tempIndices[j] + vertices.Count));
                    }
                    var localTransform = child.Entry.LocalTransform;
                    var localPosition = child.CollisionInformation.LocalPosition;
                    var orientation = localTransform.Orientation;
                    var position = localTransform.Position;
                    for (int j = 0; j < tempVertices.Count; j++)
                    {
                        VertexPositionNormalTexture vertex = tempVertices[j];
                        Vector3.Add(ref vertex.Position, ref localPosition, out vertex.Position);
                        Vector3.Transform(ref vertex.Position, ref orientation, out vertex.Position);
                        Vector3.Add(ref vertex.Position, ref position, out vertex.Position);
                        Vector3.Transform(ref vertex.Normal, ref orientation, out vertex.Normal);
                        vertices.Add(vertex);
                    }

                    tempVertices.Clear();
                    tempIndices.Clear();
                }
            }
        }
    }
}