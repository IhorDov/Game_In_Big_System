using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Client.GameObjects
{
    internal class Button : GameObject
    {
        private Action onButtonClicked;

        public Button(string contentPath, ContentManager Content, Vector2 startPos, Action onButtonClicked) : base(contentPath, Content)
        {
            this.position = startPos;
            this.onButtonClicked = onButtonClicked;

        }
        MouseState previousMouseState;
        public override void Update(GameTime gameTime)
        {
            previousMouseState = Mouse.GetState();
            if (IsClicked(previousMouseState))
            {
                onButtonClicked.Invoke();
            }
        }

        public bool IsClicked(MouseState previousMouseState)
        {
            var mouseState = Mouse.GetState();
            var mousePosition = new Point(mouseState.X, mouseState.Y);

            bool isMouseClick = mouseState.LeftButton == ButtonState.Released &&
                                previousMouseState.LeftButton == ButtonState.Pressed;

            return isMouseClick;
        }
    }
}
