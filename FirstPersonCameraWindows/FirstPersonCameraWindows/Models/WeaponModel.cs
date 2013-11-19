using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace FirstPersonCameraWindows.Models
{
    /// <summary>
    /// The avatar weapon
    /// </summary>
    public class WeaponModel : DynamicModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WeaponModel"/> class.
        /// </summary>
        /// <param name="content">The content manager.</param>
        /// <param name="location">The current location.</param>
        /// <param name="modelName">Name of the model.</param>
        public WeaponModel(ContentManager content, Matrix location, String modelName)
            : base(content, location, modelName)
        {
        }

        /// <summary>
        /// Updates the model.
        /// </summary>
        /// <param name="gameTime">The time of the game.</param>
        /// <param name="camera">The camera.</param>
        public override void update(GameTime gameTime, Camera camera)
        {
            Vector3 cameraRight = Vector3.Cross(camera.Direction, camera.UpVector);
            Vector3 weaponPosition = camera.Position + camera.Direction * 10 + cameraRight * 4 - camera.UpVector * 4;
            WorldMatrix = Matrix.CreateScale(0.08f) * Matrix.CreateWorld(weaponPosition, cameraRight, camera.UpVector);
        }

        /// <summary>
        /// Calculates if the given bounding spheres collides with this model.
        /// Corrects the velocity if the collition occurs and adds the edge cases if exist.
        /// </summary>
        /// <param name="boundingSpheres">The bounding spheres.</param>
        /// <param name="velocity">The current velocity.</param>
        /// <param name="edgeCases">The edge cases.</param>
        /// <returns>The ground normal.</returns>
        public override Vector3? CollidesWith(BoundingSphere[] boundingSpheres, ref Vector3 velocity, List<EdgeCase> edgeCases)
        {
            // No collition detections for the weapon.
            return null;
        }
    }
}
