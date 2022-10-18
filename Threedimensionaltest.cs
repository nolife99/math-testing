using OpenTK;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Storyboarding3d;
using StorybrewCommon.Animations;
using System;

namespace StorybrewScripts
{
    class Threedimensionaltest : StoryboardObjectGenerator
    {
        protected override void Generate()
        {
            /* var back = GetLayer("").CreateSprite("sb/p.png");
            back.ScaleVec(66985, 854, 480);
            back.Color(66985, 88585, "#000000", "#000000"); */
            Generate3dScene(66985, 88585);
        }
        void Generate3dScene(int startTime, int endTime)
        {
            var duration = endTime - startTime;

            var scene = new Scene3d();
            var camera = new PerspectiveCamera();

            camera.PositionX.Add(startTime, 0).Add(endTime, 0);
            camera.PositionY.Add(startTime, 0).Add(endTime, 0);
            camera.PositionZ.Add(startTime, -50).Add(endTime, -50);
            
            camera.NearClip.Add(startTime, 30);
            camera.FarClip.Add(startTime, 3000);

            camera.NearFade.Add(startTime, 10); 
            camera.FarFade.Add(startTime, 1000);

            var parent = scene.Root;

            for (var i = 0; i < 2000; i++)
            {
                Vector3 RandEndPos = new Vector3(Random(-8000, 8000), Random(-4500, 4500), i * 12.2f);

                var star = new Sprite3d()
                {
                    SpritePath = "sb/dot.png",
                    UseDistanceFade = true,
                    Additive = true
                };
                star.ConfigureGenerators(g =>
                {
                    g.PositionTolerance = 1;
                    g.ScaleTolerance = 2;
                    g.OpacityTolerance = 1;
                });
                
                star.Opacity.Add(startTime, 0)
                    .Add(startTime + 500, 0.8f)
                    .Until(endTime - 500)
                    .Add(endTime, 0);

                star.PositionX.Add(startTime, RandEndPos.X - 2000)
                    .Add(startTime + duration / 2, RandEndPos.X + 2000, EasingFunctions.SineOut)
                    .Add(endTime, RandEndPos.X - 2000, EasingFunctions.SineInOut);

                star.PositionY.Add(startTime, RandEndPos.Y).Add(endTime, RandEndPos.Y);

                star.PositionZ.Add(startTime, RandEndPos.Z)
                    .Add(startTime + duration / 2, RandEndPos.Z - 4000, EasingFunctions.SineOut)
                    .Add(endTime, RandEndPos.Z - 8000, EasingFunctions.QuintIn);

                star.SpriteScale.Add(startTime, new Vector2(10, 10))
                    .Add(startTime + 500, new Vector2(5, 5), EasingFunctions.QuadOut);
           
                parent.Add(star);
            }
            scene.Generate(camera, GetLayer(""), startTime, endTime, Beatmap.GetTimingPointAt(startTime).BeatDuration / 16);
        }
    }
}