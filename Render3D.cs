//   _    __               _                ____   ____   ___        
//  | |  / /__  __________(_)___  ____     / __ \ / __ \ <  /  ____ _
//  | | / / _ \/ ___/ ___/ / __ \/ __ \   / / / // / / / / /  / __ `/
//  | |/ /  __/ /  (__  ) / /_/ / / / /  / /_/ // /_/ / / /  / /_/ / 
//  |___/\___/_/  /____/_/\____/_/ /_/   \____(_)____(_)_/   \__,_/  
using OpenTK;
using OpenTK.Graphics;
using System.IO;
using System.Globalization;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using System;
using System.Linq;
using System.Collections.Generic;

namespace StorybrewScripts
{
    public class Render3D : StoryboardObjectGenerator
    {
        [Configurable] public static string Sprite = "sb\\t.png";
        [Configurable] public static Color4 LightColor = Color4.White;
        [Configurable] public static Color4 ShadowColor = Color4.Red;
        [Configurable] public static Vector3 LightPosition = new Vector3(0, 0, -1);
        [Configurable] public float Multiplier = 1f;
        [Configurable] public static bool ColorByDepth = true;
        [Configurable] public static string ModelFileName = "cube.obj";
        [Configurable] public static bool GenerateRandom = false;
        [Configurable] public static int StartTime = 0;
        [Configurable] public static int EndTime = 10000;
        [Configurable] public static Vector3 ModelPosition;
        [Configurable] public static Vector3 ModelRotation;
        [Configurable] public static int RotationTime = 3000;
        [Configurable] public static int FPS = 30;
        [Configurable] public static int FOV = 90;
        [Configurable] public static Vector3 CameraPosition = new Vector3(0, 0, -1);
        [Configurable] public static Vector3 CameraRotation;

        //[Configurable] you can configure plans only in code
        public void AddPlans()
        {
            //Manual Model and Camera Control after adding any of those, loops won't be used and some configurables will be ignored
            //Template: new Plan(object, property, Time of start, Time of End, Starting Position in XYZ axis and Rotation in degrees, End Position in XYZ axis and Rotation in degrees, easing: "InSine" "OutSine" "InOutSine" you can add more just modify Easing function )
            /////////////////////////////////////////////////////////////////////////
            //Plans.Add(new Plan("Camera", "Position", StartTime, StartTime + 1000, new Vector3(0, 0, -2), new Vector3(-2, 0, -3), "InOutSine"));
            //Plans.Add(new Plan("Camera", "Rotation", StartTime, StartTime + 1000, new Vector3(0, 0, 0), new Vector3(0, -45, 0), "InSine"));
            //Plans.Add(new Plan("Model", "Position", StartTime, StartTime + 1000, new Vector3(0, 0, 0), new Vector3(0, 0, 0), "InOutSine"));
            //Plans.Add(new Plan("Model", "Rotation", StartTime, StartTime + 1000, new Vector3(0, 0, 0), new Vector3(0, 180, 0), "InSine"));
            /////////////////////////////////////////////////////////////////////////
        }


