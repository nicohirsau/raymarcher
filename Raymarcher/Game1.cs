using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Raymarcher
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        readonly float ninetyDegree = MathHelper.ToRadians(90);
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D shaderTexture;
        Effect shader;
        Vector2 sphereposition;
        Vector2 sphereposition2;
        float totalElapsedTime;
        float u_time_multiplier;
        float max_steps = 20;
        Vector3 parameters = new Vector3(10, 10, .1f);
        Vector2 rotation = new Vector2(0, 0);

        Vector3 position_offset = new Vector3(280, 280, -500);
        Vector4[] spheres;
        Vector4[] colors;
        Point lastMousePosition;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Mouse.SetPosition(1, 1);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            //graphics.ToggleFullScreen();
            shaderTexture = new Texture2D(graphics.GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);
            this.IsMouseVisible = false;
            
            //graphics.ToggleFullScreen();
            graphics.ApplyChanges();
            shader = Content.Load<Effect>("shader");
            sphereposition = new Vector2(400, 225);
            sphereposition2 = new Vector2(400, 225);
            totalElapsedTime = 0;
            u_time_multiplier = 15f;
            spheres = new Vector4[5]
            {
                new Vector4(400, 255, 200, 100),
                new Vector4(500, 255, 50, 30),
                new Vector4(250, 180, 100, 50),
                new Vector4(100, 325, 100, 45),
                new Vector4(-10000, -8000, 10000, 5000)
            };
            colors = new Vector4[5]
            {
                new Vector4(1, 0, 0, 20),
                new Vector4(0, 1, 0, 15),
                new Vector4(0, 0, 1, 10),
                new Vector4(1, 1, 1, 5),
                new Vector4(0.5f, 0.5f, 0.5f, 1000)
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

            // * 150 * (float)gameTime.ElapsedGameTime.TotalSeconds) - position_offset;

            // deltaPosition * 150 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState ks = Keyboard.GetState();
            MouseState ms = Mouse.GetState();

            if (ms.Position.X >= Window.ClientBounds.Width - 1)
            {
                Mouse.SetPosition(1, ms.Position.Y);
                lastMousePosition = new Point(1, ms.Position.Y);
            }
            if (ms.Position.X <= 0)
            {
                Mouse.SetPosition(Window.ClientBounds.Width - 2, ms.Position.Y);
                lastMousePosition = new Point(Window.ClientBounds.Width - 2, ms.Position.Y);
            }
            if (ms.Position.Y >= Window.ClientBounds.Height - 1)
            {
                Mouse.SetPosition(ms.Position.X, 1);
                lastMousePosition = new Point(ms.Position.X, 1);
            }
            if (ms.Position.Y <= 0)
            {
                Mouse.SetPosition(ms.Position.X, Window.ClientBounds.Height - 2);
                lastMousePosition = new Point(ms.Position.X, Window.ClientBounds.Height - 2);
            }
            ms = Mouse.GetState();

            if (ms.Position != lastMousePosition)
            {
                Point deltaMousePosition = (ms.Position - lastMousePosition);
                
                rotation += new Vector2(
                    deltaMousePosition.Y,
                    -deltaMousePosition.X    
                ) * .1f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                lastMousePosition = ms.Position;
            }

            if (ks.IsKeyDown(Keys.C))
                rotation = Vector2.Zero;

            if (ks.IsKeyDown(Keys.O))
                max_steps += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (ks.IsKeyDown(Keys.L))
                max_steps -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (ks.IsKeyDown(Keys.Right))
                rotation.Y -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (ks.IsKeyDown(Keys.Left))
                rotation.Y += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (ks.IsKeyDown(Keys.Up))
                rotation.X -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (ks.IsKeyDown(Keys.Down))
                rotation.X += (float)gameTime.ElapsedGameTime.TotalSeconds;

            rotation.X = MathHelper.Clamp(rotation.X, -ninetyDegree, ninetyDegree);
            
            Vector3 deltaPosition = Vector3.Zero;
            if (ks.IsKeyDown(Keys.A))
                deltaPosition.X = -1;
            if (ks.IsKeyDown(Keys.D))
                deltaPosition.X = 1;
            if (ks.IsKeyDown(Keys.W))
                deltaPosition.Z = 1;
            if (ks.IsKeyDown(Keys.S))
                deltaPosition.Z = -1;
            if (ks.IsKeyDown(Keys.R))
                deltaPosition.Y = -1;
            if (ks.IsKeyDown(Keys.F))
                deltaPosition.Y = 1;
            
            deltaPosition = Vector3.Transform(deltaPosition, Matrix.CreateRotationX(-rotation.X));
            deltaPosition = Vector3.Transform(deltaPosition, Matrix.CreateRotationY(-rotation.Y));

            position_offset += deltaPosition * 150 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (ks.IsKeyDown(Keys.LeftShift))
                position_offset += deltaPosition * 500 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            System.Diagnostics.Debug.WriteLine(ms.Position);

            Window.Title = Window.ClientBounds.ToString();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            shader.Parameters["resolution"].SetValue(new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height));
            //shader.Parameters["resolution_x"].SetValue(800f);
            //shader.Parameters["resolution_y"].SetValue(800f);
            shader.Parameters["max_steps"].SetValue(max_steps);
            shader.Parameters["position_offset"].SetValue(position_offset);
            //shader.Parameters["parameters"].SetValue(parameters);
            //shader.Parameters["u_elapsedTime"].SetValue(totalElapsedTime);
            shader.Parameters["rotation"].SetValue(rotation);
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
