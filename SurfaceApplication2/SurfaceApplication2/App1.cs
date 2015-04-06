using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using System.Windows;



using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading;

namespace SurfaceApplication2
{
    /// <summary>
    /// This is the main type for your application.
    /// </summary>
    public class App1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D light;
        int elapse;
        bool flag = false;
        float delay = 200f;
        int frame = 0;
        string binaryResult;
        Rectangle sourceRect;
        int binaryResultLength = 0;
        int binaryposition = 0;
        int location=-1;
        char[] arr;
        char character = ' ';
        int count = 0;
        string myString = "bet,1234564563456756456754678";
        int st = 0;


        private TouchTarget touchTarget;
        private Color backgroundColor = new Color(0, 0, 0);
        private bool applicationLoadCompleteSignalled;

        private UserOrientation currentOrientation = UserOrientation.Bottom;
        private Matrix screenTransform = Matrix.Identity;

        /// <summary>
        /// The target receiving all surface input for the application.
        /// </summary>
        protected TouchTarget TouchTarget
        {
            get { return touchTarget; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        #region Initialization

        /// <summary>
        /// Moves and sizes the window to cover the input surface.
        /// </summary>
        private void SetWindowOnSurface()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero,
                "Window initialization must be complete before SetWindowOnSurface is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
                return;

            // Get the window sized right.
            Program.InitializeWindow(Window);
            // Set the graphics device buffers.
            graphics.PreferredBackBufferWidth = Program.WindowSize.Width;
            graphics.PreferredBackBufferHeight = Program.WindowSize.Height;
            graphics.ApplyChanges();
            // Make sure the window is in the right location.
            Program.PositionWindow();
        }

        /// <summary>
        /// Initializes the surface input system. This should be called after any window
        /// initialization is done, and should only be called once.
        /// </summary>
        private void InitializeSurfaceInput()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero,
                "Window initialization must be complete before InitializeSurfaceInput is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
                return;
            System.Diagnostics.Debug.Assert(touchTarget == null,
                "Surface input already initialized");
            if (touchTarget != null)
                return;

            touchTarget = new TouchTarget((IntPtr)0, EventThreadChoice.OnBackgroundThread); // I Want to capture all runing application events.    
            touchTarget.EnableInput();
            touchTarget.TouchDown += new EventHandler<TouchEventArgs>(touchTarget_TouchDown);


            // Create a target for surface input.
            touchTarget = new TouchTarget(Window.Handle, EventThreadChoice.OnBackgroundThread);
            touchTarget.EnableInput();
        }

        //recognises touch input
        void touchTarget_TouchDown(object sender, TouchEventArgs e)
        {
            string touchEvent = e.ToString();
            char[] delimeters = { '(', ')' };
            string[] position = touchEvent.Split(delimeters);
            string pos = position[1];
            string[] axis = pos.Split(',', ' ');
            int xAxis = int.Parse(axis[0]);
            int yAxis = int.Parse(axis[2]);
            string[] values = touchEvent.Split('=');
            //Console.WriteLine(values[4]);
            String[] BinaryArray = new String[8];
            string touchType = "blob";
            /*
            switch(values[4].Equals(touchType) && (xAxis>1250) && (xAxis<1900) && (yAxis>0) && (yAxis<500))
                    {
                        case 1  //((xAxis > 1550) && (xAxis < 1600) && (yAxis > 150) && (yAxis < 200))
                                :
                            
                            break;

                        case 2:
                            //...
                            break;

                        case 3:
                            //...
                            break;

                        case 4:
                           // ...
                            break;

                        default:
                            //...
                            break;


                    }
            */
            if (values[4].Equals(touchType) && (xAxis > 1250) && (xAxis < 1900) && (yAxis > 0) && (yAxis < 3000))
            {
                Console.WriteLine("X Axis:");
                Console.WriteLine(xAxis);
                Console.WriteLine("Y Axis:");
                Console.WriteLine(yAxis);
                /*
                if ((xAxis > 1550) && (xAxis < 1600) && (yAxis > 150) && (yAxis < 200))
                {
                    BinaryArray[1] = "1";
                    Console.WriteLine("Point A");
                }
                else{
                    BinaryArray[1] = "1";
                }
                if ((xAxis > 1600) && (xAxis < 1650) && (yAxis > 150) && (yAxis < 200))
                {
                    Console.WriteLine("Point B");
                }
                else if ((xAxis > 1650) && (xAxis < 1700) && (yAxis > 150) && (yAxis < 200))
                {
                    Console.WriteLine("Point C");
                }
                else if ((xAxis > 1700) && (xAxis < 1750) && (yAxis > 150) && (yAxis < 200))
                {
                    Console.WriteLine("Point D");
                }
                else if ((xAxis > 1750) && (xAxis < 1800) && (yAxis > 150) && (yAxis < 200))
                {
                    Console.WriteLine("Point E");
                }
                else if ((xAxis > 1800) && (xAxis < 1850) && (yAxis > 150) && (yAxis < 200))
                {
                    Console.WriteLine("Point F");
                }
                else if ((xAxis > 1850) && (xAxis < 1900) && (yAxis > 150) && (yAxis < 200))
                {
                    Console.WriteLine("Point G");
                }
                 * */


            }

        }

