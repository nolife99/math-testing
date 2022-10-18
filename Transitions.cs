using OpenTK;
using OpenTK.Graphics;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using System;

namespace StorybrewScripts
{
    class Transitions : StoryboardObjectGenerator
    {
        protected override void Generate()
        {
            SquareTransition(65614, 66814, 66985, 30, Color4.White);
		    SquareTransition(285042, 285728, 286414, 30, Color4.White);
        }
        void SquareTransition(int startTime, int endTime, int endFade, int amount, Color4 color)
        {
            var squareScale = 854f / amount;
            var posX = -107 + squareScale / 2;
            var posY = squareScale / 2;
            var duration = endTime - startTime;

            var delay = 0;
            while (posX < 747 + squareScale / 2)
            {
                while (posY < 480 + squareScale / 2)
                {
                    var sprite = GetLayer("").CreateSprite("sb/p.png", OsbOrigin.Centre, new Vector2(posX, posY));
                    sprite.Scale(startTime + delay, endTime, 0, squareScale);
                    sprite.Rotate(startTime + delay, endTime, 0, Math.PI);
                    if (color != Color4.White) sprite.Color(endFade, color);
                    sprite.Fade(OsbEasing.Out, endFade, endFade + 500, 1, 0);

                    posY += squareScale;
                }
                posY = squareScale / 2;
                posX += squareScale;
                delay += 15;
            }
        }
    }
}
