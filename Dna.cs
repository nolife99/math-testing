using OpenTK;
using OpenTK.Graphics;
using StorybrewCommon.Animations;
using StorybrewCommon.Mapset;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Storyboarding.CommandValues;
using StorybrewCommon.Storyboarding3d;
using System;

namespace StorybrewScripts
{
    public class Dna : StoryboardObjectGenerator
    {
        [Configurable]
        public int StartTime = 100286;

        [Configurable]
        public int EndTime = 114686;

        [Configurable]
        public int Amount = 16;

        [Configurable]
        public int Scale = 60;

        [Configurable]
        public float Radius = 2;

        [Configurable]
        public int Height = 1;

        [Configurable]
        public int CameraHeight = 20;

        [Configurable]
        public float Fov = 80;

        [Configurable]
        public int LoopCount = 32;

        [Configurable]
        public float Thickness = 0.1f;

        [Configurable]
        public Color4 Color = Color4.White;

        private CommandColor lightBrown = CommandColor.FromHtml("#B8A58D");
        private CommandColor darkBrown = CommandColor.FromHtml("#525252");
        private CommandColor lightBlue = CommandColor.FromHtml("#4E707D");
        private CommandColor darkBlue = CommandColor.FromHtml("#0B131F");

        protected override void Generate()
        {
            var loopDuration = (EndTime - StartTime) / LoopCount;
            var startTime = StartTime;
            var loopStartTime = StartTime + loopDuration;
            var loopEndTime = loopStartTime + loopDuration;
            var fadeTime = EndTime - loopDuration;
            var endTime = EndTime;
            var timeStep = Beatmap.GetTimingPointAt(StartTime).BeatDuration / 8;

            var camera = new PerspectiveCamera();
            camera.HorizontalFov.Add(startTime, Fov);
            camera.PositionX.Add(startTime, 0);
            camera.PositionY.Add(startTime, 0);
            camera.PositionZ.Add(startTime, CameraHeight * Scale);

            var inScene = makeLoopScene(startTime, loopStartTime, camera, EasingFunctions.SineIn);
            applyCommonStates(inScene);
            inScene.Root.ScaleY
                .Add(startTime, 0)
                .Add(loopStartTime, 1);
            inScene.Generate(camera, GetLayer(""), startTime, loopStartTime, timeStep);

            if (LoopCount > 2)
            {
                var loopScene = makeLoopScene(loopStartTime, loopEndTime, camera, EasingFunctions.Linear);
                applyCommonStates(loopScene);
                loopScene.Generate(camera, GetLayer(""), loopStartTime, loopEndTime, timeStep, LoopCount - 2);
            }

            var outScene = makeLoopScene(fadeTime, endTime, camera, EasingFunctions.Linear);
            applyCommonStates(outScene);
            outScene.Root.ScaleY
                .Add(fadeTime, 1)
                .Add(endTime, 0);
            outScene.Generate(camera, GetLayer(""), fadeTime, endTime, timeStep);

            var extraScene = makeExtraScene(startTime, endTime, loopDuration, camera);
            applyCommonStates(extraScene);
            extraScene.Root.Opacity
                .Add(startTime, 0)
                .Add(loopStartTime, 1)
                .Until(fadeTime)
                .Add(endTime, 0);
            extraScene.Generate(camera, GetLayer(""), startTime, endTime, timeStep);
        }

        public void applyCommonStates(Scene3d scene)
        {
            scene.Root.PositionX.Add(StartTime, Scale * -3);
            scene.Root.PositionY.Add(StartTime, Scale * Amount / 2);
            scene.Root.Rotation.Add(StartTime, new Vector3(1, 0.8f, -0.1f).Normalized(), (float)Math.PI * 0.5f);
        }

        public Scene3d makeExtraScene(double startTime, double endTime, double movementDuration, Camera camera)
        {
            var scene = new Scene3d();
            return scene;
        }

        public Scene3d makeLoopScene(double startTime, double endTime, Camera camera, Func<double, double> easing)
        {
            var scene = new Scene3d();

            var grid = new Node3d();
            grid.ScaleX.Add(startTime, Scale);
            grid.ScaleY.Add(startTime, Scale);
            grid.ScaleZ.Add(startTime, Scale);
            grid.Rotation
                .Add(startTime, 0)
                .Add(endTime, -(float)Math.PI, easing);
            grid.Coloring.Add(startTime, Color == Color4.White ? darkBlue : (CommandColor)Color);
            scene.Add(grid);

            var prevLeftPos = new Vector3();
            var prevRightPos = new Vector3();
            for (var i = 0; i <= Amount; i++)
            {
                var angle = i * 0.6f;
                var leftPos = new Vector3((float)Math.Cos(angle) * Radius, (float)Math.Sin(angle) * Radius, i * Height);
                var rightPos = new Vector3(-(float)Math.Cos(angle) * Radius, -(float)Math.Sin(angle) * Radius, i * Height);

                var left = new Line3dEx()
                {
                    SpritePathBody = "sb/w.png",
                    SpritePathEdge = "sb/e.png",
                    SpritePathCap = "sb/c.png",
                    EnableEndCap = false,
                    EnableStartCap = true,
                    UseDistanceFade = false,
                };
                left.ConfigureGenerators(g =>
                {
                    g.PositionDecimals = 1;
                    g.PositionTolerance = 1.5;
                    g.RotationDecimals = 1;
                    g.ScaleDecimals = 2;
                });
                left.StartPosition.Add(startTime, prevLeftPos);
                left.EndPosition.Add(startTime, leftPos);
                left.Thickness.Add(startTime, Thickness);
                grid.Add(left);

                var right = new Line3dEx()
                {
                    SpritePathBody = "sb/w.png",
                    SpritePathEdge = "sb/e.png",
                    SpritePathCap = "sb/c.png",
                    EnableEndCap = false,
                    EnableStartCap = true,
                    UseDistanceFade = false,
                };
                right.ConfigureGenerators(g =>
                {
                    g.PositionDecimals = 0;
                    g.PositionTolerance = 1.5;
                    g.RotationDecimals = 1;
                    g.ScaleDecimals = 2;
                });
                right.StartPosition.Add(startTime, prevRightPos);
                right.EndPosition.Add(startTime, rightPos);
                right.Thickness.Add(startTime, Thickness);
                grid.Add(right);

                var across = new Line3dEx()
                {
                    SpritePathBody = "sb/w.png",
                    InheritsColor = false,
                    UseDistanceFade = false,
                };
                across.ConfigureGenerators(g =>
                {
                    g.PositionDecimals = 1;
                    g.PositionTolerance = 1.5;
                    g.RotationDecimals = 1;
                    g.ScaleDecimals = 1;
                });
                across.StartPosition.Add(startTime, leftPos);
                across.EndPosition.Add(startTime, rightPos);
                across.Thickness.Add(startTime, Thickness * 0.8f);
                across.Coloring.Add(startTime, Color == Color4.White ? lightBlue : (CommandColor)Color);
                grid.Add(across);

                prevLeftPos = leftPos;
                prevRightPos = rightPos;
            }

            return scene;
        }
    }
}
