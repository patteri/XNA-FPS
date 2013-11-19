using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FirstPersonCameraWindows
{
    /// <summary>
    /// Implements a camera, the sight of the avatar.
    /// </summary>
    public class Camera
    {
        // Constants
        public const float initViewAngle = MathHelper.PiOver4;

        // View and projection matrices.
        private Matrix world;
        private Matrix view;
        private Matrix projection;

        // Camera position and helper vectors
        private Vector3 cameraPosition;
        private Vector3 cameraDirection;
        private Vector3 cameraUp;
        private Vector3 referenceCameraDirection;
        private Vector3 referenceCameraUp;

        // View angle and aspect ration.
        private float viewAngle = initViewAngle;
        private float aspectRatio;

        // Clipping planes.
        private float nearClip = 1.0f;
        private float farClip = 3000.0f;

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
        /// Gets the view matrix.
        /// </summary>
        /// <value>The view matrix.</value>
        public Matrix ViewMatrix
        {
            get { return view; }
        }

        /// <summary>
        /// Gets the projection matrix.
        /// </summary>
        /// <value>The projection matrix.</value>
        public Matrix ProjectionMatrix
        {
            get { return projection; }
        }

        /// <summary>
        /// Gets or sets the camera position.
        /// </summary>
        /// <value>The camera position.</value>
        public Vector3 Position
        {
            get { return cameraPosition; }
            set { cameraPosition = value; }
        }

        /// <summary>
        /// Gets the camera direction.
        /// </summary>
        /// <value>The camera direction.</value>
        public Vector3 Direction
        {
            get { return cameraDirection; }
        }

        /// <summary>
        /// Gets the up vector.
        /// </summary>
        /// <value>The up vector.</value>
        public Vector3 UpVector
        {
            get { return cameraUp; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        /// <param name="position">The starting position.</param>
        /// <param name="aspectRatio">The aspect ratio.</param>
        public Camera(Vector3 position, float aspectRatio)
        {
            this.cameraPosition = position;
            this.aspectRatio = aspectRatio;
            cameraDirection = Vector3.Forward;
            cameraUp = Vector3.Up;
            referenceCameraDirection = Vector3.Forward;
            referenceCameraUp = Vector3.Up;

            world = Matrix.Identity;
            projection = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio, nearClip, farClip);
        }

        /// <summary>
        /// Updates the camera by moving it the specified offset and rotating it by the specified angles.
        /// </summary>
        /// <param name="cameraOffset">The camera offset.</param>
        /// <param name="yaw">The yaw.</param>
        /// <param name="pitch">The pitch.</param>
        public void Update(Vector3 cameraOffset, float yaw, float pitch)
        {
            cameraPosition += cameraOffset;

            // Calculate new direction and up vector
            cameraDirection = Vector3.Transform(referenceCameraDirection, Matrix.CreateFromAxisAngle(referenceCameraUp, yaw));
            Matrix temp = Matrix.CreateFromAxisAngle(Vector3.Cross(referenceCameraUp, cameraDirection), pitch);
            cameraDirection = Vector3.Transform(cameraDirection, temp);
            cameraUp = Vector3.Transform(referenceCameraUp, temp);

            // Calculate new view matrix
            view = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraDirection, cameraUp);
        }

        /// <summary>
        /// Sets the view angle. Can be used for zooming effects.
        /// </summary>
        /// <param name="viewAngle">The view angle.</param>
        public void setViewAngle(float viewAngle)
        {
            projection = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio, nearClip, farClip);
        }
    }
}