        #region Code
        protected override void Generate()
        {
            AddPlans();
            if (GenerateRandom) MakeObject();
            else ReadObject();
            Initialize();
        }
        public List<Plan> Plans = new List<Plan>();
        public void ReadObject()
        {
            string line;
            StreamReader file = new StreamReader(OpenProjectFile(ModelFileName));
            float empty = 0f;
            while ((line = file.ReadLine()) != null)
            {
                if (line.StartsWith("v "))
                {
                    var i = 0;
                    string[] parts = line.Split(new[] { ' ' });
                    while (float.TryParse(parts[i], out empty) == false) i++;
                    var x = float.Parse(parts[i], CultureInfo.InvariantCulture);
                    var y = float.Parse(parts[i + 1], CultureInfo.InvariantCulture);
                    var z = float.Parse(parts[i + 2], CultureInfo.InvariantCulture);
                    Points.Add(new Vector3(x, y, z));
                }
            }
        }
        public static float Span;
        public static Vector3 RelativeCameraPosition = new Vector3(0, 0, 0); // so currently polygons are being added camera position for proper sorting so relative camera postion is 0,0,0 lol why I've done it that way?
        public float FinalTime;
        public float previousFrameTime;
        public void Initialize()
        {
            Span = (EndTime - StartTime > RotationTime && (!AnyPlanCheck())) ? StartTime + RotationTime : EndTime;
            for (float i = StartTime; i <= Span; i += 1000f / FPS) FinalTime = i;
            for (float i = StartTime; i <= Span; i += 1000f / FPS)
            {
                SpriteNumber = 0;
                float Rot = (!AnyPlanCheck()) ? ((i - StartTime) / (FinalTime - StartTime)) : 0;
                var CurrentModelRotation = (Plans.Exists(x => x.Object == "Model" && x.Property == "Rotation") == false) ? new Vector3(ModelRotation.X, ModelRotation.Y + Rot * 360, ModelRotation.Z): GetProperty("Model", "Rotation", (int)i);
                var CurrentModelPosition = (Plans.Exists(x => x.Object == "Model" && x.Property == "Position") == false) ? ModelPosition : GetProperty("Model", "Position", (int)i);
                var CurrentCameraRotation = (Plans.Exists(x => x.Object == "Camera" && x.Property == "Rotation") == false) ? CameraRotation : GetProperty("Camera", "Rotation", (int)i);
                var CurrentCameraPosition = (Plans.Exists(x => x.Object == "Camera" && x.Property == "Position") == false) ? CameraPosition : GetProperty("Camera", "Position", (int)i);
                List<Polygon> RotatedPolygons = Polygons.Select(poly => { poly = RotatePoly(RotatePoly(poly, CurrentModelRotation) + CurrentModelPosition - CurrentCameraPosition, CurrentCameraRotation); return poly; }).ToList();
                for (var v = 0; v < RotatedPolygons.Count; v++)
                {
                    var Polygon = RotatedPolygons[v];
                    Polygon.Normal = CheckNormal(Polygon);
                    Polygon.Distance = Polygon.MinDistance();
                    List<Polygon> ClippedPolygonsZ = ClippingZ(Polygon);
                    List<string> Distances = new List<string>();
                    foreach (Polygon ClipZ in ClippedPolygonsZ) Clipping(ApplyPerspective(ClipZ));
                }
                List<Polygon> VisiblePolygons = new List<Polygon>();
                for (var x = 0; x < ClippedPolygons.Count; x++)
                {
                    if (ClippedPolygons[x].Normal.X * (ClippedPolygons[x].v1.X) +
                        ClippedPolygons[x].Normal.Y * (ClippedPolygons[x].v1.Y) +
                        ClippedPolygons[x].Normal.Z * (ClippedPolygons[x].v1.Z) < 0.0f)
                        VisiblePolygons.Add(ClippedPolygons[x]);
                }
                List<Polygon> SortedPolygons = VisiblePolygons.OrderByDescending(poly => poly.Distance).ToList();

                foreach (Polygon Polygon in SortedPolygons) RenderPolygon(Polygon, i);
                ClearUnusedSprites(i, VisiblePolygons.Count);
                ClippedPolygons.Clear();
                previousFrameTime = i;
            }
            if (EndTime - StartTime > RotationTime && (!AnyPlanCheck())) EndLoops(StartTime + FinalTime);
        }
        public List<Polygon> ClippedPolygons = new List<Polygon>();
        public List<Polygon> ClippingZ(Polygon polygon)
        {
            List<Polygon> ClippedPolygonsZ = new List<Polygon>();
            List<Vector3> InFront = new List<Vector3>();
            List<Vector3> InBack = new List<Vector3>();
            List<Vector3> NewPos = new List<Vector3>();

            if (polygon.v1.Z <= RelativeCameraPosition.Z) InBack.Add(polygon.v1); else InFront.Add(polygon.v1);
            if (polygon.v2.Z <= RelativeCameraPosition.Z) InBack.Add(polygon.v2); else InFront.Add(polygon.v2);
            if (polygon.v3.Z <= RelativeCameraPosition.Z) InBack.Add(polygon.v3); else InFront.Add(polygon.v3);

            if (InFront.Count == 3) ClippedPolygonsZ.Add(polygon);
            else if (InFront.Count == 2)
            {
                foreach (Vector3 vec in InFront)
                {
                    var t = Math.Abs(vec.Z - RelativeCameraPosition.Z) / Math.Abs(vec.Z - InBack[0].Z);
                    NewPos.Add(Vector3.Lerp(vec, InBack[0], t));
                }
                var Poly1 = new Polygon(InFront[0], NewPos[0], NewPos[1]);
                Poly1.Normal = polygon.Normal;
                Poly1.Distance = polygon.Distance;
                ClippedPolygonsZ.Add(Poly1);

                var Poly2 = new Polygon(InFront[0], InFront[1], NewPos[1]);
                Poly2.Normal = polygon.Normal;
                Poly2.Distance = polygon.Distance;
                ClippedPolygonsZ.Add(Poly2);
            }
            else if (InFront.Count == 1)
            {
                foreach (Vector3 vec in InBack)
                {
                    var t = Math.Abs(InFront[0].Z - RelativeCameraPosition.Z) / Math.Abs(vec.Z - InFront[0].Z);
                    NewPos.Add(Vector3.Lerp(InFront[0], vec, t));
                }
                var Poly = new Polygon(InFront[0], NewPos[0], NewPos[1]);
                Poly.Normal = polygon.Normal;
                Poly.Distance = polygon.Distance;
                ClippedPolygonsZ.Add(Poly);
            }
            return (ClippedPolygonsZ);
        }
        public void Clipping(Polygon polygon)
        {
            List<Polygon> ClippedPolygonsLeft = new List<Polygon>();
            List<Polygon> ClippedPolygonsRight = new List<Polygon>();
            List<Polygon> ClippedPolygonsTop = new List<Polygon>();
            List<Polygon> ClippedPolygonsBottom = new List<Polygon>();
            if (!float.IsNaN(polygon.v1.X))
            {
                float X = -427f;
                List<Vector3> InFront = new List<Vector3>();
                List<Vector3> InBack = new List<Vector3>();
                List<Vector3> NewPos = new List<Vector3>();

                if (polygon.v1.X < X) InBack.Add(polygon.v1); else InFront.Add(polygon.v1);
                if (polygon.v2.X < X) InBack.Add(polygon.v2); else InFront.Add(polygon.v2);
                if (polygon.v3.X < X) InBack.Add(polygon.v3); else InFront.Add(polygon.v3);

                if (InFront.Count == 3) ClippedPolygonsLeft.Add(polygon);
                else if (InFront.Count == 2)
                {
                    foreach (Vector3 vec in InFront)
                    {
                        var t = Math.Abs(vec.X - X) / Math.Abs(vec.X - InBack[0].X);
                        NewPos.Add(Vector3.Lerp(vec, InBack[0], t));
                    }
                    var Poly1 = new Polygon(InFront[0], NewPos[0], NewPos[1]);
                    Poly1.Normal = polygon.Normal;
                    Poly1.Distance = polygon.Distance;
                    ClippedPolygonsLeft.Add(Poly1);

                    var Poly2 = new Polygon(InFront[0], InFront[1], NewPos[1]);
                    Poly2.Normal = polygon.Normal;
                    Poly2.Distance = polygon.Distance;
                    ClippedPolygonsLeft.Add(Poly2);
                }
                else if (InFront.Count == 1)
                {
                    foreach (Vector3 vec in InBack)
                    {
                        var t = Math.Abs(InFront[0].X - X) / Math.Abs(vec.X - InFront[0].X);
                        NewPos.Add(Vector3.Lerp(InFront[0], vec, t));
                    }
                    var Poly = new Polygon(InFront[0], NewPos[0], NewPos[1]);
                    Poly.Normal = polygon.Normal;
                    Poly.Distance = polygon.Distance;
                    ClippedPolygonsLeft.Add(Poly);
                }
            }
            foreach (Polygon Leftpoly in ClippedPolygonsLeft)
            {
                float X = 424f;
                List<Vector3> In = new List<Vector3>();
                List<Vector3> Out = new List<Vector3>();
                List<Vector3> New = new List<Vector3>();
                if (Leftpoly.v1.X > X) { Out.Add(Leftpoly.v1); } else { In.Add(Leftpoly.v1); }
                if (Leftpoly.v2.X > X) { Out.Add(Leftpoly.v2); } else { In.Add(Leftpoly.v2); }
                if (Leftpoly.v3.X > X) { Out.Add(Leftpoly.v3); } else { In.Add(Leftpoly.v3); }
                if (In.Count == 3) ClippedPolygonsRight.Add(Leftpoly);
                else if (In.Count == 2)
                {
                    foreach (Vector3 vec in In)
                    {
                        var t = Math.Abs(vec.X - X) / Math.Abs(vec.X - Out[0].X);
                        New.Add(Vector3.Lerp(vec, Out[0], t));
                    }

                    var Poly1 = new Polygon(In[0], New[0], New[1]);
                    Poly1.Normal = polygon.Normal;
                    Poly1.Distance = polygon.Distance;
                    ClippedPolygonsRight.Add(Poly1);

                    var Poly2 = new Polygon(In[0], In[1], New[1]);
                    Poly2.Normal = polygon.Normal;
                    Poly2.Distance = polygon.Distance;
                    ClippedPolygonsRight.Add(Poly2);
                }
                else if (In.Count == 1)
                {
                    foreach (Vector3 vec in Out)
                    {
                        var t = Math.Abs(In[0].X - X) / Math.Abs(vec.X - In[0].X);
                        New.Add(Vector3.Lerp(In[0], vec, t));
                    }
                    var Poly = new Polygon(In[0], New[0], New[1]);
                    Poly.Normal = polygon.Normal;
                    Poly.Distance = polygon.Distance;
                    ClippedPolygonsRight.Add(Poly);
                }
            }
            foreach (Polygon Rightpoly in ClippedPolygonsRight)
            {
                float Y = -240;
                List<Vector3> In = new List<Vector3>();
                List<Vector3> Out = new List<Vector3>();
                List<Vector3> New = new List<Vector3>();

                if (Rightpoly.v1.Y < Y) Out.Add(Rightpoly.v1); else In.Add(Rightpoly.v1);
                if (Rightpoly.v2.Y < Y) Out.Add(Rightpoly.v2); else In.Add(Rightpoly.v2);
                if (Rightpoly.v3.Y < Y) Out.Add(Rightpoly.v3); else In.Add(Rightpoly.v3);

                if (In.Count == 3) ClippedPolygonsBottom.Add(Rightpoly);
                else if (In.Count == 2)
                {
                    foreach (Vector3 vec in In)
                    {
                        var t = Math.Abs(vec.Y - Y) / Math.Abs(vec.Y - Out[0].Y);
                        New.Add(Vector3.Lerp(vec, Out[0], t));
                    }

                    var Poly1 = new Polygon(In[0], New[0], New[1]);
                    Poly1.Normal = polygon.Normal;
                    Poly1.Distance = polygon.Distance;
                    ClippedPolygonsBottom.Add(Poly1);

                    var Poly2 = new Polygon(In[0], In[1], New[1]);
                    Poly2.Normal = polygon.Normal;
                    Poly2.Distance = polygon.Distance;
                    ClippedPolygonsBottom.Add(Poly2);
                }
                else if (In.Count == 1)
                {
                    foreach (Vector3 vec in Out)
                    {
                        var t = Math.Abs(In[0].Y - Y) / Math.Abs(vec.Y - In[0].Y);
                        New.Add(Vector3.Lerp(In[0], vec, t));
                    }
                    var Poly = new Polygon(In[0], New[0], New[1]);
                    Poly.Normal = polygon.Normal;
                    Poly.Distance = polygon.Distance;
                    ClippedPolygonsBottom.Add(Poly);
                }
            }
            foreach (Polygon Bottompoly in ClippedPolygonsBottom)
            {
                float Y = 240f;
                List<Vector3> In = new List<Vector3>();
                List<Vector3> Out = new List<Vector3>();
                List<Vector3> New = new List<Vector3>();
                if (Bottompoly.v1.Y > Y) { Out.Add(Bottompoly.v1); } else { In.Add(Bottompoly.v1); }
                if (Bottompoly.v2.Y > Y) { Out.Add(Bottompoly.v2); } else { In.Add(Bottompoly.v2); }
                if (Bottompoly.v3.Y > Y) { Out.Add(Bottompoly.v3); } else { In.Add(Bottompoly.v3); }
                if (In.Count == 3) ClippedPolygons.Add(Bottompoly);
                else if (In.Count == 2)
                {
                    foreach (Vector3 vec in In)
                    {
                        var t = Math.Abs(vec.Y - Y) / Math.Abs(vec.Y - Out[0].Y);
                        New.Add(Vector3.Lerp(vec, Out[0], t));
                    }
                    var Poly1 = new Polygon(In[0], New[0], New[1]);
                    Poly1.Normal = polygon.Normal;
                    Poly1.Distance = polygon.Distance;
                    ClippedPolygons.Add(Poly1);
                    var Poly2 = new Polygon(In[0], In[1], New[1]);
                    Poly2.Normal = polygon.Normal;
                    Poly2.Distance = polygon.Distance;
                    ClippedPolygons.Add(Poly2);
                }
                else if (In.Count == 1)
                {
                    foreach (Vector3 vec in Out)
                    {
                        var t = Math.Abs(In[0].Y - Y) / Math.Abs(vec.Y - In[0].Y);
                        New.Add(Vector3.Lerp(In[0], vec, t));
                    }
                    var Poly = new Polygon(In[0], New[0], New[1]);
                    Poly.Normal = polygon.Normal;
                    Poly.Distance = polygon.Distance;
                    ClippedPolygons.Add(Poly);
                }
            }
        }
        public float prevTime = float.NaN;
        public int SpriteNumber = 0;
        public int SpriteCount = -1;
        public bool NewSprite = false;
        public List<OsbSprite> Sprites = new List<OsbSprite>();
        public OsbSprite GetSprite(float time)
        {
            int Add = 0;
            if ((EndTime - StartTime) % RotationTime > 0) Add = 1;
            if (time == prevTime) SpriteNumber++;
            prevTime = time;
            if (SpriteCount < SpriteNumber)
            {
                SpriteCount = SpriteNumber;
                Sprites.Add(GetLayer("").CreateSprite(Sprite, OsbOrigin.BottomLeft));
                if (!ColorByDepth) Sprites[SpriteNumber].Color(time, FinalTime, LightColor, LightColor);
                if (!AnyPlanCheck())
                {
                    if (time == StartTime) Sprites[SpriteNumber].Fade(StartTime, EndTime, 1, 1);
                    if (EndTime - StartTime > RotationTime) Sprites[SpriteNumber].StartLoopGroup(0, ((EndTime - StartTime) / RotationTime) + Add);
                    if (time != StartTime)
                    {
                        Sprites[SpriteNumber].Fade(StartTime, time - 1, 0, 0);
                        Sprites[SpriteNumber].Fade(time, FinalTime, 1, 1);
                    }
                }
                else
                {
                    Sprites[SpriteNumber].Fade(time, EndTime, 1, 1);
                }
                NewSprite = true;
                return Sprites[SpriteNumber];
            }
            else
            {
                NewSprite = false;
                return Sprites[SpriteNumber];
            }
        }
        public void RenderPolygon(Polygon polygon, float time)
        {
            var MinDist = polygon.MaxDistance();
            var bitmap = GetMapsetBitmap(Sprite);

            var Center = new Vector2(320, 240);

            Polygon Poly = HeightBaseCross(polygon);

            if (Get2dDistance(new Vector2(Poly.Middle.X, -Poly.Middle.Y) + Center, new Vector2(Poly.v3.X, -Poly.v3.Y) + Center) < 0.0001f || Get2dDistance(new Vector2(Poly.Middle.X, -Poly.Middle.Y) + Center, new Vector2(Poly.v1.X, -Poly.v1.Y) + Center) < 0.0001f)
            {
                Poly = RotatePoly(Poly, new Vector3(0, 0.01f, 0));
            }

            Vector2 Middle = new Vector2(Poly.Middle.X, -Poly.Middle.Y) + Center;

            var v12 = new Vector2(Poly.v1.X, -Poly.v1.Y) + Center;
            var v22 = new Vector2(Poly.v2.X, -Poly.v2.Y) + Center;
            var v32 = new Vector2(Poly.v3.X, -Poly.v3.Y) + Center;

            var First = Rotate(new Vector3(Get2dDistance(v12, Middle), -Get2dDistance(Middle, v32), 0), new Vector3(0, 0, (float)MathHelper.RadiansToDegrees(Look(v12, Middle)))) + new Vector3(Poly.v1.X, -Poly.v1.Y, 0);
            var Second = Rotate(new Vector3(Get2dDistance(v22, Middle), -Get2dDistance(Middle, v32), 0), new Vector3(0, 0, (float)MathHelper.RadiansToDegrees(Look(v22, Middle)))) + new Vector3(Poly.v2.X, -Poly.v2.Y, 0);

            var First2d = new Vector2(First.X, First.Y);
            var Second2d = new Vector2(Second.X, Second.Y);

            Vector2 Scale1;
            Vector2 Scale2;
            double Rot1;
            double Rot2;
            if (Get2dDistance(Second2d + Center, v32) > Get2dDistance(First2d + Center, v32))
            {
                var a = v22;
                v22 = v12;
                v12 = a;
            }

            Scale1 = new Vector2(Get2dDistance(v22, Middle) / bitmap.Width, Get2dDistance(Middle, v32) / bitmap.Height);
            Scale2 = new Vector2(Get2dDistance(Middle, v32) / bitmap.Width, Get2dDistance(Middle, v12) / bitmap.Height);

            Rot1 = Look(v22, Middle);
            Rot2 = Look(v32, Middle);

            var Triangle1 = GetSprite(time);
            var Tr1 = NewSprite;
            if (!AnyPlanCheck() || NewSprite || Triangle1.ScaleAt(previousFrameTime) != (StorybrewCommon.Storyboarding.CommandValues.CommandScale)Scale1) Triangle1.ScaleVec(time, Scale1);
            if (!AnyPlanCheck() || NewSprite || Triangle1.PositionAt(previousFrameTime) != (StorybrewCommon.Storyboarding.CommandValues.CommandPosition)v22) Triangle1.Move(time, v22);
            if (!AnyPlanCheck() || NewSprite || Triangle1.RotationAt(previousFrameTime) != (StorybrewCommon.Storyboarding.CommandValues.CommandDecimal)Rot1) Triangle1.Rotate(time, Rot1);

            var Triangle2 = GetSprite(time);

            if (!AnyPlanCheck() || NewSprite || Triangle2.ScaleAt(previousFrameTime) != (StorybrewCommon.Storyboarding.CommandValues.CommandScale)Scale2) Triangle2.ScaleVec(time, Scale2);
            if (!AnyPlanCheck() || NewSprite || Triangle2.PositionAt(previousFrameTime) != (StorybrewCommon.Storyboarding.CommandValues.CommandPosition)v32) Triangle2.Move(time, v32);
            if (!AnyPlanCheck() || NewSprite || Triangle2.RotationAt(previousFrameTime) != (StorybrewCommon.Storyboarding.CommandValues.CommandDecimal)Rot2) Triangle2.Rotate(time, Rot2);
            if (ColorByDepth)
            {
                Vector3 Light = LightPosition;
                if (Light.Length != 0f)
                    Light = (Light / Light.Length);
                else
                    Light = new Vector3(1, 1, 1);
                float DP = polygon.Normal.X * Light.X + polygon.Normal.Y * Light.Y + polygon.Normal.Z * Light.Z;
                if (!float.IsNaN(DP))
                {
                    var Color = (StorybrewCommon.Storyboarding.CommandValues.CommandColor)new Color4(ShadowColor.R + DP * (LightColor.R - ShadowColor.R), ShadowColor.G + DP * (LightColor.G - ShadowColor.G), ShadowColor.B + DP * (LightColor.B - ShadowColor.B), 1);
                    if (!AnyPlanCheck() || Tr1 || Triangle1.ColorAt(previousFrameTime) != Color) Triangle1.Color(time, Color);
                    if (!AnyPlanCheck() || NewSprite || Triangle2.ColorAt(previousFrameTime) != Color) Triangle2.Color(time, Color);
                }
            }
        }
        public void ClearUnusedSprites(float time, int PolygonCount)
        {
            var w = 1;
            if (PolygonCount == 0) w = 0;
            for (var i = SpriteNumber + w; i <= SpriteCount; i++)
            {
                if ((Vector2)Sprites[i].ScaleAt(previousFrameTime) != Vector2.Zero)
                {
                    Sprites[i].ScaleVec(time, 0, 0);
                }
            }
        }
        public void EndLoops(float time)
        {
            for (var z = 0; z <= SpriteCount; z++)
            {
                Sprites[z].EndGroup();
            }
        }
        public bool AnyPlanCheck()
        {
            if (Plans.Count == 0) return false;
            else return true;
        }
        public Vector3 GetProperty(string objects, string property, int time)
        {
            if (Plans.Exists(x => x.Object == objects && x.Property == property && time >= x.StartTime && time < x.EndTime))
            {
                var CurrentPlan = Plans.FirstOrDefault(x => x.Object == objects && x.Property == property && time >= x.StartTime && time < x.EndTime);
                return Vector3.Lerp(CurrentPlan.StartPosition, CurrentPlan.EndPosition, Easing((float)(time - CurrentPlan.StartTime) / (CurrentPlan.EndTime - CurrentPlan.StartTime), CurrentPlan.Easing));
            }
            else
            {
                if (Plans.Exists(x => x.Object == objects && x.Property == property && time > x.StartTime))
                {
                    var CurrentPlan = Plans.Where(x => x.Object == objects && x.Property == property).OrderBy(x => x.StartTime).LastOrDefault(x => time > x.StartTime);
                    return CurrentPlan.EndPosition;
                }
                else
                {
                    var CurrentPlan = Plans.Where(x => x.Object == objects && x.Property == property).OrderBy(x => x.StartTime).FirstOrDefault(x => time < x.StartTime);
                    return CurrentPlan.StartPosition;
                }
            }
        }
        //Not needed on per frame rendering
        /*public double CheckRot(double OldRot, double NewRot)
        {
            if (Math.Abs(NewRot - OldRot) > MathHelper.DegreesToRadians(180) && OldRot < NewRot) return NewRot - MathHelper.DegreesToRadians(360);
            else if (Math.Abs(NewRot - OldRot) > MathHelper.DegreesToRadians(180) && OldRot > NewRot) return MathHelper.DegreesToRadians(360) + NewRot;
            else if (Math.Abs(NewRot - OldRot) <= MathHelper.DegreesToRadians(180)) return NewRot;
            else throw new ArgumentException("Rotation Can't be determined");
        }*/
        public Vector3 ApplyPerspective(Vector3 Vector)
        {
            if (Math.Abs(RelativeCameraPosition.Z - Vector.Z) != 0)
            {
                Vector *= 480 / (2 * (float)Math.Tan(MathHelper.DegreesToRadians(FOV / 2)) * Math.Abs(RelativeCameraPosition.Z - Vector.Z));
                return Vector;
            }
            else
            {
                Vector *= 480 / (2 * (float)Math.Tan(MathHelper.DegreesToRadians(FOV / 2)) * 0.0001f);
                return Vector;
            }
        }
        public Polygon ApplyPerspective(Polygon Polygon)
        {
            Polygon.v1 = ApplyPerspective(Polygon.v1);
            Polygon.v2 = ApplyPerspective(Polygon.v2);
            Polygon.v3 = ApplyPerspective(Polygon.v3);
            return Polygon;
        }
        public List<Vector3> Points = new List<Vector3>();
        public float[,] Properties;

