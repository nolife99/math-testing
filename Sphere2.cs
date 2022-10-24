using OpenTK;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Animations;
using System;

namespace StorybrewScripts
{
    /// <summary>
    /// Deprecated, not working
    /// </summary>
    class Sphere2 : StoryboardObjectGenerator
    {
        /// <summary>
        /// Custom build of storybrew: <see href="http://github.com/nolife99/storybrew"/>
        /// </summary>
        protected override void Generate()
        {
            int Radius = 120;
            var Centre = new Vector2(320, 240);
            double Gap = 10;

            for (var i = 240 - Radius + Gap / 2; i <= 240 + Radius * 2 - Gap / 2; i += Gap)
            {    
                int count = 1;
                for (int deg = 0; deg < 360; deg += 6)
                {
                    count++;
                    if (count > 3 && count < 6) continue;
                    else if (count == 6) count = 1;

                    int time = 176699;
                    int endTime = 197271;
                    var rad = deg * Math.PI / 180;
                    var tiltAngle = 0 * Math.PI / 180;

                    var height = 240 - i;
                    var length = 2 * Math.Sqrt(Math.Abs(height * height - Radius * Radius));

                    var X = height / 2 * Math.Cos(tiltAngle) + 320;
                    var Y = height / 2 * Math.Sin(tiltAngle) + i;

                    var radius = new Vector2((float)length / 2, (float)(Radius - Math.Abs(height)) / 2);

                    var x = radius.X * Math.Cos(rad) * Math.Cos(tiltAngle) - radius.Y * Math.Sin(rad) * Math.Sin(tiltAngle) + X;
                    var y = radius.X * Math.Cos(rad) * Math.Sin(tiltAngle) + radius.Y * Math.Sin(rad) * Math.Cos(tiltAngle) + Y;

                    var b = GetLayer("").CreateSprite("sb/p.png", OsbOrigin.Centre);
                    b.Scale(time, 5);
                    b.Rotate(time, tiltAngle);
                    b.Fade(time, time + 500, 0, 1);
                    b.Fade(endTime, endTime + 500, 1, 0);

                    var keyframe = new KeyframedValue<Vector2>(null);
                    
                    int time1 = time;
                    for (int deg2 = deg + 6; deg2 < deg + 360; deg2 += 3)
                    {
                        var rad2 = deg2 * Math.PI / 180;

                        var x1 = radius.X * Math.Cos(rad2) * Math.Cos(tiltAngle) - radius.Y * Math.Sin(rad2) * Math.Sin(tiltAngle) + X;
                        var y1 = radius.X * Math.Cos(rad2) * Math.Sin(tiltAngle) + radius.Y * Math.Sin(rad2) * Math.Cos(tiltAngle) + Y;

                        keyframe.Add(time1, new Vector2((float)x, (float)y));

                        x = x1;
                        y = y1;
                        time1 += 40;
                    }
                    keyframe.Simplify2dKeyframes(1, f => f);
                    b.StartLoopGroup(time, (int)Math.Ceiling((double)(endTime - time) / (keyframe.Count * 100) - 3));
                    keyframe.ForEachPair((start, end) => b.Move(start.Time - time, end.Time - time, start.Value, end.Value), Centre, s => s);
                    b.EndGroup();
                }
            }
        }
    }
}