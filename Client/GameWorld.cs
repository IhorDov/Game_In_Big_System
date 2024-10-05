using Client.GameObjects;
using MessagePack;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Client
{
    public class GameWorld : Game
    {
        //private GraphicsDeviceManager _graphics;
        //private SpriteBatch _spriteBatch;
        List<GameObject> gamebjects = new List<GameObject>();
        public static bool USE_INTERPOLATION = true;
        public static bool USE_RECONCILITION = true;
        public static bool USE_PREDICTION = true;
        public static int LATENCY = 250;
        //private SpriteFont font;
        Player player = null!;
        TCPGameClient client = null!;

        public GameWorld()
        {
            //_graphics = new GraphicsDeviceManager(this);
            //Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }


        protected override void Initialize()
        {
            //force FPS, makes it easier to create functional network code, otherwise time is a factor...
            this.IsFixedTimeStep = true;//false;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d); //60);
            base.Initialize();
            client = new TCPGameClient(onDataRecieved);
            client.SendDataToServer(new JoinMessage());
            StartGame();
        }

        private void onDataRecieved(byte[] receivedData)
        {
            MessageType messageType = (MessageType)receivedData[0];
            byte[] dataToDeserialize = receivedData.Skip(1).ToArray();
            switch (messageType)
            {
                case MessageType.SnapShot:
                    SnapShot snap = MessagePackSerializer.Deserialize<SnapShot>(dataToDeserialize);
                    player.HandleSnapShot(snap);
                    break;
                case MessageType.JoinAnswer:
                    player.Owner = MessagePackSerializer.Deserialize<JoinAnswer>(dataToDeserialize).PlayerOwner;
                    break;
                case MessageType.UpdateTickRate:
                    player.SetTickRate(MessagePackSerializer.Deserialize<UpdateTickRate>(dataToDeserialize).TickRate);
                    break;
                default:
                    break;
            }
        }

        void StartGame()
        {
            player = new Player("player", Content, new Vector2(GraphicsDevice.Viewport.Bounds.Width / 2, GraphicsDevice.Viewport.Bounds.Height / 2), client);
            gamebjects.Add(player);
            gamebjects.ForEach(x => x.Init());
        }


        protected override void LoadContent()
        {
            //_spriteBatch = new SpriteBatch(GraphicsDevice);
            //font = Content.Load<SpriteFont>("font");
            //gamebjects.ForEach(x => x.LoadContent());
            // TODO: use this.Content to load your game content here
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            GetState();
            if (HasBeenPressed(Keys.Add))
            {
                Debug.WriteLine("hello");
                LATENCY += 50;
            }
            if (HasBeenPressed(Keys.OemMinus))
            {
                LATENCY -= 50;
            }
            if (HasBeenPressed(Keys.P))
            {
                Debug.WriteLine("hello");
                USE_PREDICTION = !USE_PREDICTION;
            }
            if (HasBeenPressed(Keys.I))
            {
                USE_INTERPOLATION = !USE_INTERPOLATION;
            }
            if (HasBeenPressed(Keys.R))
            {
                USE_RECONCILITION = !USE_RECONCILITION;
            }
            var kstate = Keyboard.GetState();

            gamebjects.ForEach(x => x.Update(gameTime));
            base.Update(gameTime);
        }
        static KeyboardState currentKeyState;
        static KeyboardState previousKeyState;

        public KeyboardState GetState()
        {
            previousKeyState = currentKeyState;
            currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            return currentKeyState;
        }

        public bool HasBeenPressed(Keys key)
        {
            return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
        }

    }
}
