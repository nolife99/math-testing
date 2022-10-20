using OpenTK;
using OpenTK.Graphics;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Animations;
using System;

namespace StorybrewScripts
{
    class QuaternionTest : StoryboardObjectGenerator
    {
        protected override void Generate() => Generate(286414, 328928, 140, 18, 36, 17);
        void Generate(int startTime, int endTime, double baseScale, int rings, int ringDotCount, double durationMult)
        {
            var beat = Beatmap.GetTimingPointAt(startTime).BeatDuration;
            var rotFunc = new Vector3(degRad(-50), .1f, degRad(-30));
            var count = 0;

            for (double s = 0; s < rings; s++) 
            {
                count++;
                for (double c = 1; c < ringDotCount; c++)
                {
                    if (count > 5 && count < 10) continue;
                    else if (count == 10) count = 1;

                    if (c == ringDotCount / 2) continue;

                    var r = baseScale * Math.Sin(c / (double)ringDotCount * Math.PI * 2);
                    var pos = rotate(new Vector3d(
                        r * Math.Cos(s / (double)rings * Math.PI),
                        baseScale * Math.Cos(c / (double)ringDotCount * Math.PI * 2),
                        r * Math.Sin(s / (double)rings * Math.PI)), rotFunc);

                    var sprite = GetLayer("").CreateSprite("sb/dot.png", OsbOrigin.Centre, new Vector2(0, (float)pos.Y + 240));
                    sprite.Fade(startTime + (c - 1) * 60, startTime + (c - 1) * 60 + 1000, 0, 1);
                    sprite.Fade(endTime - s * 30, endTime - s * 30 + 1000, 1, 0);

                    var spinDuration = beat * durationMult;
                    sprite.StartLoopGroup(startTime, ceiling((endTime - startTime) / spinDuration));

                    var keyframe = new KeyframedValue<double>(null);
                    for (var i = 2; i <= 62; i++)
                    {
                        var pos2 = rotate(new Vector3d(
                            r * Math.Cos(s / (double)rings * Math.PI),
                            baseScale * Math.Cos(c / (double)ringDotCount * Math.PI * 2),
                            r * Math.Sin(s / (double)rings * Math.PI)), 
                            new Vector3(rotFunc.X, degRad(.1 + i * 6), rotFunc.Z));

                        keyframe.Add(spinDuration / 60 * (i - 2), pos.X + 320);
                        pos = pos2;
                    }
                    keyframe.Simplify1dKeyframes(1, f => (float)f);
                    keyframe.ForEachPair((start, end) => sprite.MoveX(start.Time, end.Time, start.Value, end.Value));

                    sprite.EndGroup();

                    if (count == 1 | count == 5)
                    {
                        sprite.Additive(startTime);
                        sprite.Color(297385, 297728, Color4.DeepSkyBlue, Color4.IndianRed);
                        sprite.Color(308356, 308699, Color4.IndianRed, Color4.SeaGreen);
                        sprite.Color(319328, 319671, Color4.SeaGreen, Color4.Crimson);

                        Action<double, double> ScaleLoop = (MaxScale, AmpScale) =>
                        {
                            sprite.StartLoopGroup(startTime, ceiling((endTime - startTime) / (beat * 4)));
                            sprite.Scale(OsbEasing.InQuad, 0, beat, MaxScale, .25);
                            sprite.Scale(OsbEasing.InQuad, beat, beat * 2.5, AmpScale, .25);
                            sprite.Scale(OsbEasing.InQuad, beat * 2.5, beat * 3, MaxScale, .25);
                            sprite.Scale(OsbEasing.InQuad, beat * 3, beat * 4, AmpScale, .25);
                            sprite.EndGroup();
                        };
                        ScaleLoop(.5, .6);
                    }
                    else sprite.Scale(startTime, .3);
                }
            }
        }

        #region ExtensionMethods

        Vector3d rotate(Vector3d Vector, Vector3 Rotation) => Vector3d.Transform(Vector, new Quaterniond(Rotation.X, Rotation.Y, Rotation.Z));
        float degRad(double value) => MathHelper.DegreesToRadians((float)value);
        int ceiling(double value) => (int)Math.Ceiling(value);

        #endregion
    }
}