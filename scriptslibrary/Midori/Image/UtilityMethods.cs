using OpenTK;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts.Midori.Image
{
    class ImageHelper
    {
        /// <summary>
        /// Given an image file and the desired resized dimensions, return the necessary ratio for a ScaleVec command.
        /// </summary>
        public static Vector2 GetScaleRatio(string filename, Vector2 size)
        {
            var bitmap = StoryboardObjectGenerator.Current.GetMapsetBitmap(filename);
            return new Vector2(size.X / bitmap.Width, size.Y / bitmap.Height);
        }

        public static Vector2 GetScaleRatio(string filename) => GetScaleRatio(filename, new Vector2(854, 480)); // TODO: Put these in some constants library


        public static float GetScaleRatio(string filename, float cap, bool useHeight = true)
        {
            var bitmap = StoryboardObjectGenerator.Current.GetMapsetBitmap(filename);
            return cap / (useHeight ? bitmap.Height : bitmap.Width);
        }

        /// <summary>
        /// TODO: Given the TopLeft coordinate and the OsbOrigin, find the proper Vector2 for a sprite.
        /// </summary>
        public static Vector2 FindPointToOrigin(Vector2 old, OsbOrigin origin)
        {
            return old;
        }
    }
}