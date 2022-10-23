using OpenTK;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Storyboarding3d; // Debug assembly
using StorybrewCommon.Animations;
using System;

namespace StorybrewScripts
{
    /// <summary>
    /// Uses debug assemblies to make a simple 3D scene.
    /// </summary>
    class Threedimensionaltest : StoryboardObjectGenerator
    {
        /// <summary>
        /// Custom build of storybrew: <see href="http://github.com/nolife99/storybrew"/>
        /// </summary>
        protected override void Generate()
        {
            /* var back = GetLayer("").CreateSprite("sb/p.png");
            back.ScaleVec(66985, 854, 480);
            back.Color(66985, 88585, "#000000", "#000000"); */
            Generate3dScene(66985, 88585);
        }
        
        // Travel through star world
        void Generate3dScene(int startTime, int endTime)
        {
            var duration = endTime - startTime;

            var scene = new Scene3d();
            var camera = new PerspectiveCamera();

            camera.PositionX.Add(startTime, 0).Add(endTime, 0);
            camera.PositionY.Add(startTime, 0).Add(endTime, 0);
            camera.PositionZ.Add(startTime, -20).Add(endTime, -20);
            
            camera.NearClip.Add(startTime, 30);
            camera.FarClip.Add(startTime, 3000);

            camera.NearFade.Add(startTime, 10); 
            camera.FarFade.Add(startTime, 1000);

            var parent = scene.Root;
            parent.Rotation.Add(endTime - 8000, new Quaternion(0, 0, 0))
                .Add(endTime, new Quaternion(MathHelper.DegreesToRadians(90), 0, 0), EasingFunctions.QuintIn);

            for (var i = 0; i < 1750; i++)
            {
                Vector3 RandEndPos = new Vector3(Random(-5024, 5024), Random(-3600, 3600), i * 10);

                var star = new Sprite3d()
                {
                    SpritePath = "sb/d.png",
                    UseDistanceFade = true,
                    Additive = true,
                    RotationMode = RotationMode.Fixed
                };
                star.ConfigureGenerators(g =>
                {
                    g.PositionTolerance = 1.5;
                    g.ScaleTolerance = 3;
                    g.OpacityTolerance = 1.5;
                });
                
                star.Opacity.Add(startTime, 0)
                    .Add(startTime + 500, 0.8f)
                    .Until(endTime - 500)
                    .Add(endTime, 0);

                star.PositionX.Add(startTime, RandEndPos.X - 2000)
                    .Add(startTime + duration / 2, RandEndPos.X + 2000, EasingFunctions.SineOut)
                    .Add(endTime, RandEndPos.X - 1000, EasingFunctions.SineInOut);

                star.PositionY.Add(startTime, RandEndPos.Y).Add(endTime, RandEndPos.Y);

                star.PositionZ.Add(startTime, RandEndPos.Z)
                    .Add(startTime + duration / 2, RandEndPos.Z - 4000, EasingFunctions.SineOut)
                    .Add(endTime, RandEndPos.Z - 8000, EasingFunctions.QuintIn);

                star.SpriteScale.Add(startTime, new Vector2(.4f, .4f)).Add(startTime, new Vector2(.4f, .4f));
           
                parent.Add(star);
            }
            scene.Generate(camera, GetLayer(""), startTime, endTime, Beatmap.GetTimingPointAt(startTime).BeatDuration / 16);
        }
    }
}
