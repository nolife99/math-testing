using OpenTK;
using OpenTK.Graphics;

namespace StorybrewScripts.Midori.Image
{
    /// <summary>
    /// Stores common information for a sprite for easy generation and utilization with effects.
    /// </summary>
    public class SpriteDescription
    {
        public string spritePath = "";
        public Vector2 location = Vector2.Zero;
        public float scale = 1f;
        public Color4 color = Color4.White;
    }
}