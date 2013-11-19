using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FirstPersonCameraWindows.Models
{
    /// <summary>
    /// Interface that all models must implement.
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        /// <value>The world matrix.</value>
        Matrix WorldMatrix { get; set; }

        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        /// <value>The projection matrix.</value>
        Matrix ProjectionMatrix { get; set; }

        /// <summary>
        /// Draws the model by the specified view matrix.
        /// </summary>
        /// <param name="viewMatrix">The view matrix.</param>
        void Draw(Matrix viewMatrix);

        /// <summary>
        /// Calculates if the given bounding spheres collides with this model.
        /// Corrects the velocity if the collition occurs and adds the edge cases if exist.
        /// </summary>
        /// <param name="boundingSpheres">The bounding spheres.</param>
        /// <param name="velocity">The current velocity.</param>
        /// <param name="edgeCases">The edge cases.</param>
        /// <returns>The ground normal.</returns>
        Vector3? CollidesWith(BoundingSphere[] boundingSpheres, ref Vector3 velocity, List<EdgeCase> edgeCases);
    }
}
