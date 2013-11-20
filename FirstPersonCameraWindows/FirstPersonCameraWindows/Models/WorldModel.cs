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
    /// The world model.
    /// </summary>
    public class WorldModel : BaseWorldModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldModel"/> class.
        /// </summary>
        /// <param name="content">The content manager.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        public WorldModel(ContentManager content, GraphicsDevice graphicsDevice)
            : base(content, graphicsDevice)
        {
        }

        #region BaseWorldModel members

        /// <summary>
        /// Initializes the world.
        /// </summary>
        protected override void initializeWorld()
        {
            Texture2D stonewall = Content.Load<Texture2D>("Stonewall");
            Texture2D brickwall = Content.Load<Texture2D>("Brickwall");
            Texture2D woodfine = Content.Load<Texture2D>("Woodfine");
            Texture2D stonefloor = Content.Load<Texture2D>("Stonefloor");
            Texture2D directx10 = Content.Load<Texture2D>("Directx10");

            // Basic walls
            addQuad(Vector3.Forward, 0, 50, 100, 600, 120, stonewall); // Back wall
            addQuad(Vector3.Backward, 0, 50, 110, 620, 120, stonewall);
            addQuad(Vector3.Up, 0, 110, 105, 620, 10, stonefloor);
            addQuad(Vector3.Right, -300, 50, -200, 600, 120, stonewall); // Left wall
            addQuad(Vector3.Left, -310, 50, -200, 620, 120, stonewall);
            addQuad(Vector3.Up, -305, 110, -200, 10, 600, stonefloor);
            addQuad(Vector3.Left, 300, 50, -200, 600, 120, stonewall); // Right wall
            addQuad(Vector3.Right, 310, 50, -200, 620, 120, stonewall);
            addQuad(Vector3.Up, 305, 110, -200, 10, 600, stonefloor);
            addQuad(Vector3.Backward, 0, 40, -499, 400, 100, brickwall); // Front wall
            addQuad(Vector3.Backward, 0, 95, -500, 600, 30, stonewall);
            addQuad(Vector3.Forward, 0, 50, -510, 620, 120, stonewall);
            addQuad(Vector3.Up, 0, 110, -505, 620, 10, stonefloor);

            addQuad(Vector3.Up, 0, 0, -200, 600, 600, woodfine); // 1. Floor
            addQuad(Vector3.Down, 0, -10, -200, 620, 620, woodfine);
            addQuad(Vector3.Down, 0, 90, -380, 360, 240, stonefloor); // Roof
            addQuad(Vector3.Backward, 0, 95, -260, 400, 10, stonewall); // Between roof and 2. floor
            addQuad(Vector3.Up, 0, 100, -380, 400, 240, stonefloor); // 2. Floor

            addQuad(Vector3.Left, -200, 50, -380, 240, 100, stonewall); // Left stairs walls
            addQuad(Vector3.Right, -180, 45, -380, 240, 90, brickwall);
            addQuad(Vector3.Backward, -190, 45, -260, 20, 90, stonewall);
            addQuad(Vector3.Right, 200, 50, -380, 240, 100, stonewall); // Right stairs walls
            addQuad(Vector3.Left, 180, 45, -380, 240, 90, brickwall);
            addQuad(Vector3.Backward, 190, 45, -260, 20, 90, stonewall);

            // Ramp
            Models.Add(new Quad(Device, stonefloor, new Vector3(-250, 45, -382), Vector3.Normalize(new Vector3(0f, 1.0f, angleToCot(70))), Vector3.Right, 260, 100));

            // Stairs
            for (int i = 1; i <= 12; i++)
            {
                addQuad(Vector3.Backward, 250, i * 8 - 4, -240 - i * 20, 100, 8, stonefloor); // Front wall
                addQuad(Vector3.Up, 250, i * 8, -250 - i * 20, 100, 20, stonefloor); // Floor
            }

            // The rotating box
            Models.Add(new BoxModel(Content, directx10, Matrix.CreateTranslation(0, 30, -320)));

            // Pistol
            Models.Add(new WeaponModel(Content, Matrix.CreateTranslation(0, 0, -250), "pistol"));
        }

        #endregion BaseWorldModel members

        // Adds a quad to the models.
        private void addQuad(Vector3 normal, float x, float y, float z, float width, float height, Texture2D texture)
        {
            Vector3 up = Vector3.Up;
            if (normal == Vector3.Up || normal == Vector3.Down)
            {
                up = Vector3.Backward;
            }

            Models.Add(new Quad(Device, texture, new Vector3(x, y, z), normal, up, width, height));
        }

        // Gets the cotangent of the given angle (in degrees).
        private float angleToCot(float deg)
        {
            return (float)(1.0 / Math.Tan(MathHelper.ToRadians(deg)));
        }
    }
}
