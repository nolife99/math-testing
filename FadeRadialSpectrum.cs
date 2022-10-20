using OpenTK;
using StorybrewCommon.Animations;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using System;

namespace StorybrewScripts
{
    class FadeRadialSpectrum : StoryboardObjectGenerator
    {
        protected override void Generate() => RadialSpectrum(286414, 328928, 20, new Vector4(2, 8, 8, 120), new Vector2(320, 240));
        void RadialSpectrum(int startTime, int endTime, int ColumnCount, Vector4 size, Vector2 center)
        {
            var ColumnGap = size.X;
            var BarGap = size.Y;
            var InitRadius = size.Z;
            var MaxRadius = size.W;

            var distance = 360 / ColumnCount * Math.PI / 180;

            for (var i = 0; i < ColumnCount; i++)
            {
                var keyframes = new KeyframedValue<float>(null);

                var beat = Beatmap.GetTimingPointAt(startTime).BeatDuration;
                for (var time = (double)startTime; time < endTime + beat / 4; time += beat / 4)
                {
                    var fft = GetFft(time + 40, ColumnCount + 5, null, OsbEasing.InExpo);
                    var barHeight = Math.Pow(Math.Log10(1 + fft[i] * 75), 1.25) * MaxRadius;
                    if (barHeight < InitRadius) barHeight = InitRadius;
                    keyframes.Add(time, (float)barHeight);
                }
                keyframes.Simplify1dKeyframes(1, h => h);

                for (var t = 0; t < Math.Floor((MaxRadius - InitRadius) / BarGap); t++)
                {
                    var radius = InitRadius + t * BarGap;
                    var columnAngle = distance * i;
                    var x = radius * Math.Cos(columnAngle) + center.X;
                    var y = radius * Math.Sin(columnAngle) + center.Y;
                    var width = Math.Abs(radius * Math.Sin(distance / 2) * 2 - ColumnGap);
                    if (width < .5) width = .5;
                    var scale = new Vector2((float)width, 3);

                    var keyframe = new KeyframedValue<float>(null);
                    foreach (var frame in keyframes)
                    {
                        if (frame.Value >= InitRadius + t * BarGap) keyframe.Add(frame.Time, 1 - t * .05f);
                        else keyframe.Add(frame.Time, 0);
                    }

                    var sprite = GetLayer("").CreateSprite("sb/p.png", OsbOrigin.Centre, new Vector2((float)x, (float)y));
                    keyframe.ForEachPair((start, end) => sprite.Fade(start.Time, end.Time, start.Value, end.Value));

                    if (sprite.CommandCount > 0) 
                    {
                        var rotation = distance * i + Math.PI / 2;
                        if (rotation != Math.PI && rotation != Math.PI * 2) sprite.Rotate(startTime, rotation);
                        sprite.ScaleVec(startTime, scale);
                        if (sprite.OpacityAt(endTime + 1) > 0) sprite.Fade(endTime, endTime + beat, sprite.OpacityAt(endTime), 0);
                    }
                }
            }
        }
    }
}