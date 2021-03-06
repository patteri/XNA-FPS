#region File Description
//-----------------------------------------------------------------------------
// Game1.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using FirstPersonCameraWindows.Models;

namespace FirstPersonCameraWindows
{
    /// <summary>
    /// The game motor.
    /// Handles the player controls and updates the camera and the world.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        // Constants
        private const float G = 9.81f;
        private const float RotationSpeed = MathHelper.Pi / 300f;
        private const float InitRunA = MathHelper.Pi * 6f;
        private const float InitRunDecelerateA = MathHelper.Pi * 4f;
        private const float InitRunVmax = MathHelper.Pi * 1.2f;
        private const float InitJumpV = MathHelper.Pi;
        private const float InitForwardSpeed = MathHelper.Pi / 1.5f;
        private const float EndOfTheWorld = -2000; // The y coordinate for reseting the avatar state
        private readonly Vector3 StartPosition = new Vector3(0, 80, 0);

        // The world model.
        private BaseWorldModel model;

        // The camera
        private Camera camera;
        
        // Avatar bounding sphere radius
        private const float avatarBsR = 8;

        // Avatar moving-related variables
        private Vector3 velocity;
        private Vector3? groundNormal;
        private float currentYaw;
        private float currentPitch;
        
        private float runA = InitRunA;
        private float runDecelerateA = InitRunDecelerateA;
        private float runVmax = InitRunVmax;
        private float jumpV = InitJumpV;
        private float forwardSpeed = InitForwardSpeed;
        private bool isInAir = false;
        private bool isCrouching = false;
        
        // Control states
        private KeyboardState prevKeyboardState;
        private MouseState prevMouseState;

        // The graphics manager
        private GraphicsDeviceManager graphics;

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;
            Content.RootDirectory = "Content";
            this.Window.Title = "First-person shooter";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            camera = new Camera(StartPosition, graphics.GraphicsDevice.Viewport.AspectRatio);

            Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
            prevMouseState = Mouse.GetState();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            model = new WorldModel(Content, GraphicsDevice);
            model.SetProjectionMatrix(camera.ProjectionMatrix);

            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
            
            UpdateCamera(gameTime);
            model.update(gameTime, camera);

