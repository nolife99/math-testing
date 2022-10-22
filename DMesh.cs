using OpenTK;
using OpenTK.Graphics;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using System;
using System.IO;
using System.Globalization;

namespace StorybrewScripts
{
    public class DMesh : StoryboardObjectGenerator
    {
        protected override void Generate()
        {
            GenerateMesh($"{ProjectPath}/assetlibrary/sphere.obj", new Vector4(286414, 328928, 4000, 150), new Vector2(320, 180), false, -0.4,  
            (sprite, startTime, endTime) => 
            {
                sprite.Fade(startTime, startTime + 500, 0, 1);
                sprite.ScaleVec(OsbEasing.OutQuint, startTime, startTime + 500, 7.5, 15, 1.5, 1.5);
                sprite.Fade(endTime, endTime + 250, 1, 0);

                sprite.StartTriggerGroup("HitSoundNormal", startTime, endTime);
                sprite.ScaleVec(OsbEasing.OutQuint, 0, 200, 3, 7.5, 1.5, 1.5);
                sprite.EndGroup();

                sprite.StartTriggerGroup("HitSoundClap", startTime, endTime);
                sprite.ScaleVec(OsbEasing.OutQuint, 0, 200, 7.5, 3, 1.5, 1.5);
                sprite.EndGroup();
            });

            GenerateMesh($"{ProjectPath}/assetlibrary/diamond.obj", new Vector4(308356, 328928, 4000, 2), new Vector2(320, 220), false, -0.4,
            (sprite, startTime, endTime) =>
            {
                sprite.Fade(startTime, startTime + 500, 0, 1);
                sprite.ScaleVec(OsbEasing.OutQuint, startTime, startTime + 500, 8, 16, 2, 2);
                sprite.Fade(endTime, endTime + 250, 1, 0);
                sprite.Color(startTime, Color4.RoyalBlue);

                sprite.StartTriggerGroup("HitSoundFinish", startTime, endTime);
                sprite.ScaleVec(OsbEasing.OutQuint, 0, 200, 5, 10, 2, 2);
                sprite.EndGroup();

                sprite.StartTriggerGroup("HitSoundWhistle", startTime, endTime);
                sprite.ScaleVec(OsbEasing.OutQuint, 0, 200, 8, 4, 2, 2);
                sprite.EndGroup();
            });
        }
        void GenerateMesh(string filepath, Vector4 times, Vector2 centrePos, bool color, double tilt, Action<OsbSprite, int, int> additions = null)
        {
            var startTime = (int)times.X;
            var endTime = (int)times.Y;
            var spinDuration = (int)times.Z;
            var scale = times.W;

            foreach (var line in File.ReadAllLines(filepath))
            {
                var arg = line.Split(new[]{' '}, 4);
                if (arg[0] == "v")
                {
                    var threeDpos = new Vector3(
                        (float)Math.Round(double.Parse(arg[1], NumberStyles.Float), 6),
                        (float)Math.Round(double.Parse(arg[2], NumberStyles.Float), 6),
                        (float)Math.Round(double.Parse(arg[3], NumberStyles.Float), 6));

                    Rotate(threeDpos, new Vector3(MathHelper.DegreesToRadians(-10), 0, 0));

                    var sprite = GetLayer("").CreateSprite("sb/p.png");

                    var angle = Math.Atan2(threeDpos.Z, threeDpos.X);
                    var radius = scale * Distance(new Vector2(threeDpos.X, threeDpos.Z), new Vector2(0, 0));
                    var delay = spinDuration * (angle / (Math.PI * 2));

                    sprite.MoveY(startTime - delay - (int)spinDuration / 1.5, threeDpos.Y * radius + centrePos.Y);

                    sprite.StartLoopGroup(startTime - delay - (int)spinDuration / 1.5, (endTime - startTime) / spinDuration + 2);
                    sprite.MoveX(OsbEasing.InOutSine, 0, spinDuration / 2, centrePos.X - radius * threeDpos.X, centrePos.X + radius * threeDpos.X);
                    sprite.MoveX(OsbEasing.InOutSine, spinDuration / 2, spinDuration, centrePos.X + radius * threeDpos.X, centrePos.X - radius * threeDpos.X);
                    sprite.EndGroup();

                    if (additions != null) additions(sprite, startTime, endTime);
                }
            }
        }
        static double Distance(Vector2 point1, Vector2 point2) 
            => Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));

        public Vector3 Rotate(Vector3 Vector, Vector3 Rotation)
        {
            var Rot = new Quaternion(Rotation.Z, Rotation.Y, Rotation.X);
            return Vector3.Transform(Vector, Rot);
        }
    }
}