        public Vector3 CheckNormal(Polygon poly)
        {
            Vector3 Line1, Line2, Normal;
            Line1 = poly.v2 - poly.v1;
            Line2 = poly.v3 - poly.v1;
            Normal.X = Line1.Y * Line2.Z - Line1.Z * Line2.Y;
            Normal.Y = Line1.Z * Line2.X - Line1.X * Line2.Z;
            Normal.Z = Line1.X * Line2.Y - Line1.Y * Line2.X;
            Normal = Normal / Normal.Length;
            return Normal;
        }
        public struct Plan
        {
            public string _Object;
            public string _Property;
            public Vector3 _StartPosition;
            public Vector3 _EndPosition;
            public int _StartTime;
            public int _EndTime;
            public string _Easing;
            public Plan(string Object, string Property, int StartTime, int EndTime, Vector3 StartPosition, Vector3 EndPosition, string Easing)
            {
                this._Object = Object;
                this._Property = Property;
                this._StartPosition = StartPosition;
                this._EndPosition = EndPosition;
                this._StartTime = StartTime;
                this._EndTime = EndTime;
                this._Easing = Easing;
            }
            public string Object
            {
                get { return _Object; }
                set { _Object = value; }
            }
            public string Property
            {
                get { return _Property; }
                set { _Property = value; }
            }
            public Vector3 StartPosition
            {
                get { return _StartPosition; }
                set { _StartPosition = value; }
            }
            public Vector3 EndPosition
            {
                get { return _EndPosition; }
                set { _EndPosition = value; }
            }
            public int StartTime
            {
                get { return _StartTime; }
                set { _StartTime = value; }
            }
            public int EndTime
            {
                get { return _EndTime; }
                set { _EndTime = value; }
            }
            public string Easing
            {
                get { return _Easing; }
                set { _Easing = value; }
            }
        }

