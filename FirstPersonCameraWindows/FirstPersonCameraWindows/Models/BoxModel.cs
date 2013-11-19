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
    /// The model of a rotating box
    /// </summary>
    public class BoxModel : DynamicModel
    {
        private Texture2D texture;

        private float angleX = 0;
        private float angleY = 0;
        private float angleZ = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxModel"/> class.
        /// </summary>
        /// <param name="content">The content manager.</param>
        /// <param name="texture">The texture.</param>
        /// <param name="location">The location of the box.</param>
        public BoxModel(ContentManager content, Texture2D texture, Matrix location)
            : base(content, location, "box")
        {
            this.texture = texture;
        }

        /// <summary>
        /// Updates the model.
        /// </summary>
        /// <param name="gameTime">The time of the game.</param>
        /// <param name="camera">The camera.</param>
        public override void update(GameTime gameTime, Camera camera)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Let's rotate the box!
            angleX += elapsed / 2f * MathHelper.Pi;
            angleY += elapsed / 3f * MathHelper.Pi;

            WorldMatrix = Matrix.CreateRotationY(angleY) * Matrix.CreateRotationX(angleX) * Matrix.CreateRotationZ(angleZ) * Location;
        }

        /// <summary>
        /// Draws the model by the specified view matrix.
        /// </summary>
        /// <param name="viewMatrix">The view matrix.</param>
        public override void Draw(Matrix viewMatrix)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.Projection = ProjectionMatrix;
                    be.View = viewMatrix;
                    be.World = WorldMatrix;
                    be.Texture = texture;
                    be.TextureEnabled = true;
                }
                mesh.Draw();
            }
        }
    }
}
