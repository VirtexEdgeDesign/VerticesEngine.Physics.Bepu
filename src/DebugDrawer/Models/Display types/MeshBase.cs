using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BEPUphysicsDrawer.Models
{
	public abstract class MeshBase
	{
		/// <summary>
		/// Gets the drawer that this display object belongs to.
		/// </summary>
		public ModelDrawer Drawer { get; set; }

		/// <summary>
		/// Gets the world transformation applied to the model.
		/// </summary>
		public Matrix WorldTransform { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public virtual void Activate()
		{ }

		/// <summary>
		/// Updates the display object and reports the world transform.
		/// </summary>
		public abstract void Update();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="textures"></param>
		/// <param name="device"></param>
		/// <param name="effect"></param>
		/// <param name="pass"></param>
		public virtual void Draw(Matrix viewMatrix, Matrix projectionMatrix)
		{
		}
	}
}