        public struct Polygon
        {
            public Vector3 _v1, _v2, _v3;
            public Vector2 Middle;
            public Vector3 _Normal;
            public float _Dist;
            public Polygon(Vector3 v1, Vector3 v2, Vector3 v3)
            {
                this._v1 = v1;
                this._v2 = v2;
                this._v3 = v3;
                this._Normal = new Vector3();
                Middle = new Vector2(float.NaN, float.NaN);
                this._Dist = -1;
            }
            public Vector3 v1
            {
                get { return _v1; }
                set { _v1 = value; }
            }
            public Vector3 v2
            {
                get { return _v2; }
                set { _v2 = value; }
            }
            public Vector3 v3
            {
                get { return _v3; }
                set { _v3 = value; }
            }
            public Vector3 Normal
            {
                get { return _Normal; }
                set { _Normal = value; }
            }
            public float Distance
            {
                get { return _Dist; }
                set { _Dist = value; }
            }
            public override string ToString()
            {
                return _v1.ToString() + _v2.ToString() + _v3.ToString();
            }
            public float MaxDistance()
            {
                return Math.Max(Math.Max(Distance(v1, v2, RelativeCameraPosition), Distance(v2, v3, RelativeCameraPosition)), Distance(v1, v3, RelativeCameraPosition));
            }
            public float MinDistance()
            {
                return Math.Min(Math.Min(Distance(v1, v2, RelativeCameraPosition), Distance(v2, v3, RelativeCameraPosition)), Distance(v1, v3, RelativeCameraPosition));
            }
            public float MidPointDistance()
            {
                return MaxDistance() + MinDistance() / 2f;
            }
            public static Polygon operator +(Polygon poly, Vector3 Vector)
            {
                poly.v1 += Vector;
                poly.v2 += Vector;
                poly.v3 += Vector;
                return poly;
            }
            public static Polygon operator -(Polygon poly, Vector3 Vector)
            {
                poly.v1 -= Vector;
                poly.v2 -= Vector;
                poly.v3 -= Vector;
                return poly;
            }
        }
        public List<Polygon> Polygons = new List<Polygon>();
        private static float Get3dDistance(Vector3 First, Vector3 Second)
        {
            return (float)Math.Sqrt(Math.Pow((Second.X - First.X), 2) + Math.Pow((Second.Y - First.Y), 2) + Math.Pow((Second.Z - First.Z), 2));
        }
        private static float Get2dDistance(Vector2 First, Vector2 Second)
        {
            return (float)Math.Sqrt(Math.Pow((Second.X - First.X), 2) + Math.Pow((Second.Y - First.Y), 2));
        }
        public static double Distance(Vector2 Vector)
        {
            return Math.Sqrt(Math.Pow(Vector.X, 2) + Math.Pow(Vector.Y, 2));
        }
        public static float Distance(Vector3 StartVec, Vector3 EndVec, Vector3 Point)
        {
            var ClosestPoint = GetClosestPointOnFiniteLine(StartVec, EndVec, Point);
            return Get3dDistance(ClosestPoint, Point);
        }
        public static Vector3 GetClosestPointOnFiniteLine(Vector3 StartVec, Vector3 EndVec, Vector3 Point)
        {
            Vector3 line_direction = EndVec - StartVec;
            float line_length = line_direction.Length;
            line_direction.Normalize();
            float project_length = MathHelper.Clamp(Vector3.Dot(Point - StartVec, line_direction), 0f, line_length);
            return StartVec + line_direction * project_length;
        }
        public static double Multiply(Vector2 Vector, Vector2 Vector2)
        {
            return Vector.X * Vector2.X + Vector.Y * Vector2.Y;
        }
        public static double Look(Vector2 Vector, Vector2 Vector2)
        {
            if (Vector == Vector2)
                return 0;
            var First = Vector.X > Vector2.X ? new Vector2(100f, 0f) : new Vector2(-100f, 0f);
            var Second = new Vector2(-Math.Abs(Vector2.X - Vector.X), -Math.Abs(Vector2.Y - Vector.Y));
            float angle = (float)Math.Acos(Multiply(First, Second) / (Distance(First) * Distance(Second)));
            return Vector.Y < Vector2.Y ? angle : -angle;
        }
        public Vector3 Rotate(Vector3 Vector, Vector3 Rotation)
        {
            var Rot = new Quaternion(MathHelper.DegreesToRadians(Rotation.Z), MathHelper.DegreesToRadians(Rotation.Y), MathHelper.DegreesToRadians(Rotation.X));
            return Vector3.Transform(Vector, Rot);
        }

