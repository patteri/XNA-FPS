using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FirstPersonCameraWindows.Models
{
    /// <summary>
    /// Base implementation for world models.
    /// </summary>
    public abstract class BaseWorldModel
    {
        // The world consists of distinct models.
        private List<IModel> models;

        private Matrix projection;

        private ContentManager content;
        private GraphicsDevice device;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorldModel"/> class.
        /// </summary>
        /// <param name="content">The content manager.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        public BaseWorldModel(ContentManager content, GraphicsDevice graphicsDevice)
        {
            Content = content;
            Device = graphicsDevice;

            models = new List<IModel>();

            initializeWorld();
        }

        /// <summary>
        /// Gets or sets the models.
        /// </summary>
        /// <value>The models.</value>
        protected List<IModel> Models
        {
            get { return models; }
            set { models = value; }
        }

        /// <summary>
        /// Gets the content manager.
        /// </summary>
        /// <value>The content manager.</value>
        protected ContentManager Content
        {
            get { return content; }
            private set { content = value; }
        }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        protected GraphicsDevice Device
        {
            get { return device; }
            private set { device = value; }
        }

        #region Abstract methods

        /// <summary>
        /// Initializes the world.
        /// </summary>
        protected abstract void initializeWorld();

        #endregion

        /// <summary>
        /// Updates the world.
        /// </summary>
        /// <param name="gameTime">The time of the game.</param>
        /// <param name="camera">The camera.</param>
        public void update(GameTime gameTime, Camera camera)
        {
            // Update the dynamic models
            foreach (var item in Models.OfType<DynamicModel>())
            {
                item.update(gameTime, camera);
            }
        }

        /// <summary>
        /// Sets a projection matrix to the world.
        /// </summary>
        /// <param name="projectionMatrix">The projection matrix.</param>
        public void SetProjectionMatrix(Matrix projectionMatrix)
        {
            projection = projectionMatrix;
            foreach (var model in models)
            {
                model.ProjectionMatrix = projectionMatrix;
            }
        }

        /// <summary>
        /// Draws the world using the specified view matrix.
        /// </summary>
        /// <param name="viewMatrix">The view matrix.</param>
        public void Draw(Matrix viewMatrix)
        {
            foreach (var model in models)
            {
                model.Draw(viewMatrix);
            }
        }

        /// <summary>
        /// Executes the collition tests and corrects the velocity if going through a plane.
        /// </summary>
        /// <param name="boundingSpheres">The avatar bounding spheres.</param>
        /// <param name="velocity">The current velocity.</param>
        /// <returns>The current normal of the ground.</returns>
        public Vector3? CollitionTest(BoundingSphere[] boundingSpheres, ref Vector3 velocity)
        {
            Vector3? groundNormal = null;
            List<EdgeCase> edgeCases = new List<EdgeCase>();
            foreach (var model in models)
            {
                Vector3? ground = model.CollidesWith(boundingSpheres, ref velocity, edgeCases);
                if (ground != null)
                {
                    groundNormal = ground;
                }
            }

            if (edgeCases.Count > 1)
            {
                EdgeCase edgeCase = edgeCases.Where(d => d.distance == edgeCases.Min(it => it.distance)).First();
                velocity += edgeCase.correction;
            }
            
            return groundNormal;
        }
    }
}
