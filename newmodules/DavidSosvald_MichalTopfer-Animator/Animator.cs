using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using Rendering;
using System.Diagnostics;

namespace DavidSosvald_MichalTopfer
{
    public class Animator : ITimeDependent, ITimeDependentProperty
    {
        private Dictionary<string, Parameter> parameters;
        private List<Keyframe> keyframes = new List<Keyframe>();
        private string keyframesFile;

        private static char[] colonSeparator = {':'};
        private static char[] commaSeparator = {','};

        private TimeAndParams[] currentParams;
        private int currentParamsIndex;

#if DEBUG
        private static volatile int nextSerial = 0;
        private readonly int serial = nextSerial++;
        public int getSerial () => serial;
#endif

        public Animator (string keyframesFile, int currentParamsSize)
        {
#if DEBUG
            Debug.WriteLine("Animator #" + getSerial() + " created.");
#endif

            currentParams = new TimeAndParams[currentParamsSize];
            parameters = new Dictionary<string, Parameter>();
            this.keyframesFile = keyframesFile;
        }

        public Animator (string keyframesFile) : this(keyframesFile, 1) { }

        public void RegisterParams(IEnumerable<Parameter> parameters)
        {
            if (keyframes.Count > 0)
                throw new InvalidOperationException("All params have to be registered before loading keyframes.");

            foreach (var p in parameters)
            {
                if (!this.parameters.ContainsKey(p.Name))
                    this.parameters.Add(p.Name, p);
                else
                    Console.WriteLine("Parameter '" + p.Name + "' already exists.");
            }
        }

        public void RegisterParam (Parameter parameter) => RegisterParams(new Parameter[] { parameter });

        public void LoadKeyframes()
        {
            ReadAndSaveKeyframes(keyframesFile);
            Start = keyframes[0].Time;
            End = keyframes[keyframes.Count - 1].Time;
            Time = Start;
        }

        private void ReadAndSaveKeyframes (string fileName)
        {
            StreamReader sr;
            try
            {
                sr = new StreamReader(fileName);
            }
            catch (IOException)
            {
                throw new IOException("Error opening keyframes file: '" + fileName + "'.");
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
                        keyframeTime = double.Parse(tokens[0], CultureInfo.InvariantCulture);
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
                throw new IOException("Error reading keyframes file: '" + fileName + "'.");
            }
        }

        private void CheckRequiredParams (Dictionary<string, object> keyframeParams)
        {
            foreach (Parameter param in parameters.Values)
                if (param.Required && !keyframeParams.ContainsKey(param.Name))
                    throw new ArgumentException("The value of '" + param.Name + "' parameter wasn't provided in the first keyframe.");
        }

        private Dictionary<string, object> CopyDictionary (Dictionary<string, object> original)
        {
            var copy = new Dictionary<string, object>();
            foreach (string key in original.Keys)
                copy[key] = original[key];
            return copy;
        }

        public double Start { get; set; }
        public double End { get; set; }
        public double Time
        {
            get => time;
            set {
                time = value;
                currentParams[currentParamsIndex].T = time;
                currentParams[currentParamsIndex].P = InterpolateKeyframes(time);
                currentParamsIndex = (currentParamsIndex + 1) % currentParams.Length;
            }
        }
        private double time;

        public object Clone ()
        {
            Animator clone = new Animator(this.keyframesFile);
            clone.parameters = this.parameters;
            clone.keyframes = this.keyframes;
            clone.currentParamsIndex = this.currentParamsIndex;
            clone.Start = this.Start;
            clone.End = this.End;
            clone.time = this.time;
            return clone;
        }

        private Dictionary<string, object> InterpolateKeyframes(double time)
        {
            int i = 0;
            for (; i < keyframes.Count; i++)
            {
                if (keyframes[i].Time > time)
                    break;
            }
            i--;
            Keyframe previous = i >= 1 ? keyframes[i - 1] : keyframes[0];
            Keyframe current = keyframes[i];
            Keyframe next = i < keyframes.Count - 1 ?  keyframes[i+1] : keyframes[keyframes.Count - 1];
            Keyframe later = i < keyframes.Count - 2 ? keyframes[i + 2] : keyframes[keyframes.Count - 1];
            double t = (time - current.Time) / (next.Time - current.Time);
            if (double.IsNaN(t) && next.Time == current.Time)
                t = 1;

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

            return cameraParams;
        }

        /// <summary>
        /// Returns the value of 'paramName' in 'time'. Fast if 'time' is one of the cached values (computed when Time property is set), slow otherwise.
        /// </summary>
        public object getParam(string paramName, double time)
        {
            Dictionary<string, object> p = getParams(time);
            if (p.TryGetValue(paramName, out object value))
                return value;
            else
                throw new ArgumentException("paramName '" + paramName + "' not found.");
        }

