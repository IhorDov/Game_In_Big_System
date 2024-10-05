using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Client.GameObjects
{
    public abstract class GameObject
    {
        public Vector2 Position { get => position; set => position = value; }

        private readonly string contentPath;
        ContentManager content;
        protected Vector2 position;

        public GameObject(string contentPath, ContentManager Content)
        {
            this.contentPath = contentPath;
            this.content = Content;
            //LoadContent();
        }
        //public virtual void LoadContent()
        //{
        //    texture = content.Load<Texture2D>(contentPath);
        //}
        public virtual void Init()
        {
        }
        public virtual void Update(GameTime gameTime)
        {

        }
        //public virtual void Draw(GameTime gameTime, SpriteBatch sb)
        //{
        //    sb.Draw(texture, position, null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
        //}
        public void SetPosition(Vector2 newPos)
        {
            this.position = newPos;
        }
    }
}
