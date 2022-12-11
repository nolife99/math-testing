using OpenTK;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using System;

namespace StorybrewScripts
{
    /// <summary>
    /// Replicates a splitted, tilted sphere (like the 3D objects in world.execute(me);) without using 3D assemblies.
    /// Verified working.
    /// </summary>
    class Sphere : StoryboardObjectGenerator
    {
        [Configurable(DisplayName = "Start Time")] public int StartTime = 0;
        [Configurable(DisplayName = "End Time")] public int EndTime = 10000;
        [Configurable(DisplayName = "Duration Multiplier")] public double DurationMult = 8;
        [Configurable(DisplayName = "Dot Split Interval")] public int SplitInterval = 3;
        [Configurable(DisplayName = "Shape Size")] public float MeshSize = 200;
        [Configurable(DisplayName = "Bitmap Divisor")] public Vector2 Subdivision = new Vector2(20, 20);
        [Configurable] public float Tilt = 10;
        [Configurable] public Vector2 Center = new Vector2(320, 240);
        [Configurable(DisplayName = "Rotation Direction")] public Direction SpinDir = Direction.Clockwise;

        /// <summary>
        /// Custom build of storybrew: <see href="http://github.com/nolife99/storybrew"/>
        /// </summary>
        protected override void Generate()
        {
            var beat = Beatmap.GetTimingPointAt(StartTime).BeatDuration;

            var tiltAngle = Math.Sin(MathHelper.DegreesToRadians(Tilt));
            var spinDuration = beat * DurationMult;
            double startTime = StartTime;

            var count = 1;
            for (var i = 0; i < Subdivision.X; i++)
            {
                count++;
                for (var j = 1; j < Subdivision.Y; j++)
                {
                    if (count > SplitInterval && count < SplitInterval * 2) continue;
                    else if (count == SplitInterval * 2) count = 1;

                    var angle = Math.PI / 2 - j * (Math.PI / Subdivision.Y);
                    var position = new Vector2(
                        (float)(MeshSize * Math.Cos(angle)),
                        (float)(MeshSize * Math.Sin(angle)));
                        
                    var translated = Center + position;

                    var sprite = GetLayer("").CreateSprite("sb/dot.png");
                    
                    sprite.Fade(OsbEasing.OutQuad, StartTime, StartTime + beat, 0, 1);
                    sprite.Fade(OsbEasing.In, EndTime, EndTime + beat, 1, 0);

                    sprite.StartLoopGroup(startTime, (int)Math.Ceiling((EndTime + beat - startTime) / spinDuration));
                    sprite.MoveX(OsbEasing.InOutSine, 0, spinDuration / 2, translated.X, Center.X - position.X);
                    sprite.MoveX(OsbEasing.InOutSine, spinDuration / 2, spinDuration, Center.X - position.X, translated.X);

                    sprite.MoveY(OsbEasing.OutSine, 0, spinDuration / 4, 
                        translated.Y, translated.Y + (int)SpinDir * Math.Cos(angle) * MeshSize * tiltAngle);

                    sprite.MoveY(OsbEasing.InOutSine, spinDuration / 4, spinDuration * .75, 
                        translated.Y + (int)SpinDir * Math.Cos(angle) * MeshSize * tiltAngle,
                        translated.Y - (int)SpinDir * Math.Cos(angle) * MeshSize * tiltAngle);

                    sprite.MoveY(OsbEasing.InSine, spinDuration * .75, spinDuration, 
                        translated.Y - (int)SpinDir * Math.Cos(angle) * MeshSize * tiltAngle, translated.Y);

                    if (count != 1 && count != SplitInterval)
                    {
                        if (SpinDir == Direction.Clockwise)
                        {
                            sprite.Scale(OsbEasing.Out, 0, spinDuration / 4, .25, .3);
                            sprite.Scale(OsbEasing.InOutSine, spinDuration / 4, spinDuration * .75, .3, .2);
                            sprite.Scale(OsbEasing.In, spinDuration * .75, spinDuration, .2, .25);
                        }
                        else
                        {
                            sprite.Scale(OsbEasing.Out, 0, spinDuration / 4, .25, .2);
                            sprite.Scale(OsbEasing.InOutSine, spinDuration / 4, spinDuration * .75, .2, .3);
                            sprite.Scale(OsbEasing.In, spinDuration * .75, spinDuration, .3, .25);
                        }
                    }
                    sprite.EndGroup();

                    if (SplitInterval > 0 && count == 1 || count == SplitInterval)
                    {
                        sprite.Additive(StartTime);
                        sprite.ColorHsb(StartTime, 190, .8, .8);

                        Action<double, double> ScaleLoop = (MaxScale, AmpScale) =>
                        {
                            sprite.StartLoopGroup(StartTime, (int)Math.Ceiling((EndTime - StartTime) / (beat * 4)));
                            sprite.Scale(OsbEasing.In, 0, beat, MaxScale, .25);
                            sprite.Scale(OsbEasing.In, beat, beat * 2.5, AmpScale, .25);
                            sprite.Scale(OsbEasing.In, beat * 2.5, beat * 3, MaxScale, .25);
                            sprite.Scale(OsbEasing.In, beat * 3, beat * 4, AmpScale, .25);
                            sprite.EndGroup();
                        };
                        ScaleLoop(.5, .6);
                    }
                }
                startTime -= spinDuration / Subdivision.X;
            }
        }
        public enum Direction
        {
            Clockwise = 1, CounterClockwise = -1
        }
    }
}