        /// <summary>
        /// Returns the dictionary of parameters in 'time'. Fast if 'time' is one of the cached values (computed when Time property is set), slow otherwise.
        /// </summary>
        public Dictionary<string, object> getParams (double time)
        {
            return currentParams.FirstOrDefault(x => x.T == time).P ?? InterpolateKeyframes(time);
        }

        /// <summary>
        /// Returns the current value of 'paramName' (based on the value of the Time property)
        /// </summary>
        public object getParam (string paramName)
        {
            return getParam(paramName, Time);
        }

        /// <summary>
        /// Returns the current dictionary of parameters (based on the value of the Time property)
        /// </summary>
        public Dictionary<string, object> getParams ()
        {
            return getParams(Time);
        }

    #region ITimeDependentProperty methods implementation
    public object GetValue (in string name)
    {
      try
      {
        return getParam(name, Time);
      }
      catch
      {
        return null;
      }
    }

    public bool TryGetValue (in string name, ref float f)
    {
      try
      {
        f = (float)getParam(name, Time);
        return true;
      }
      catch
      {
        return false;
      }
    }

    public bool TryGetValue (in string name, ref double d)
    {
      try
      {
        d = (double)getParam(name, Time);
        return true;
      }
      catch
      {
        return false;
      }
    }

    public bool TryGetValue (in string name, ref Vector3 v3)
    {
      try
      {
        v3 = (Vector3)getParam(name, Time);
        return true;
      }
      catch
      {
        return false;
      }
    }

    public bool TryGetValue (in string name, ref Vector4 v4)
    {
      try
      {
        v4 = (Vector4)getParam(name, Time);
        return true;
      }
      catch
      {
        return false;
      }
    }

    public bool TryGetValue (in string name, ref Vector3d v3)
    {
      try
      {
        v3 = (Vector3d)getParam(name, Time);
        return true;
      }
      catch
      {
        return false;
      }
    }

    public bool TryGetValue (in string name, ref Vector4d v4)
    {
      try
      {
        v4 = (Vector4d)getParam(name, Time);
        return true;
      }
      catch
      {
        return false;
      }
    }

    public bool TryGetValue (in string name, ref Quaterniond q)
    {
      try
      {
        q = (Quaterniond)getParam(name, Time);
        return true;
      }
      catch
      {
        return false;
      }
    }

    public bool TryGetValue (in string name, ref Matrix4d m4)
    {
      try
      {
        m4 = (Matrix4d)getParam(name, Time);
        return true;
      }
      catch
      {
        return false;
      }
    }
    #endregion

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

        private struct TimeAndParams
        {
            public double T;
            public Dictionary<string, object> P;
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
                    return new Vector3d(
                        double.Parse(values[0], CultureInfo.InvariantCulture),
                        double.Parse(values[1], CultureInfo.InvariantCulture),
                        double.Parse(values[2], CultureInfo.InvariantCulture));
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
                    return double.Parse(s, CultureInfo.InvariantCulture);
                }
                catch
                {
                    throw new ArgumentException("Error while parsing double: '" + s + "'.");
                }
            }

            public static object ParseDoubleArray (string s)
            {
                try
                {
                    string[] tokens = s.Split(commaSeparator);
                    double[] array = new double[tokens.Length];
                    for (int i = 0; i < tokens.Length; i++)
                        array[i] = double.Parse(tokens[i], CultureInfo.InvariantCulture);
                    return array;
                }
                catch
                {
                    throw new ArgumentException("Error while parsing double array: '" + s + "'.");
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
                if (previous is double[])
                    return Catmull_Rom_DoubleArray((double[])previous, (double[])current, (double[])next, (double[])later, t);
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

            private static double[] Catmull_Rom_DoubleArray (double[] previous, double[] current, double[] next, double[] later, double t)
            {
                double[] result = new double[previous.Length];
                for (int i = 0; i < previous.Length; i++)
                    result[i] = Catmull_Rom_Double(previous[i], current[i], next[i], later[i], t);
                return result;
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
                if (previous is double[])
                    return LERP_DoubleArray((double[])previous, (double[])current, (double[])next, (double[])later, t);
                throw new ArgumentException("Type not supported: '" + previous.GetType() + "'.");
            }

            private static double[] LERP_DoubleArray (double[] previous, double[] current, double[] next, double[] later, double t)
            {
                double[] result = new double[previous.Length];
                for (int i = 0; i < previous.Length; i++)
                    result[i] = (double)LERP(previous[i], current[i], next[i], later[i], t);
                return result;
            }
        }
    }
}
