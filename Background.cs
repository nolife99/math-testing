using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using OpenTK;
using OpenTK.Graphics;
using System.Linq;
using StorybrewScripts.Midori.Image;

namespace StorybrewScripts
{
    class Background : StoryboardObjectGenerator
    {
        [Configurable] public string BackgroundPath = "";
        [Configurable] public int StartTime = 0;
        [Configurable] public int EndTime = 0;
        [Configurable] public double Opacity = 0.2;

        protected override void Generate()
        {
            if (BackgroundPath == "") BackgroundPath = Beatmap.BackgroundPath ?? string.Empty;
            if (StartTime == EndTime) EndTime = (int)(Beatmap.HitObjects.LastOrDefault()?.EndTime ?? AudioDuration);

            var bitmap = GetMapsetBitmap(BackgroundPath);
            var bg = GetLayer("").CreateSprite(BackgroundPath, OsbOrigin.Centre, new Vector2(320, 240));
            bg.Fade(0, 0);
            bg.Scale(StartTime, 854.0f / bitmap.Width);
            bg.Fade(StartTime - 500, StartTime, 0, Opacity);
            bg.Fade(EndTime, EndTime + 500, Opacity, 0);

            //Dissolve(285042, 286414, BackgroundPath, "sb/p.png", 854f / bitmap.Width, 854f / 30f, new Vector2(-107, -27));
        }
        void Dissolve(int startTime, int endTime, string baseImagePath, string spritePath, float baseImageScale, float pixelSize, Vector2 position)
        {
            var baseImage = GetMapsetBitmap(baseImagePath);
            var spriteScale = ImageHelper.GetScaleRatio(spritePath, pixelSize) * baseImageScale;
            var xMax = baseImage.Width / pixelSize;
            var yMax = baseImage.Height / pixelSize;

            for (int i = 0; i < xMax; i++)
            {
                for (int j = 0; j < yMax; j++)
                {
                    var location = new Vector2(i * pixelSize * baseImageScale, j * pixelSize * baseImageScale) + position;
                    var color = (Color4)baseImage.GetPixel(i * (int)pixelSize, j * (int)pixelSize);

                    var sprite = GetLayer("").CreateSprite(spritePath, OsbOrigin.TopLeft, location);
                    sprite.Fade(startTime, startTime + 1000, 0, 1);
                    sprite.Color(startTime, color);
                    sprite.Scale(startTime, spriteScale);
                    sprite.Fade(OsbEasing.Out, endTime, endTime + 500, 1, 0);
                }
            }
        }
    }
}
