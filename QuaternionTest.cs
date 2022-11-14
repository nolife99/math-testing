using OpenTK;
using OpenTK.Graphics;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Animations;
using System.Collections.Generic;
using System.Linq;
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
            var spinDuration = beat * durationMult;
            var index = 0; // split mechanism

            for (var r = .0; r < rings; r++) 
            {
                index++;
                for (var c = 1.0; c < ringDotCount; c++)
                {
                    if (index > 5 && index < 10) break;
                    else if (index == 10) index = 1;
                    if (c == ringDotCount / 2) continue;

                    var radius = baseScale * Math.Sin(c / (double)ringDotCount * Math.PI * 2);
                    var basePos = new Vector3d(
                        radius * Math.Cos(r / (double)rings * Math.PI),
                        baseScale * Math.Cos(c / (double)ringDotCount * Math.PI * 2),
                        radius * Math.Sin(r / (double)rings * Math.PI));
                        
                    var rotFunc = new Vector3(degRad(-50), 0, degRad(-30));
                    var pos = rotate(basePos, rotFunc);

                    var sprite = GetLayer("").CreateSprite("sb/d.png", OsbOrigin.Centre, new Vector2(0, (float)pos.Y + 240));
                    sprite.Fade(startTime + (c - 1) * 60, startTime + (c - 1) * 60 + 1000, 0, 1);
                    sprite.Fade(endTime - r * 30, endTime - r * 30 + 1000, 1, 0);
                    
                    var keyframe = new List<Keyframe<double>>();
                    for (var i = .0; i <= 360; i += .5)
                    {
                        pos = rotate(basePos, new Vector3(rotFunc.X, degRad(i), rotFunc.Z));
                        keyframe.Add(new Keyframe<double>(spinDuration / 360 * i, pos.X));
                    }
                    var maxFrame = getGreatestKeyframe(keyframe);

                    // For this movement, the algorithm to get the correct starting point would be:
                    // base start time + when keyframe reaches the max value out of the list - spin duration.

                    var sTime = startTime + maxFrame.Time - spinDuration;
                    sprite.StartLoopGroup(sTime, ceiling((endTime - startTime) / spinDuration));
                    sprite.MoveX(OsbEasing.InOutSine, 0, spinDuration / 2, 320 + maxFrame.Value, 320 - maxFrame.Value);
                    sprite.MoveX(OsbEasing.InOutSine, spinDuration / 2, spinDuration, 320 - maxFrame.Value, 320 + maxFrame.Value);
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

        Keyframe<double> getGreatestKeyframe(List<Keyframe<double>> list)
        {
            var maxVal = list.Max(t => t.Value);
            var finalKeyframe = new Keyframe<double>();

            // iterate through the list and find the keyframe that matches the value.
            foreach (var keyframe in list) if (maxVal == keyframe.Value) finalKeyframe = keyframe; 
            return finalKeyframe;
        }

        #endregion
    }
}
