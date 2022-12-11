using OpenTK;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.OpenTKUtil;
using System;

using static StorybrewCommon.OpenTKUtil.MathHelper;

namespace StorybrewScripts
{
    /// <summary>
    /// Replicates a transformed and splitted sphere.
    /// <para>Verified working.</para>
    /// </summary>
    class QuaternionTest : StoryboardObjectGenerator
    {
        protected override void Generate()
        {
            MakeSphere(286414, 328928, 130, 5, new Vector2i(36, 20), 20, sp => 
            {
                sp.Color(297385, 297728, 0, .75, 1, .8, .36, .36);
                sp.Color(308356, 308699, .8, .36, .36, .18, .55, .34);
                sp.Color(319328, 319671, .18, .55, .34, .86, .08, .24);
            });
        }

        ///<summary> Generates a transformed sphere that rotates about the yaw axis </summary>
        ///<param name="start"> Start time of the object </param>
        ///<param name="end"> End time of the object </param>
        ///<param name="size"> Size multiplier of the object </param>
        ///<param name="split"> Amount of dots between equal splits </param>
        ///<param name="dots"> X and Y subdivisions of the object <para>Should be positive integers</para></param>
        ///<param name="spinMult"> Spin duration multiplier of the object </param>
        ///<param name="action"> Any wanted additions to the "splitted" dots </param>
        void MakeSphere(int start, int end, double size, uint split, Vector2i dots, double spinMult, Action<OsbSprite> action = null)
        {
            var beat = Beatmap.GetTimingPointAt(start).BeatDuration;
            var spinDur = beat * spinMult;

            var i = 1;
            for (double r = 0; r < dots.X; r++, i++) for (double c = 1; c < dots.Y; c++)
            {
                if (i > split && i < split * 2) break;
                else if (i == split * 2) i = 1;

                var rad = size * Sin(c / dots.Y * Pi);
                var basePos = new Vector3d(rad * Cos(r / dots.X * TwoPi), size * Cos(c / dots.Y * Pi), rad * Sin(r / dots.X * TwoPi));

                var rotFunc = new Vector3d(DegreesToRadians(42.5), 0, DegreesToRadians(25));
                var pos = Vector3d.Transform(basePos, new Quaterniond(rotFunc));

                var sprite = GetLayer("").CreateSprite("sb/dot.png", OsbOrigin.Centre, new Vector2(0, (float)pos.Y + 240));
                sprite.Fade(start + r * 40, start + r * 40 + beat * 4, 0, 1);
                sprite.Fade(end - c * 40, end - c * 40 + beat * 4, 1, 0);

                var sTime = start - spinDur * Atan2(pos.Z, pos.X) / TwoPi;
                if (sTime > start + r * 40) sTime -= spinDur;
                sprite.StartLoopGroup(sTime, (int)Ceiling((end - c * 40 + beat * 2 - sTime) / spinDur));

                var maxRad = Sqrt(pos.X * pos.X + pos.Z * pos.Z);
                sprite.MoveX(OsbEasing.InOutSine, 0, spinDur / 2, 320 + maxRad, 320 - maxRad);
                sprite.MoveX(OsbEasing.InOutSine, spinDur / 2, spinDur, 320 - maxRad, 320 + maxRad);
                sprite.EndGroup();

                sprite.Scale(start, .25);
                if (split != 0 && i == 1 || i == split)
                {
                    sprite.Additive(start);
                    Action<double, double> ScaleLoop = (MaxScale, AmpScale) =>
                    {
                        sprite.StartLoopGroup(start, (int)Ceiling((end - start) / (beat * 4)));
                        sprite.Scale(OsbEasing.InQuad, 0, beat, MaxScale, .25);
                        sprite.Scale(OsbEasing.InQuad, beat, beat * 2.5, AmpScale, .25);
                        sprite.Scale(OsbEasing.InQuad, beat * 2.5, beat * 3, MaxScale, .25);
                        sprite.Scale(OsbEasing.InQuad, beat * 3, beat * 4, AmpScale, .25);
                        sprite.EndGroup();
                    };
                    ScaleLoop(.5, .55);

                    if (action != null) action(sprite);
                }
            }
        }
    }
}