        public Polygon RotatePoly(Polygon poly, Vector3 Rotation)
        {
            var Polygon = poly;
            Polygon.v1 = Rotate(poly.v1, Rotation);
            Polygon.v2 = Rotate(poly.v2, Rotation);
            Polygon.v3 = Rotate(poly.v3, Rotation);
            return Polygon;
        }
        //below is the worst way to find where height intersects with base need to find better one but well this one works!!!!!
        public Polygon HeightBaseCross(Polygon polygon)
        {
            var v1 = new Vector2(polygon.v1.X, polygon.v1.Y);
            var v2 = new Vector2(polygon.v2.X, polygon.v2.Y);
            var v3 = new Vector2(polygon.v3.X, polygon.v3.Y);
            Polygon Poly = polygon;
            var maxds = Math.Max(Get2dDistance(v1, v2), Math.Max(Get2dDistance(v2, v3), Get2dDistance(v1, v3)));
            if (Get2dDistance(v2, v3) == maxds)
            {
                var f = v1;
                v1 = v2;
                v2 = v3;
                v3 = f;
                Poly = new Polygon(polygon.v2, polygon.v3, polygon.v1);
            }
            else if (Get2dDistance(v1, v3) == maxds)
            {
                var f = v2;
                v2 = v3;
                v3 = f;
                Poly = new Polygon(polygon.v1, polygon.v3, polygon.v2);
            }
            var Res = GetClosestPointOnFiniteLine(new Vector3(v1.X,v1.Y,0), new Vector3(v2.X,v2.Y,0),new Vector3(v3.X,v3.Y,0));
            Poly.Middle = new Vector2(Res.X, Res.Y);
            return (Poly);
        }
        public float Easing(float t, string easing)
        {
            switch (easing)
            {
                case "InSine":
                    return 1 - (float)Math.Cos((t * Math.PI) / 2);
                case "OutSine":
                    return (float)Math.Sin((t * Math.PI) / 2);
                case "InOutSine":
                    return -((float)Math.Cos(Math.PI * t) - 1) / 2;
                default:
                    return t;
            }
        }
        public void MakeObject()
        {
            for (int i = 0; i < 16; i += 1)
            {
                float Height = Random(1f, 200f);
                float width = Random(1f, 5f);
                var w = Random(-15, 15);
                var a = Random(-15, 15);
                var b = Random(-15, 15);
                var c = Random(-15, 15);
                var R = new Vector3(Random(0, 360), Random(0, 360), Random(0, 360));
                Polygons.Add(RotatePoly(new Polygon(new Vector3(width + (a * w), Height + (c * w), width + (b * w)), new Vector3(width + (a * w), -width + (c * w), width + (b * w)), new Vector3(width + (a * w), Height + (c * w), -width + (b * w))), R));
                Polygons.Add(RotatePoly(new Polygon(new Vector3(width + (a * w), -width + (c * w), width + (b * w)), new Vector3(width + (a * w), -width + (c * w), -width + (b * w)), new Vector3(width + (a * w), Height + (c * w), -width + (b * w))), R));
                Polygons.Add(RotatePoly(new Polygon(new Vector3(-width + (a * w), Height + (c * w), -width + (b * w)), new Vector3(-width + (a * w), -width + (c * w), -width + (b * w)), new Vector3(-width + (a * w), Height + (c * w), width + (b * w))), R));
                Polygons.Add(RotatePoly(new Polygon(new Vector3(-width + (a * w), -width + (c * w), -width + (b * w)), new Vector3(-width + (a * w), -width + (c * w), width + (b * w)), new Vector3(-width + (a * w), Height + (c * w), width + (b * w))), R));
                Polygons.Add(RotatePoly(new Polygon(new Vector3(-width + (a * w), Height + (c * w), -width + (b * w)), new Vector3(-width + (a * w), Height + (c * w), width + (b * w)), new Vector3(width + (a * w), Height + (c * w), -width + (b * w))), R));
                Polygons.Add(RotatePoly(new Polygon(new Vector3(-width + (a * w), Height + (c * w), width + (b * w)), new Vector3(width + (a * w), Height + (c * w), width + (b * w)), new Vector3(width + (a * w), Height + (c * w), -width + (b * w))), R));
                Polygons.Add(RotatePoly(new Polygon(new Vector3(-width + (a * w), -width + (c * w), width + (b * w)), new Vector3(-width + (a * w), -width + (c * w), -width + (b * w)), new Vector3(width + (a * w), -width + (c * w), width + (b * w))), R));
                Polygons.Add(RotatePoly(new Polygon(new Vector3(-width + (a * w), -width + (c * w), -width + (b * w)), new Vector3(width + (a * w), -width + (c * w), -width + (b * w)), new Vector3(width + (a * w), -width + (c * w), width + (b * w))), R));
                Polygons.Add(RotatePoly(new Polygon(new Vector3(-width + (a * w), Height + (c * w), width + (b * w)), new Vector3(-width + (a * w), -width + (c * w), width + (b * w)), new Vector3(width + (a * w), Height + (c * w), width + (b * w))), R));
                Polygons.Add(RotatePoly(new Polygon(new Vector3(-width + (a * w), -width + (c * w), width + (b * w)), new Vector3(width + (a * w), -width + (c * w), width + (b * w)), new Vector3(width + (a * w), Height + (c * w), width + (b * w))), R));
                Polygons.Add(RotatePoly(new Polygon(new Vector3(width + (a * w), Height + (c * w), -width + (b * w)), new Vector3(width + (a * w), -width + (c * w), -width + (b * w)), new Vector3(-width + (a * w), Height + (c * w), -width + (b * w))), R));
                Polygons.Add(RotatePoly(new Polygon(new Vector3(width + (a * w), -width + (c * w), -width + (b * w)), new Vector3(-width + (a * w), -width + (c * w), -width + (b * w)), new Vector3(-width + (a * w), Height + (c * w), -width + (b * w))), R));
            }
        }
    }
    #endregion
}
