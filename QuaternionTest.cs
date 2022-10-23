using OpenTK;
using OpenTK.Graphics;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Animations;
using System;

namespace StorybrewScripts
{
    /// <summary>
    /// Recreates the splitted, tilted sphere from Asymmetry (using accurate keyframing and quaternions).
    /// Verified working.
    /// </summary>
    class QuaternionTest : StoryboardObjectGenerator
    {
        /// <summary>
        /// Custom build of storybrew: <see href="http://github.com/nolife99/storybrew"/>
        /// </summary>
        protected override void Generate() => Generate(286414, 328928, 140, 18, 36, 17);
        void Generate(int startTime, int endTime, double baseScale, int rings, int ringDotCount, double durationMult)
        {
            var beat = Beatmap.GetTimingPointAt(startTime).BeatDuration;
            var rotFunc = new Vector3(degRad(-50), 0, degRad(-30));
            var index = 0; // split mechanism

            for (double r = 0; r < rings; r++) 
            {
                index++;
                for (double c = 1; c < ringDotCount; c++)
                {
                    if (index > 5 && index < 10) break;
                    else if (index == 10) index = 1;
                    if (c == ringDotCount / 2) continue;

                    var radius = baseScale * Math.Sin(c / (double)ringDotCount * Math.PI * 2);
                    var pos = rotate(new Vector3d(
                        radius * Math.Cos(r / (double)rings * Math.PI),
                        baseScale * Math.Cos(c / (double)ringDotCount * Math.PI * 2),
                        radius * Math.Sin(r / (double)rings * Math.PI)), rotFunc);

                    var sprite = GetLayer("").CreateSprite("sb/d.png", OsbOrigin.Centre, new Vector2(0, (float)pos.Y + 240));
                    sprite.Fade(startTime + (c - 1) * 60, startTime + (c - 1) * 60 + 1000, 0, 1);
                    sprite.Fade(endTime - r * 30, endTime - r * 30 + 1000, 1, 0);

                    var spinDuration = beat * durationMult;
                    sprite.StartLoopGroup(startTime, ceiling((endTime - startTime) / spinDuration));

                    var keyframe = new KeyframedValue<double>(null);
                    for (var i = 1; i <= 361; i++) // start at 1 because we are using the original 0 degree position.
                    {
                        var pos2 = rotate(new Vector3d(
                            radius * Math.Cos(r / (double)rings * Math.PI),
                            baseScale * Math.Cos(c / (double)ringDotCount * Math.PI * 2),
                            radius * Math.Sin(r / (double)rings * Math.PI)), 
                            new Vector3(rotFunc.X, degRad(i), rotFunc.Z));

                        keyframe.Add(spinDuration / 360 * (i - 1), pos.X + 320);
                        pos = pos2;
                    }
                    keyframe.Simplify1dKeyframes(1, d => (float)d);
                    keyframe.ForEachPair((s, e) => sprite.MoveX(s.Time, e.Time, (int)s.Value, (int)e.Value));

                    sprite.EndGroup();

                    if (index == 1 | index == 5)
                    {
                        sprite.Color(297385, 297728, Color4.DeepSkyBlue, Color4.IndianRed);
                        sprite.Color(308356, 308699, Color4.IndianRed, Color4.SeaGreen);
                        sprite.Color(319328, 319671, Color4.SeaGreen, Color4.Crimson);

                        Action<double, double> ScaleLoop = (MaxScale, AmpScale) =>
                        {
                            sprite.StartLoopGroup(startTime, ceiling((endTime - startTime) / (beat * 4)));
                            sprite.Scale(OsbEasing.InQuad, 0, beat, MaxScale, .03);
                            sprite.Scale(OsbEasing.InQuad, beat, beat * 2.5, AmpScale, .03);
                            sprite.Scale(OsbEasing.InQuad, beat * 2.5, beat * 3, MaxScale, .03);
                            sprite.Scale(OsbEasing.InQuad, beat * 3, beat * 4, AmpScale, .03);
                            sprite.EndGroup();
                        };
                        ScaleLoop(.065, .07);
                    }
                    else sprite.Scale(startTime, .035);
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