            base.Update(gameTime);
        }

        /// <summary>
        /// Updates the position and direction of the camera.
        /// </summary>
        private void UpdateCamera(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            handleKeyboardAndMouse(keyboardState, mouseState);

            prevKeyboardState = keyboardState;
            prevMouseState = mouseState;

            // Moving direction
            float? moveAngle = getMoveDirection(keyboardState);

            // The elapsed game time.
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;   

            // Calculate the current velocity by the move direction.
            if (moveAngle != null)
            {
                Vector3 moveDirection = Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(moveAngle.Value) * Matrix.CreateRotationY(currentYaw)) * forwardSpeed;
                Vector3 newVelocity = velocity + moveDirection * runA * elapsed;
                if (newVelocity.Length() <= runVmax)
                {
                    velocity = newVelocity;
                }
            }

            // Jumping
            if (!isInAir && !isCrouching && keyboardState.IsKeyDown(Keys.Space))
            {
                velocity.Y += jumpV;
            }

            // Gravity
            velocity.Y -= G * elapsed;
            
            // Movement deceleration
            if (!isInAir)
            {
                float factor = 1f - runDecelerateA * elapsed;
                velocity *= new Vector3(factor, 1, factor);
                if (Math.Abs(velocity.X) < 0.01) velocity.X = 0;
                if (Math.Abs(velocity.Z) < 0.01) velocity.Z = 0;
            }

            // Camera offset
            Vector3 offset = camera.Position + velocity;
            
            // Construct bounding spheres
            int bsCount = isCrouching ? 2 : 3;
            BoundingSphere[] bss = new BoundingSphere[bsCount];
            bss[bsCount - 1] = new BoundingSphere(offset, avatarBsR);
            bss[bsCount - 2] = new BoundingSphere(offset + new Vector3(0, -2f * avatarBsR, 0), avatarBsR);
            if (!isCrouching) bss[bsCount - 3] = new BoundingSphere(offset + new Vector3(0, -4f * avatarBsR, 0), avatarBsR);

            // Collition tests
            groundNormal = model.CollitionTest(bss, ref velocity);
            
            // Update the camera
            camera.Update(velocity, currentYaw, currentPitch);

            isInAir = groundNormal == null;

            // Falling off the world?
            if (camera.Position.Y < EndOfTheWorld)
            {
                ResetAvatar();
            }
        }

        /// <summary>
        /// Handles the keyboard and mouse controls
        /// </summary>
        /// <param name="curKeyboardState">The current keyboard state.</param>
        /// <param name="curMouseState">The current mouse state.</param>
        private void handleKeyboardAndMouse(KeyboardState curKeyboardState, MouseState curMouseState)
        {
            // Mouse zoom
            if (curMouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton == ButtonState.Released)
            {
                camera.setViewAngle(MathHelper.Pi / 16);
                model.SetProjectionMatrix(camera.ProjectionMatrix);
            }
            else if (curMouseState.RightButton == ButtonState.Released && prevMouseState.RightButton == ButtonState.Pressed)
            {
                camera.setViewAngle(Camera.initViewAngle);
                model.SetProjectionMatrix(camera.ProjectionMatrix);
            }

            // Sneaking
            if (curKeyboardState.IsKeyDown(Keys.LeftShift) && !prevKeyboardState.IsKeyDown(Keys.LeftShift))
            {
                runA = MathHelper.Pi * 5f / 2f;
                runVmax = MathHelper.PiOver2;
            }
            else if (!curKeyboardState.IsKeyDown(Keys.LeftShift) && prevKeyboardState.IsKeyDown(Keys.LeftShift))
            {
                runA = InitRunA;
                runVmax = InitRunVmax;
            }

            // Crouching
            if (curKeyboardState.IsKeyDown(Keys.LeftControl) && !prevKeyboardState.IsKeyDown(Keys.LeftControl))
            {
                runA = MathHelper.Pi * 5f / 2f;
                runVmax = MathHelper.PiOver2;
                isCrouching = true;
                camera.Update(new Vector3(0, -2f * avatarBsR, 0), currentYaw, currentPitch);
            }
            else if (!curKeyboardState.IsKeyDown(Keys.LeftControl) && prevKeyboardState.IsKeyDown(Keys.LeftControl))
            {
                runA = InitRunA;
                runVmax = InitRunVmax;
                isCrouching = false;
                camera.Update(new Vector3(0, 2 * avatarBsR, 0), currentYaw, currentPitch);
            }

            // Looking direction
            currentYaw -= RotationSpeed * (curMouseState.X - prevMouseState.X);
            if (currentYaw < 0)
            {
                currentYaw += 2f * MathHelper.Pi;
            }
            else if (currentYaw > 2f * MathHelper.Pi)
            {
                currentYaw -= 2f * MathHelper.Pi;
            }

            currentPitch += RotationSpeed * (curMouseState.Y - prevMouseState.Y);
            if (currentPitch < -0.45f * MathHelper.Pi)
            {
                currentPitch = -0.45f * MathHelper.Pi;
            }
            else if (currentPitch > 0.45f * MathHelper.Pi)
            {
                currentPitch = 0.45f * MathHelper.Pi;
            }
        }

        /// <summary>
        /// Gets the current move direction by the pressed buttons.
        /// </summary>
        /// <param name="keyboardState">State of the keyboard.</param>
        /// <returns>The move direction. Null, if no direction.</returns>
        private float? getMoveDirection(KeyboardState keyboardState)
        {
            float? moveAngle = null;

            if (!isInAir)
            {
                if (keyboardState.IsKeyDown(Keys.W)) moveAngle = MathHelper.Pi;
                if (keyboardState.IsKeyDown(Keys.A)) moveAngle = 1.5f * MathHelper.Pi;
                if (keyboardState.IsKeyDown(Keys.S)) moveAngle = 0;
                if (keyboardState.IsKeyDown(Keys.D)) moveAngle = MathHelper.PiOver2;
                if (keyboardState.IsKeyDown(Keys.W) && keyboardState.IsKeyDown(Keys.A)) moveAngle = 1.25f * MathHelper.Pi;
                if (keyboardState.IsKeyDown(Keys.W) && keyboardState.IsKeyDown(Keys.D)) moveAngle = 0.75f * MathHelper.Pi;
                if (keyboardState.IsKeyDown(Keys.A) && keyboardState.IsKeyDown(Keys.S)) moveAngle = 1.75f * MathHelper.Pi;
                if (keyboardState.IsKeyDown(Keys.S) && keyboardState.IsKeyDown(Keys.D)) moveAngle = MathHelper.PiOver4;
            }

            return moveAngle;
        }

        /// <summary>
        /// Resets the avatar state to the initial position.
        /// </summary>
        private void ResetAvatar()
        {
            velocity = Vector3.Zero;
            currentPitch = 0;
            currentYaw = 0;
            camera.Position = StartPosition;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.SteelBlue);

            model.Draw(camera.ViewMatrix);

            base.Draw(gameTime);
        }
    }
}
