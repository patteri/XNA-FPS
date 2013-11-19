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
    /// A textured quad model.
    /// </summary>
    public class Quad : IModel
    {
        // Location and direction vectors
        private Vector3 Origin;
        private Vector3 UpperLeft;
        private Vector3 LowerLeft;
        private Vector3 UpperRight;
        private Vector3 LowerRight;
        private Vector3 Normal;
        private Vector3 Up;
        private Vector3 Left;

        // Vertex data
        private VertexPositionNormalTexture[] Vertices;
        private short[] Indexes;

        // Edge case data
        private BoundingBox boundingBox;
        private Plane plane;
        private Plane[] edges;

        private GraphicsDevice graphicsDevice;

        private BasicEffect quadEffect;
        private Texture2D texture;

        private Matrix world;
        private Matrix projection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Quad"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="texture">The texture.</param>
        /// <param name="origin">The quad origin.</param>
        /// <param name="normal">The plane normal.</param>
        /// <param name="up">The up vector.</param>
        /// <param name="width">The width of the quad.</param>
        /// <param name="height">The height of the quad.</param>
        public Quad(GraphicsDevice graphicsDevice, Texture2D texture, Vector3 origin, Vector3 normal, Vector3 up, float width, float height)
        {
            this.graphicsDevice = graphicsDevice;
            this.texture = texture;

            quadEffect = new BasicEffect(graphicsDevice);
            quadEffect.World = Matrix.Identity;
            quadEffect.TextureEnabled = true;
            quadEffect.Texture = texture;

            Vertices = new VertexPositionNormalTexture[4];
            Indexes = new short[6];
            Origin = origin;
            normal.Normalize();
            Normal = normal;
            up.Normalize();
            Up = up;

            // Calculate the quad corners
            Left = Vector3.Cross(normal, Up);
            Vector3 uppercenter = (Up * height / 2) + origin;
            UpperLeft = uppercenter + (Left * width / 2);
            UpperRight = uppercenter - (Left * width / 2);
            LowerLeft = UpperLeft - (Up * height);
            LowerRight = UpperRight - (Up * height);

            // Calculate the bounding box
            float minX = MathHelper.Min(MathHelper.Min(MathHelper.Min(UpperLeft.X, UpperRight.X), LowerLeft.X), LowerRight.X);
            float minY = MathHelper.Min(MathHelper.Min(MathHelper.Min(UpperLeft.Y, UpperRight.Y), LowerLeft.Y), LowerRight.Y);
            float minZ = MathHelper.Min(MathHelper.Min(MathHelper.Min(UpperLeft.Z, UpperRight.Z), LowerLeft.Z), LowerRight.Z);
            float maxX = MathHelper.Max(MathHelper.Max(MathHelper.Max(UpperLeft.X, UpperRight.X), LowerLeft.X), LowerRight.X);
            float maxY = MathHelper.Max(MathHelper.Max(MathHelper.Max(UpperLeft.Y, UpperRight.Y), LowerLeft.Y), LowerRight.Y);
            float maxZ = MathHelper.Max(MathHelper.Max(MathHelper.Max(UpperLeft.Z, UpperRight.Z), LowerLeft.Z), LowerRight.Z);

            boundingBox = new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));

            // Calculate the helper planes for edge cases.
            plane = new Plane(LowerRight, UpperRight, UpperLeft);
            edges = new Plane[4]
            {
                new Plane(UpperLeft, UpperRight, UpperRight + Normal),
                new Plane(UpperRight, LowerRight, LowerRight + Normal),
                new Plane(LowerRight, LowerLeft, LowerLeft + Normal),
                new Plane(LowerLeft, UpperLeft, UpperLeft + Normal)
            };

            FillVertices(width / (float)texture.Width * 10f, height / (float)texture.Height * 10f);
        }

        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        /// <value>The world matrix.</value>
        public Matrix WorldMatrix
        {
            get { return world; }
            set
            {
                world = value;
                quadEffect.World = value;
            }
        }

        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        /// <value>The projection matrix.</value>
        public Matrix ProjectionMatrix
        {
            get { return projection; }
            set
            {
                projection = value;
                quadEffect.Projection = value;
            }
        }

        /// <summary>
        /// Draws the model by the specified view matrix.
        /// </summary>
        /// <param name="viewMatrix">The view matrix.</param>
        public void Draw(Matrix viewMatrix)
        {
            quadEffect.View = viewMatrix;
            foreach (EffectPass pass in quadEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                    PrimitiveType.TriangleList, Vertices, 0, 4, Indexes, 0, 2);
            }
        }

        /// <summary>
        /// Calculates if the given bounding spheres collides with this model.
        /// Corrects the velocity if the collition occurs and adds the edge cases if exist.
        /// </summary>
        /// <param name="boundingSpheres">The bounding spheres.</param>
        /// <param name="velocity">The current velocity.</param>
        /// <param name="edgeCases">The edge cases.</param>
        /// <returns>The ground normal.</returns>
        public Vector3? CollidesWith(BoundingSphere[] boundingSpheres, ref Vector3 velocity, List<EdgeCase> edgeCases)
        {
            Vector3? groundNormal = null;
            for (int i = boundingSpheres.Length - 1; i >= 0; i--)
            {
                BoundingSphere bs = boundingSpheres[i];

                // Basic check: bounding sphere intersecting the bounding box?
                if (boundingBox.Intersects(bs))
                {
                    // Calculate distance to the plane
                    Vector3 bsCenter = bs.Center;
                    float trueDistance = Vector3.Dot(plane.Normal, bsCenter) + plane.D;
                    float distance = trueDistance - bs.Radius;

                    if (distance < 0)
                    {
                        //if (i != 0 || Normal == Vector3.Up) // This makes the stairs work
                        //{

                        bool edgeCase = false;
                        Vector3 normal = Normal;

                        if (new Ray(bsCenter, -Normal).Intersects(boundingBox) == null)
                        {
                            // The "edge case"
                            foreach (var edgePlane in edges)
                            {
                                if (edgePlane.Intersects(bs) == PlaneIntersectionType.Intersecting)
                                {
                                    edgeCase = true;

                                    // Calculate distance to the edge
                                    float edgeDistance = Vector3.Dot(edgePlane.Normal, bsCenter) + edgePlane.D;

                                    float newRadius = (float)Math.Sqrt(Math.Pow(bs.Radius, 2) - Math.Pow(edgeDistance, 2));
                                    distance = trueDistance - newRadius;

                                    break;
                                }
                            }
                        }
                        else if (Normal.Y > 0.5 && Normal.Y < 1)
                        {
                            // The "sloping plane case"
                            Ray r = new Ray(bsCenter, Vector3.Down);
                            float? newDistance = r.Intersects(plane);
                            distance = newDistance.Value - bs.Radius;
                            normal = Vector3.Up;
                        }

                        distance = Math.Abs(distance);

                        Vector3 correction = normal * distance;
                        if (!edgeCase)
                        {
                            velocity += correction;

                            for (int j = 0; j < boundingSpheres.Length; j++)
                            {
                                boundingSpheres[j].Center += correction;
                            }

                            if (i == 0 && Normal.Y > 0.5)
                            {
                                groundNormal = Normal;
                            }
                        }
                        else
                        {
                            edgeCases.Add(new EdgeCase(distance, correction));
                        }

                        break;
                        //}
                    }
                }
            }
            return groundNormal;
        }

        private void FillVertices(float textureWidthRatio, float textureHeightRatio)
        {
            // Fill in texture coordinates to display full texture on quad
            Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureUpperRight = new Vector2(textureWidthRatio, 0.0f);
            Vector2 textureLowerLeft = new Vector2(0.0f, textureHeightRatio);
            Vector2 textureLowerRight = new Vector2(textureWidthRatio, textureHeightRatio);

            // Provide a normal for each vertex
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal = Normal;
            }

            // Set the position and texture coordinate for each vertex
            Vertices[0].Position = LowerLeft;
            Vertices[0].TextureCoordinate = textureLowerLeft;
            Vertices[1].Position = UpperLeft;
            Vertices[1].TextureCoordinate = textureUpperLeft;
            Vertices[2].Position = LowerRight;
            Vertices[2].TextureCoordinate = textureLowerRight;
            Vertices[3].Position = UpperRight;
            Vertices[3].TextureCoordinate = textureUpperRight;

            // Set the index buffer for each vertex, using
            // clockwise winding
            Indexes[0] = 0;
            Indexes[1] = 1;
            Indexes[2] = 2;
            Indexes[3] = 2;
            Indexes[4] = 1;
            Indexes[5] = 3;
        }
    }
}
