﻿
using BEPUphysics.Entities;
using BEPUutilities;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BEPUphysicsDrawer.Models
{
    /// <summary>
    /// Display object of a model that follows an entity.
    /// </summary>
	public class Model3D : MeshBase
    {
        private Model myModel;

        private Texture2D myTexture;

        /// <summary>
        /// Bone transformations of meshes in the model.
        /// </summary>
        private Microsoft.Xna.Framework.Matrix[] transforms;


		/// <summary>
		/// Constructs a new display model.
		/// </summary>
		/// <param name="model">Model to draw on the entity.</param>
		/// <param name="modelDrawer">Model drawer to use.</param>
		public Model3D(Model model, ModelDrawer modelDrawer)
		{
			Drawer = modelDrawer;
			Model = model;
			LocalTransform = Matrix.Identity;
		}

        /// <summary>
        /// Constructs a new display model.
        /// </summary>
        /// <param name="entity">Entity to follow.</param>
        /// <param name="model">Model to draw on the entity.</param>
        /// <param name="modelDrawer">Model drawer to use.</param>
        public Model3D(Entity entity, Model model, ModelDrawer modelDrawer)
        {
			Drawer = modelDrawer;
            LocalTransform = Matrix.Identity;
            Entity = entity;
            Model = model;
        }

        /// <summary>
        /// Gets or sets the entity to base the model's world matrix on.
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// Gets or sets the model to display.
        /// </summary>
        public Model Model
        {
            get { return myModel; }
            set
            {
                myModel = value;
                transforms = new Microsoft.Xna.Framework.Matrix[myModel.Bones.Count];
                for (int i = 0; i < Model.Meshes.Count; i++)
                {
                    for (int j = 0; j < Model.Meshes[i].Effects.Count; j++)
                    {
                        var effect = Model.Meshes[i].Effects[j] as BasicEffect;
                        if (effect != null)
                            effect.EnableDefaultLighting();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the texture drawn on this model.
        /// </summary>
        public Texture2D Texture
        {
            get { return myTexture; }
            set
            {
                myTexture = value;
                for (int i = 0; i < Model.Meshes.Count; i++)
                {
                    for (int j = 0; j < Model.Meshes[i].Effects.Count; j++)
                    {
                        var effect = Model.Meshes[i].Effects[j] as BasicEffect;
                        if (effect != null)
                        {
                            if (value != null)
                            {
                                effect.TextureEnabled = true;
                                effect.Texture = Texture;
                            }
                            else
                                effect.TextureEnabled = false;
                        }
                    }
                }
            }
        }

		public Vector3 Color { get; set; }

        /// <summary>
        /// Gets and sets the local transform to apply to the model prior to transforming it by the entity's world matrix.
        /// </summary>
        public Matrix LocalTransform { get; set; }



        /// <summary>
        /// Updates the display object.
        /// </summary>
        public override void Update()
        {
			if (Entity != null)
				WorldTransform = LocalTransform * Entity.WorldTransform;
        }

        /// <summary>
        /// Draws the display object.
        /// </summary>
        /// <param name="viewMatrix">Current view matrix.</param>
        /// <param name="projectionMatrix">Current projection matrix.</param>
        public override void Draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            //This is not a particularly fast method of drawing.
            //It's used very rarely in the demos.
            myModel.CopyAbsoluteBoneTransformsTo(transforms);
            for (int i = 0; i < Model.Meshes.Count; i++)
            {
                for (int j = 0; j < Model.Meshes[i].Effects.Count; j++)
                {
                    var effect = Model.Meshes[i].Effects[j] as BasicEffect;
                    if (effect != null)
                    {
                        effect.World = transforms[Model.Meshes[i].ParentBone.Index] * WorldTransform;
                        effect.View = viewMatrix;
                        effect.Projection = projectionMatrix;
						effect.AmbientLightColor = Color;

						if (Texture != null) {
							effect.TextureEnabled = true;
							effect.Texture = Texture;
						} else
							effect.TextureEnabled = false;
                    }
                }
                Model.Meshes[i].Draw();
            }
        }
    }
}