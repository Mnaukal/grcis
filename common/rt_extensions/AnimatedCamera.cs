using System;
using System.Collections.Generic;
using System.Linq;
using MathSupport;
using OpenTK;
using System.IO;
using System.Text.RegularExpressions;

namespace Rendering
{
    public class AnimatedCamera : ICamera, ITimeDependent
    {
        private IAnimatableCamera animatableCamera;
        private Dictionary<string, Parameter> parameters;
        private List<Keyframe> keyframes = new List<Keyframe>();

        private static char[] colonSeparator = {':'};
        private static char[] commaSeparator = {','};

        public AnimatedCamera (IAnimatableCamera animatableCamera, string fileName)
        {
            this.animatableCamera = animatableCamera;
            parameters = animatableCamera.GetParams().ToDictionary(p => p.Name, p => p);
            ReadAndSaveCameraScript(fileName);
            Start = keyframes[0].Time;
            End = keyframes[keyframes.Count - 1].Time;
        }

        private AnimatedCamera (IAnimatableCamera animatableCamera)
        {
            this.animatableCamera = animatableCamera;
        }

        private void ReadAndSaveCameraScript (string fileName)
        {
            StreamReader sr;
            try
            {
                sr = new StreamReader(fileName);
            }
            catch (IOException)
            {
                throw new IOException("Please specify a file with camera script.");
            }

            try
            {
                var keyframeParams = new Dictionary<string, object>();
                double keyframeTime = 0;

                while (true)
                {
                    string line = sr.ReadLine();
                    if (line == null)
                        break;
                    line = Regex.Replace(line, @"\s+", string.Empty);
                    if (line.Length == 0 || line[0] == '#')
                        continue;

                    string[] tokens = line.Split(colonSeparator, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Length == 1) // start of new keyframe
                    {
                        if (keyframeParams.Count > 0)
                        {
                            if (keyframes.Count == 0) // adding first keyframe
                                CheckRequiredParams(keyframeParams);

                            keyframes.Add(new Keyframe(keyframeTime, CopyDictionary(keyframeParams)));
                        }
                        keyframeTime = double.Parse(tokens[0]);
                    }
                    else // parameters for current keyframe
                    {
                        if (!parameters.ContainsKey(tokens[0]))
                            throw new ArgumentException("Parameter '" + tokens[0] + "' in camera script wasn't any of the provided Params.");

                        Parameter p = parameters[tokens[0]];
                        object value = p.Parser(tokens[1]);
                        keyframeParams[p.Name] = value;
                    }
                }
                keyframes.Add(new Keyframe(keyframeTime, CopyDictionary(keyframeParams)));
                sr.Close();
            }
            catch (IOException)
            {
                throw new IOException("An error occurred while reading the script file.");
            }
        }

        private void CheckRequiredParams (Dictionary<string, object> keyframeParams)
        {
            foreach (Parameter param in parameters.Values)
                if (param.Required && !keyframeParams.ContainsKey(param.Name))
                    throw new ArgumentException("The value of '" + param.Name + "' parameter wasn't provided in the first key frame.");
        }



        private Dictionary<string, object> CopyDictionary (Dictionary<string, object> original)
        {
            var copy = new Dictionary<string, object>();
            foreach (string key in original.Keys)
                copy[key] = original[key];
            return copy;
        }

        public double AspectRatio { get => animatableCamera.AspectRatio; set => animatableCamera.AspectRatio = value; }
        public double Width { get => animatableCamera.Width; set => animatableCamera.Width = value; }
        public double Height { get => animatableCamera.Height; set => animatableCamera.Height = value; }
        public double Start { get; set; }
        public double End { get; set; }
        public double Time { get; set; }
        public object Clone ()
        {
            AnimatedCamera clonedCamera = new AnimatedCamera(this.animatableCamera);
            clonedCamera.Start = this.Start;
            clonedCamera.End = this.End;
            clonedCamera.Time = this.Time;
            return clonedCamera;
        }

        public bool GetRay (double x, double y, out Vector3d p0, out Vector3d p1)
        {
            int i = 0;
            for (; i < keyframes.Count; i++)
            {
                if (keyframes[i].Time > Time)
                    break;
            }
            i--;
            Keyframe previous = i >= 1 ? keyframes[i - 1] : keyframes[0];
            Keyframe current = keyframes[i];
            Keyframe next = i < keyframes.Count - 1 ?  keyframes[i+1] : keyframes[keyframes.Count - 1];
            Keyframe later = i < keyframes.Count - 2 ? keyframes[i + 2] : keyframes[keyframes.Count - 1];
            double t = (Time - current.Time) / (next.Time - current.Time);

            var cameraParams = new Dictionary<string, object>();

            foreach (string paramName in current.parameters.Keys)
            {
                Parameter param = parameters[paramName];
                cameraParams[param.Name] = param.Interpolator(
                    previous.parameters[param.Name],
                    current.parameters[param.Name],
                    next.parameters[param.Name],
                    later.parameters[param.Name],
                    t);
            }

            animatableCamera.ApplyParams(cameraParams);
            return animatableCamera.GetRay(x, y, out p0, out p1);
        }

        private class Keyframe
        {
            public readonly double Time;
            public readonly Dictionary<string, object> parameters;

            public Keyframe (double time, Dictionary<string, object> parameters)
            {
                Time = time;
                this.parameters = parameters;
            }
        }

