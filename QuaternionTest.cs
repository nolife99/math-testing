using OpenTK;
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
        protected override void Generate() => Generate(286414, 328928, 140, 18, 36, 17.5);
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
                    var pos = Rotate(new Vector3(
                        (float)(r * Math.Cos(s / (double)rings * Math.PI)),
                        (float)(baseScale * Math.Cos(c / (double)ringDotCount * Math.PI * 2)),
                        (float)(r * Math.Sin(s / (double)rings * Math.PI))), rotFunc);

                    var sprite = GetLayer("").CreateSprite("sb/dot.png", OsbOrigin.Centre, new Vector2(0, pos.Y + 240));
                    sprite.Fade(startTime + (c - 1) * 60, startTime + (c - 1) * 60 + 1000, 0, 1);
                    sprite.Fade(endTime - s * 30, endTime - s * 30 + 1000, 1, 0);

                    var spinDuration = beat * durationMult;
                    sprite.StartLoopGroup(startTime, (int)Math.Ceiling((double)(endTime - startTime) / spinDuration));

                    var keyframe = new KeyframedValue<float>(null);
                    for (var i = 2; i <= 62; i++)
                    {
                        var pos2 = Rotate(new Vector3(
                            (float)(r * Math.Cos(s / (double)rings * Math.PI)),
                            (float)(baseScale * Math.Cos(c / (double)ringDotCount * Math.PI * 2)),
                            (float)(r * Math.Sin(s / (double)rings * Math.PI))), 
                            new Vector3(rotFunc.X, degRad(.1 + i * 6), rotFunc.Z));

                        keyframe.Add(spinDuration / 60 * (i - 2), pos.X + 320);
                        pos = pos2;
                    }
                    keyframe.Simplify1dKeyframes(1, f => f);
                    keyframe.ForEachPair((start, end) => sprite.MoveX(start.Time, end.Time, start.Value, end.Value));

                    sprite.EndGroup();

                    if (count == 1 | count == 5)
                    {
                        sprite.Additive(startTime);
                        sprite.ColorHsb(startTime, 190, .8, .8);

                        Action<double, double> ScaleLoop = (MaxScale, AmpScale) =>
                        {
                            sprite.StartLoopGroup(startTime, (int)Math.Ceiling((endTime - startTime) / (beat * 4)));
                            sprite.Scale(OsbEasing.In, 0, beat, MaxScale, .25);
                            sprite.Scale(OsbEasing.In, beat, beat * 2.5, AmpScale, .25);
                            sprite.Scale(OsbEasing.In, beat * 2.5, beat * 3, MaxScale, .25);
                            sprite.Scale(OsbEasing.In, beat * 3, beat * 4, AmpScale, .25);
                            sprite.EndGroup();
                        };
                        ScaleLoop(.5, .6);
                    }
                    else sprite.Scale(startTime, .3);
                }
            }
        }
        Vector3 Rotate(Vector3 Vector, Vector3 Rotation) => Vector3.Transform(Vector, new Quaternion(Rotation.X, Rotation.Y, Rotation.Z));
        float degRad(double value) => MathHelper.DegreesToRadians((float)value);
    }
}
