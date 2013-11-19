using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace FirstPersonCameraWindows.Models
{
    /// <summary>
    /// Base implementation for dynamic models.
    /// </summary>
    public abstract class DynamicModel : IModel
    {
        private Model model;
        private Matrix world;
        private Matrix projection;
        private Matrix location;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicModel"/> class.
        /// </summary>
        /// <param name="content">The content manager.</param>
        /// <param name="location">The current location.</param>
        /// <param name="modelName">Name of the model.</param>
        public DynamicModel(ContentManager content, Matrix location, String modelName)
        {
            this.model = content.Load<Model>(modelName);
            this.location = location;
        }

        /// <summary>
        /// Gets the XNA-model associated to this model.
        /// </summary>
        /// <value>The XNA-model.</value>
        protected Model Model
        {
            get { return model; }
        }

        /// <summary>
        /// Gets or sets the location of the model.
        /// </summary>
        /// <value>The location of the model.</value>
        protected Matrix Location
        {
            get { return location; }
            set { location = value; }
        }

        #region Abstract methods

        /// <summary>
        /// Updates the model.
        /// </summary>
        /// <param name="gameTime">The time of the game.</param>
        /// <param name="camera">The camera.</param>
        public abstract void update(GameTime gameTime, Camera camera);

        #endregion

        #region IModel members

        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        /// <value>The world matrix.</value>
        public Matrix WorldMatrix
        {
            get { return world; }
            set { world = value; }
        }

        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        /// <value>The projection matrix.</value>
        public Matrix ProjectionMatrix
        {
            get { return projection; }
            set { projection = value; }
        }

        /// <summary>
        /// Draws the model by the specified view matrix.
        /// </summary>
        /// <param name="viewMatrix">The view matrix.</param>
        public virtual void Draw(Matrix viewMatrix)
        {
            model.Draw(world, viewMatrix, projection);
        }

        /// <summary>
        /// Calculates if the given bounding spheres collides with this model.
        /// Corrects the velocity if the collition occurs and adds the edge cases if exist.
        /// </summary>
        /// <param name="boundingSpheres">The bounding spheres.</param>
        /// <param name="velocity">The current velocity.</param>
        /// <param name="edgeCases">The edge cases.</param>
        /// <returns>The ground normal.</returns>
        public virtual Vector3? CollidesWith(BoundingSphere[] boundingSpheres, ref Vector3 velocity, List<EdgeCase> edgeCases)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere myBs = mesh. BoundingSphere.Transform(world);
                foreach (var bs in boundingSpheres)
                {
                    if (myBs.Intersects(bs))
                    {
                        velocity = Vector3.Zero;
                        return null;
                    }
                }
            }
            return null;
        }

        #endregion IModel members
    }
}
