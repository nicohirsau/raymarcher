﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Raymarcher
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D shaderTexture;
        Effect shader;
        Vector2 sphereposition;
        Vector2 sphereposition2;
        float totalElapsedTime;
        float u_time_multiplier;
        float max_steps = 20;
        Vector2 rotation = new Vector2(0, 1);

        Vector3 position_offset = new Vector3(280, 280, -500);
        Vector4[] spheres;
        Vector4[] colors;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            shaderTexture = new Texture2D(graphics.GraphicsDevice, 800, 450);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 800;
            //graphics.ToggleFullScreen();
            graphics.ApplyChanges();
            shader = Content.Load<Effect>("shader");
            sphereposition = new Vector2(400, 225);
            sphereposition2 = new Vector2(400, 225);
            totalElapsedTime = 0;
            u_time_multiplier = 15f;
            spheres = new Vector4[4]
            {
                new Vector4(400, 255, 200, 100),
                new Vector4(500, 255, 50, 30),
                new Vector4(300, 180, 100, 50),
                new Vector4(100, 325, 100, 45)
            };
            colors = new Vector4[4]
            {
                new Vector4(1, 0, 0, 1),
                new Vector4(0, 1, 0, 1),
                new Vector4(0, 0, 1, 1),
                new Vector4(1, 1, 1, 1)
            };
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            totalElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState ks = Keyboard.GetState();
            Vector3 deltaPosition = Vector3.Zero;
            if (ks.IsKeyDown(Keys.A))
                deltaPosition.X = -1;
            if (ks.IsKeyDown(Keys.D))
                deltaPosition.X = 1;
            if (ks.IsKeyDown(Keys.W))
                deltaPosition.Y = -1;
            if (ks.IsKeyDown(Keys.S))
                deltaPosition.Y = 1;
            if (ks.IsKeyDown(Keys.R))
                deltaPosition.Z = 1;
            if (ks.IsKeyDown(Keys.F))
                deltaPosition.Z = -1;
            position_offset += deltaPosition * (float)gameTime.ElapsedGameTime.TotalSeconds * 75;

            if (ks.IsKeyDown(Keys.O))
                max_steps += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (ks.IsKeyDown(Keys.L))
                max_steps -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            System.Diagnostics.Debug.WriteLine(position_offset);

            Window.Title = Window.ClientBounds.ToString();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            //shader.Parameters["resolution_x"].SetValue(800f);
            //shader.Parameters["resolution_y"].SetValue(800f);
            shader.Parameters["max_steps"].SetValue(max_steps);
            shader.Parameters["position_offset"].SetValue(position_offset);
            for (int i = 0; i < spheres.Length; i++)
            {
                shader.Parameters["spheres"].Elements[i].SetValue(spheres[i]);
                shader.Parameters["colors"].Elements[i].SetValue(colors[i]);
            }
            shader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(shaderTexture, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
