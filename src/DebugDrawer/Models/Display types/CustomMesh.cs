
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BEPUphysicsDrawer.Models
{
	/// <summary>
	/// Model-based graphical representation of an object.
	/// </summary>
	/// <typeparam name="T">Type of the object to be displayed.</typeparam>
	public abstract class CustomMesh<T> : CustomMesh
	{
		protected CustomMesh(ModelDrawer drawer, T displayedObject)
			: base(drawer)
		{
			DisplayedObject = displayedObject;
		}

		/// <summary>
		/// Gets the object to represent with a model.
		/// </summary>
		public T DisplayedObject { get; private set; }
	}

    /// <summary>
    /// Base class of ModelDisplayObjects.
    /// </summary>
	public abstract class CustomMesh : MeshBase
    {
		protected static Texture2D[] textures = new Texture2D[8];
		private static BasicEffect effect;

		internal IndexBuffer indexBuffer;

		internal ushort[] indices;

		internal VertexBuffer vertexBuffer;
		internal VertexPositionNormalTexture[] vertices;

        protected static Random Random = new Random();

		static CustomMesh()
		{
			var game = ModelDrawer.Game;

			textures[0] = new Texture2D(game.GraphicsDevice, 1, 1);
			textures[0].SetData(new[] { new Color(255, 216, 0) });

			textures[1] = new Texture2D(game.GraphicsDevice, 1, 1);
			textures[1].SetData(new[] { new Color(79, 200, 255) });

			textures[2] = new Texture2D(game.GraphicsDevice, 1, 1);
			textures[2].SetData(new[] { new Color(255, 0, 0) });

			textures[3] = new Texture2D(game.GraphicsDevice, 1, 1);
			textures[3].SetData(new[] { new Color(177, 0, 254) });

			textures[4] = new Texture2D(game.GraphicsDevice, 1, 1);
			textures[4].SetData(new[] { new Color(255, 130, 151) });

			textures[5] = new Texture2D(game.GraphicsDevice, 1, 1);
			textures[5].SetData(new[] { new Color(254, 106, 0) });

			textures[6] = new Texture2D(game.GraphicsDevice, 1, 1);
			textures[6].SetData(new[] { new Color(168, 165, 255) });

			textures[7] = new Texture2D(game.GraphicsDevice, 1, 1);
			textures[7].SetData(new[] { new Color(0, 254, 33) });


			effect = new BasicEffect(game.GraphicsDevice);
			effect.PreferPerPixelLighting = false;
			effect.LightingEnabled = true;
			effect.DirectionalLight0.Enabled = true;
			effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1, -1, -1));
			effect.DirectionalLight0.DiffuseColor = new Vector3(.66f, .66f, .66f);
			effect.AmbientLightColor = new Vector3(.5f, .5f, .5f);

			effect.TextureEnabled = true;
		}

		public CustomMesh(ModelDrawer drawer)
        {
            Drawer = drawer;
            TextureIndex = Random.Next(8);
        }

		public override void Activate()
		{
			var tempVertices = new List<VertexPositionNormalTexture>();
			var tempIndices = new List<ushort>();
			GetMeshData(tempVertices, tempIndices);
			vertices = new VertexPositionNormalTexture[tempVertices.Count];
			indices = new ushort[tempIndices.Count];
			tempVertices.CopyTo(vertices);
			tempIndices.CopyTo(indices);

			vertexBuffer = new VertexBuffer(ModelDrawer.Game.GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
			indexBuffer = new IndexBuffer(ModelDrawer.Game.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);

			vertexBuffer.SetData(vertices);
			indexBuffer.SetData(indices);
		}


        /// <summary>
        /// Gets or sets the texture index used for this display object.
        /// </summary>
        public int TextureIndex { get; set; }


		/// <summary>
		/// Collects the local space vertex data of the model.
		/// </summary>
		/// <param name="vertices">List of vertices to be filled with the model vertices.</param>
		/// <param name="indices">List of indices to be filled with the model indices.</param>
		/// <param name="batch">Batch that the display object is being added to.</param>
		/// <param name="baseVertexBufferIndex">Index in the batch's vertex buffer where this display object's vertices start.</param>
		/// <param name="baseIndexBufferIndex">Index in the batch's index buffer where this display object's vertices start.</param>
		/// <param name="batchListIndex">Index in the batch's display object list where this display object will be inserted.</param>
		public void GetVertexData(List<VertexPositionNormalTexture> vertices, List<ushort> indices,
								  ushort baseVertexBufferIndex, int baseIndexBufferIndex, int batchListIndex)
		{
			GetMeshData(vertices, indices);
			//Modify the indices.
			for (int i = 0; i < indices.Count; i++) {
				indices[i] += baseVertexBufferIndex;
			}
		}

        /// <summary>
        /// Gets the approximate number of triangles that will be used by the display object if added.
        /// </summary>
        /// <returns>Approximate number of triangles that the display object will use if added.</returns>
        public abstract int GetTriangleCountEstimate();

        /// <summary>
        /// Collects the local space vertex data of the model.
        /// </summary>
        /// <param name="vertices">List of vertices to be filled with the model vertices.</param>
        /// <param name="indices">List of indices to be filled with the model indices.</param>
        public abstract void GetMeshData(List<VertexPositionNormalTexture> vertices, List<ushort> indices);


		public override void Draw(Matrix viewMatrix, Matrix projectionMatrix)
		{
			var device = ModelDrawer.Game.GraphicsDevice;

			effect.View = viewMatrix;
			effect.Projection = projectionMatrix;

			for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++) {
				var pass = effect.CurrentTechnique.Passes[i];
				effect.World = WorldTransform;
				effect.Texture = textures[TextureIndex];
				device.SetVertexBuffer(vertexBuffer);
				device.Indices = indexBuffer;
				pass.Apply();
				device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);
			}
		}
    }
}