        #endregion

        #region Overridden Game Methods

        /// <summary>
        /// Allows the app to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            // TODO: Add your initialization logic here

            IsMouseVisible = true; // easier for debugging not to "lose" mouse
            SetWindowOnSurface();
            InitializeSurfaceInput();

            // Set the application's orientation based on the orientation at launch
            currentOrientation = ApplicationServices.InitialOrientation;

            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;

            // Setup the UI to transform if the UI is rotated.
            // Create a rotation matrix to orient the screen so it is viewed correctly
            // when the user orientation is 180 degress different.
            Matrix inverted = Matrix.CreateRotationZ(MathHelper.ToRadians(180)) *
                       Matrix.CreateTranslation(graphics.GraphicsDevice.Viewport.Width,
                                                 graphics.GraphicsDevice.Viewport.Height,
                                                 0);

            if (currentOrientation == UserOrientation.Top)
            {
                screenTransform = inverted;
            }

            //fullscreen

            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            //binary conversion
           
            arr = myString.ToCharArray();
            //char myText = (char)('~'+1);
            //byte[] arr = System.Text.Encoding.ASCII.GetBytes(myText);


            base.Initialize();

        }

        /// <summary>
        /// LoadContent will be called once per app and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            Vector2 spritePosition = Vector2.Zero;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            light = Content.Load<Texture2D>("flash2");

            // TODO: use this.Content to load your application content here
        }

        /// <summary>
        /// UnloadContent will be called once per app and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the app to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (ApplicationServices.WindowAvailability != WindowAvailability.Unavailable)
            {
                if (ApplicationServices.WindowAvailability == WindowAvailability.Interactive)
                {
                    // TODO: Process touches, 
                    // use the following code to get the state of all current touch points.
                    ReadOnlyTouchPointCollection touches = touchTarget.GetState();
                    //Console.WriteLine(touches.ToString());
                    //Console.WriteLine(touchTarget.ToString());
                }

                //  Add your logics for 1 and 0 to here


                elapse += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (elapse % delay == 0)
                {

                                        if (location < arr.Length)
{
                        location++;

                    }
                    st = 1;
                }
                else
                    st = 0;

                sourceRect = new Rectangle(0, 0, 100, 100);
                //destRect = new Rectangle(frame*100, frame*100, 100, 100);
            }
            base.Update(gameTime);

        }
        /// <summary>
        /// This is called when the app should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!applicationLoadCompleteSignalled)
            {
                // Dismiss the loading screen now that we are starting to draw
                ApplicationServices.SignalApplicationLoadComplete();
                applicationLoadCompleteSignalled = true;
            }

            //TODO: Rotate the UI based on the value of screenTransform here if desired

            GraphicsDevice.Clear(backgroundColor);

            if (location < arr.Length)
            {
                binaryResult = ConvertToBinary(arr[location]);
                binaryResultLength = binaryResult.Length;
         //       Console.WriteLine(binaryResult.ToString());
         //       Console.WriteLine(binaryResultLength.ToString());

                spriteBatch.Begin();

                for (int i = 0; i < binaryResultLength; i++)
                {
                    if (binaryResult[i] == '1')
                    {
                        if (st == 1)
                            spriteBatch.Draw(light, new Rectangle((1450 + (i * 15)), 210, 10, 10), sourceRect, Color.White);
                        else
                            spriteBatch.Draw(light, new Rectangle((1450 + (i * 15)), 210, 10, 10), sourceRect, Color.Black);
                    }
                    if (binaryposition <= binaryResultLength - 1)
                    {
                        binaryposition++;
                    }

                }
                spriteBatch.Draw(light, new Rectangle(1650, 210, 10, 10), sourceRect, Color.White);
                spriteBatch.End();

            }
                
       

            //TODO: Add your drawing code here
            //TODO: Avoid any expensive logic if application is neither active nor previewed
            
            base.Draw(gameTime);
        }



        public static string ConvertToBinary(char asciiString)
        {
            string result = string.Empty;

            result += Convert.ToString((int)asciiString, 2);

            return result;
        }

        #endregion

        #region Application Event Handlers

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: Enable audio, animations here

            //TODO: Optionally enable raw image here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: Optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: Disable audio, animations here

            //TODO: Disable raw image if it's enabled
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources.
                IDisposable graphicsDispose = graphics as IDisposable;
                if (graphicsDispose != null)
                {
                    graphicsDispose.Dispose();
                }
                if (touchTarget != null)
                {
                    touchTarget.Dispose();
                    touchTarget = null;
                }
            }

            // Release unmanaged Resources.

            // Set large objects to null to facilitate garbage collection.

            base.Dispose(disposing);
        }

        #endregion
    }
}