        public class Parameter
        {
            public readonly Interpolator Interpolator;
            public readonly Parser Parser;
            public readonly string Name;
            public readonly bool Required;

            public Parameter (string name, Parser parser, Interpolator interpolator, bool required = false)
            {
                Interpolator = interpolator;
                Name = name;
                Required = required;
                Parser = parser;
            }
        }

        public delegate object Parser (string s);
        public delegate object Interpolator (object previous, object current, object next, object later, double t);

        public static class Parsers
        {
            /// <summary>
            /// Parses a Vector3d from a string in form 'A, B, C' where A,B,C are doubles
            /// </summary>
            public static object ParseVector3 (string s)
            {
                try
                {
                    string[] values = s.Split(commaSeparator);
                    return new Vector3d(double.Parse(values[0]), double.Parse(values[1]), double.Parse(values[2]));
                }
                catch
                {
                    throw new ArgumentException("Error while parsing Vector3: '" + s + "'.");
                }
            }

            public static object ParseDouble (string s)
            {
                try
                {
                    return double.Parse(s);
                }
                catch
                {
                    throw new ArgumentException("Error while parsing double: '" + s + "'.");
                }
            }
        }

        public static class Interpolators
        {
            private static Matrix4d Catmull_Rom_M = new Matrix4d(
              -1,  3, -3,  1,
               2, -5,  4, -1,
              -1,  0,  1,  0,
               0,  2,  0,  0);

            /// <summary>
            /// Interpolation using Catmull-Rom splines
            /// (slide 3Dmath 2019 / 42)
            /// </summary>
            /// <param name="t">number between 0 and 1</param>
            public static object Catmull_Rom (object previous, object current, object next, object later, double t)
            {
                if (previous is Vector4d)
                    return Catmull_Rom_Vector4((Vector4d)previous, (Vector4d)current, (Vector4d)next, (Vector4d)later, t);
                if (previous is Vector3d)
                    return Catmull_Rom_Vector3((Vector3d)previous, (Vector3d)current, (Vector3d)next, (Vector3d)later, t);
                if (previous is double)
                    return Catmull_Rom_Double((double)previous, (double)current, (double)next, (double)later, t);
                throw new ArgumentException("Type not supported: '" + previous.GetType() + "'.");
            }

            private static Vector4d Catmull_Rom_Vector4 (Vector4d previous, Vector4d current, Vector4d next, Vector4d later, double t)
            {
                Matrix4d G = new Matrix4d(previous, current, next, later);
                Vector4d T = new Vector4d(t*t*t, t*t, t, 1);
                return 0.5f * Vector4d.Transform(T, Catmull_Rom_M * G); // Vector4d.Transform is equivalent to *
            }

            private static Vector3d Catmull_Rom_Vector3 (Vector3d previous, Vector3d current, Vector3d next, Vector3d later, double t)
            {
                Vector4d result = Catmull_Rom_Vector4(new Vector4d(previous), new Vector4d(current), new Vector4d(next), new Vector4d(later), t);
                return new Vector3d(result.X, result.Y, result.Z);
            }

            private static double Catmull_Rom_Double (double previous, double current, double next, double later, double t)
            {
                Vector4d result = Catmull_Rom_Vector4(new Vector4d(previous), new Vector4d(current), new Vector4d(next), new Vector4d(later), t);
                return result.X;
            }

            /// <summary>
            /// Linear interpolation
            /// </summary>
            /// <param name="t">number between 0 and 1</param>
            /// <param name="previous">unused</param>
            /// <param name="later">unused</param>
            public static object LERP (object previous, object current, object next, object later, double t)
            {
                if (previous is Vector4d)
                    return Vector4d.Lerp((Vector4d)current, (Vector4d)next, t);
                if (previous is Vector3d)
                    return Vector3d.Lerp((Vector3d)current, (Vector3d)next, t);
                if (previous is double)
                    return (1 - t) * (double)current + t * (double)next;
                throw new ArgumentException("Type not supported: '" + previous.GetType() + "'.");
            }
        }
    }

    public class AnimatableStaticCamera : StaticCamera, IAnimatableCamera
    {
        public virtual AnimatedCamera.Parameter[] GetParams ()
        {
            return new AnimatedCamera.Parameter[] {
                new AnimatedCamera.Parameter("position", AnimatedCamera.Parsers.ParseVector3, AnimatedCamera.Interpolators.Catmull_Rom, true),
                new AnimatedCamera.Parameter("direction", AnimatedCamera.Parsers.ParseVector3, AnimatedCamera.Interpolators.Catmull_Rom, true),
                new AnimatedCamera.Parameter("angle", AnimatedCamera.Parsers.ParseDouble, AnimatedCamera.Interpolators.LERP)
            };
        }

        public virtual void ApplyParams (Dictionary<string, object> p)
        {
            try
            {
                center = (Vector3d)p["position"];
                direction = (Vector3d)p["direction"];
                if (p.ContainsKey("angle"))
                    hAngle = MathHelper.DegreesToRadians((double)p["angle"]);
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException("Invalid camera script or error when loading it.");
            }
            prepare();
        }
    }

    public interface IAnimatableCamera : ICamera
    {
        AnimatedCamera.Parameter[] GetParams ();
        void ApplyParams (Dictionary<string, object> p);
    }
}
