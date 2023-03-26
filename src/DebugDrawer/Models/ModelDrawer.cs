using System;
using System.Collections.Generic;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.DataStructures;
using BEPUphysics.Entities;
using BEPUphysics.UpdateableSystems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace BEPUphysicsDrawer.Models
{
    /// <summary>
    /// Manages and draws models.
    /// </summary>
    public class ModelDrawer
    {
        private readonly Dictionary<object, CustomMesh> displayObjects = new Dictionary<object, CustomMesh>();
        private readonly RasterizerState fillState;

		private readonly List<MeshBase> selfDrawingDisplayObjects = new List<MeshBase>();
        private readonly RasterizerState wireframeState;

        private static readonly Dictionary<Type, Type> displayTypes = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, ShapeMeshGetter> shapeMeshGetters = new Dictionary<Type, ShapeMeshGetter>();



        /// <summary>
        /// Gets the map from shape object types to methods which can be used to construct the data.
        /// </summary>
        public static Dictionary<Type, ShapeMeshGetter> ShapeMeshGetters
        {
            get { return shapeMeshGetters; }
        }

        static ModelDrawer()
        {
            //Display types are sometimes requested from contexts lacking a convenient reference to a ModelDrawer instance.
            //Having them static simplifies things.
            displayTypes.Add(typeof(FluidVolume), typeof(DisplayFluid));
            displayTypes.Add(typeof(Terrain), typeof(DisplayTerrain));
            displayTypes.Add(typeof(TriangleMesh), typeof(DisplayTriangleMesh));
            displayTypes.Add(typeof(StaticMesh), typeof(DisplayStaticMesh));
            displayTypes.Add(typeof(InstancedMesh), typeof(DisplayInstancedMesh));


            //Entity types are handled through a special case that uses an Entity's Shape to look up one of the ShapeMeshGetters.
            shapeMeshGetters.Add(typeof(ConvexCollidable<BoxShape>), DisplayBox.GetShapeMeshData);
            shapeMeshGetters.Add(typeof(ConvexCollidable<SphereShape>), DisplaySphere.GetShapeMeshData);
            shapeMeshGetters.Add(typeof(ConvexCollidable<CapsuleShape>), DisplayCapsule.GetShapeMeshData);
            shapeMeshGetters.Add(typeof(ConvexCollidable<CylinderShape>), DisplayCylinder.GetShapeMeshData);
            shapeMeshGetters.Add(typeof(ConvexCollidable<ConeShape>), DisplayCone.GetShapeMeshData);
            shapeMeshGetters.Add(typeof(ConvexCollidable<TriangleShape>), DisplayTriangle.GetShapeMeshData);
            shapeMeshGetters.Add(typeof(ConvexCollidable<ConvexHullShape>), DisplayConvexHull.GetShapeMeshData);
            shapeMeshGetters.Add(typeof(ConvexCollidable<MinkowskiSumShape>), DisplayConvex.GetShapeMeshData);
            shapeMeshGetters.Add(typeof(ConvexCollidable<WrappedShape>), DisplayConvex.GetShapeMeshData);
            shapeMeshGetters.Add(typeof(ConvexCollidable<TransformableShape>), DisplayConvex.GetShapeMeshData);
            shapeMeshGetters.Add(typeof(CompoundCollidable), DisplayCompoundBody.GetShapeMeshData);
            shapeMeshGetters.Add(typeof(MobileMeshCollidable), DisplayMobileMesh.GetShapeMeshData);

        }

		public ModelDrawer(Game game)
        {
            Game = game;
		
            fillState = new RasterizerState();
            wireframeState = new RasterizerState();
            wireframeState.FillMode = FillMode.WireFrame;

        }


        /// <summary>
        /// Gets the game using this ModelDrawer.
        /// </summary>
        public static Game Game { get; private set; }

        /// <summary>
        /// Gets or sets whether or not the model drawer is drawing wireframes.
        /// </summary>
        public bool IsWireframe { get; set; }

        /// <summary>
        /// Constructs a new display object for an object.
        /// </summary>
        /// <param name="objectToDisplay">Object to create a display object for.</param>
		/// <returns>Display object for an object.</returns>
		public CustomMesh CreateDisplayObject(object objectToDisplay)
		{
			if (!displayObjects.ContainsKey(objectToDisplay)) 
			{
				Type displayType;

				if (displayTypes.TryGetValue(objectToDisplay.GetType(), out displayType)) {
					/*
#if !WINDOWS
                    return (ModelDisplayObject)displayType.GetConstructor(
                                                     new Type[] { typeof(ModelDrawer), objectToDisplay.GetType() })
                                                     .Invoke(new object[] { this, objectToDisplay });
#else*/
					return (CustomMesh)Activator.CreateInstance(displayType, new[] { this, objectToDisplay });
//#endif
				}
				Entity e;
				if ((e = objectToDisplay as Entity) != null) {
					return new DisplayEntityCollidable(this, e.CollisionInformation);
				}
				EntityCollidable entityCollidable;
				if ((entityCollidable = objectToDisplay as EntityCollidable) != null) {
					return new DisplayEntityCollidable(this, entityCollidable);
				}
			}

			return null;
		}


        /// <summary>
        /// Attempts to add an object to the ModelDrawer.
        /// </summary>
        /// <param name="objectToDisplay">Object to be added to the model drawer.</param>
        /// <returns>ModelDisplayObject created for the object.  Null if it couldn't be added.</returns>
        public CustomMesh Add(object objectToDisplay)
        {
            CustomMesh displayObject = CreateDisplayObject(objectToDisplay);
            if (displayObject != null)
            {
				//if (!displayObjects.ContainsKey(objectToDisplay)) {
					Add(displayObject);
					displayObjects.Add(objectToDisplay, displayObject);
				//}
                return displayObject;
            }
            return null; //Couldn't add it.
        }

        /// <summary>
        /// Adds the display object to the drawer.
        /// </summary>
        /// <param name="displayObject">Display object to add.</param>
        /// <returns>Whether or not the display object was added.</returns>
		public bool Add(MeshBase displayObject)
        {
            if (!selfDrawingDisplayObjects.Contains(displayObject))
            {
				displayObject.Activate();
                selfDrawingDisplayObjects.Add(displayObject);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Removes an object from the drawer.
        /// </summary>
        /// <param name="objectToRemove">Object to remove.</param>
        /// <returns>Whether or not the object was present.</returns>
        public bool Remove(object objectToRemove)
        {
            CustomMesh displayObject;
            if (displayObjects.TryGetValue(objectToRemove, out displayObject))
            {
                Remove(displayObject);
                displayObjects.Remove(objectToRemove);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes an object from the drawer.
        /// </summary>
        /// <param name="displayObject">Display object to remove.</param>
        /// <returns>Whether or not the object was present.</returns>
		public bool Remove(MeshBase displayObject)
        {
            return selfDrawingDisplayObjects.Remove(displayObject);
        }


        /// <summary>
        /// Cleans out the model drawer of any existing display objects.
        /// </summary>
        public void Clear()
        {
            displayObjects.Clear();
            selfDrawingDisplayObjects.Clear();
        }


        /// <summary>
        /// Updates the drawer and its components.
        /// </summary>
        public void Update()
        {
			foreach (MeshBase displayObject in selfDrawingDisplayObjects)
                displayObject.Update();
        }


        /// <summary>
        /// Draws the drawer's models.
        /// </summary>
        /// <param name="viewMatrix">View matrix to use to draw the objects.</param>
        /// <param name="projectionMatrix">Projection matrix to use to draw the objects.</param>
        public void Draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            Game.GraphicsDevice.RasterizerState = IsWireframe ? wireframeState : fillState;

            Game.GraphicsDevice.BlendState = BlendState.Opaque;
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			foreach (MeshBase displayObject in selfDrawingDisplayObjects)
                displayObject.Draw(viewMatrix, projectionMatrix);
        }


        public delegate void ShapeMeshGetter(EntityCollidable collidable, List<VertexPositionNormalTexture> vertices, List<ushort> indices);
    }
}