using System;
using System.Collections.Generic;
using MathSupport;
using OpenTK;
using System.IO;
using System.Text.RegularExpressions;

namespace Rendering
{
  public class AnimatedCamera : ICamera, ITimeDependent
  {
    private IAnimatableCamera animatableCamera;
    private string[] paramNames;
    private static List<Dictionary<string, object>> parameters = new List<Dictionary<string, object>>();
    private static List<int> paramTimesMs = new List<int>();

    // matrix for Catmull-Rom spline
    private static Vector4 m0 = new Vector4(-1, 3, -3, 1);
    private static Vector4 m1 = new Vector4(2, -5, 4, -1);
    private static Vector4 m2 = new Vector4(-1, 0, 1, 0);
    private static Vector4 m3 = new Vector4(0, 2, 0, 0);
    private static Matrix4 splineMatrix = new Matrix4(m0, m1, m2, m3);

    public AnimatedCamera (IAnimatableCamera animatableCamera, string fileName)
    {
      this.animatableCamera = animatableCamera;
      paramNames = animatableCamera.GetParamNames();
      ReadAndSaveCameraScript(fileName);
      Start = paramTimesMs[0];
      End = paramTimesMs[paramTimesMs.Count - 1];
    }

    private AnimatedCamera (IAnimatableCamera animatableCamera)
    {
      this.animatableCamera = animatableCamera;
    }

    private void ReadAndSaveCameraScript(string fileName)
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
        string line = sr.ReadLine();
        char[] colonSeparator = {':'};
        char [] commaSeparator = {','};
        bool firstKeyFrame = true;
        var keyFrameParams = new Dictionary<string, object>();
        bool firstParams = true;
          
        while (line != null)
        {
          string[] tokens = line.Split(colonSeparator, StringSplitOptions.RemoveEmptyEntries);
          if (tokens.Length == 1)
          {
            paramTimesMs.Add(Int32.Parse(tokens[0]));
            if (!firstKeyFrame)
            {
              if (firstParams)
              {
                for (int i = 0; i < paramNames.Length; i++)
                {
                  if (!keyFrameParams.TryGetValue(paramNames[i], out object ignored))
                    throw new ArgumentException("The value of some parameters " +
                      "wasn't provided in the first key frame.");
                }
                firstParams = false;
              }
              parameters.Add(CopyDictionary(keyFrameParams));
            }
          }
          else
          {
            bool found = false;
            foreach (string param in paramNames)
            {
              tokens[0] = Regex.Replace(tokens[0], @"\s+", String.Empty);
              if (param == tokens[0])
              {
                found = true;
                break;
              }
            }
            if (!found)
              throw new ArgumentException("A token in camera script wasn't any of the provided ParamNames.");

            firstKeyFrame = false;
            //expecting entries in form "paramName: N, N, N", where N is an integer
            string[] values = tokens[1].Split(commaSeparator);
            keyFrameParams[tokens[0]] = new Vector3
              (Int32.Parse(values[0]), Int32.Parse(values[1]), Int32.Parse(values[2]));
          }
          line = sr.ReadLine();
          if (line == null)
            parameters.Add(CopyDictionary(keyFrameParams));
        }
        sr.Close();
      }
      catch (IOException)
      {
        throw new IOException("An error occurred while reading the script file.");
      }
    }

    private Dictionary<string, object> CopyDictionary(Dictionary<string, object> original)
    {
      var copy = new Dictionary<string, object>();
      foreach (string param in paramNames)
        copy[param] = original[param];
      return copy;
    }

    public double AspectRatio { get => animatableCamera.AspectRatio; set => animatableCamera.AspectRatio = value; }
    public double Width { get => animatableCamera.Width; set => animatableCamera.Width = value; }
    public double Height { get => animatableCamera.Height; set => animatableCamera.Height = value; }
    public double Start { get; set; }
    public double End { get; set; }
    public double Time { get; set; }
    public object Clone()
    {
      AnimatedCamera clonedCamera = new AnimatedCamera(this.animatableCamera);
      clonedCamera.Start = this.Start;
      clonedCamera.End = this.End;
      clonedCamera.Time = this.Time;
      return clonedCamera;
    }

    public bool GetRay (double x, double y, out Vector3d p0, out Vector3d p1)
    {
      int frameStart = 0;
      for (int i = paramTimesMs.Count - 1; i >= 0; i--)
      {
        if (Time >= paramTimesMs[i])
          frameStart = i;
        else
          break;
      }

      int nextFrame = frameStart + 1 > paramTimesMs.Count - 1 ? paramTimesMs.Count - 1 : frameStart + 1; 
      double t = (Time - paramTimesMs[frameStart]) / (paramTimesMs[nextFrame] - paramTimesMs[frameStart]);
      Vector4 tVector = new Vector4((float)Math.Pow(t, 3), (float)Math.Pow(t, 2), (float)t, 1);

      var cameraParams = new Dictionary<string, object>();

      foreach (string param in paramNames)
      {
        Vector4[] points = new Vector4[4];
        for (int i = -1; i <= 2; i++)
        {
          int index = frameStart + i;
          if (index < 0)
            index = 0;
          if (index >= parameters.Count)
            index = parameters.Count - 1;

          Vector3 v = (Vector3)parameters[index][param];
          points[i + 1] = new Vector4(v.X, v.Y, v.Z, 1);
        }
        Matrix4 matrixOfPoints = new Matrix4(points[0], points[1], points[2], points[3]);

        Vector4 interpolatedVector = 0.5f * tVector * splineMatrix * matrixOfPoints;
        cameraParams[param] = new Vector3d(interpolatedVector.X, interpolatedVector.Y, interpolatedVector.Z);
      }
      
      animatableCamera.ApplyParams(cameraParams);
      return animatableCamera.GetRay(x, y, out p0, out p1);
    }
  }

  public class AnimatableStaticCamera : StaticCamera, IAnimatableCamera
  {
    public virtual string[] GetParamNames ()
    {
      return new string[] { "position", "direction" };
    }

    public virtual void ApplyParams (Dictionary<string, object> p)
    {
      try
      {
        center = (Vector3d)p["position"];
        direction = (Vector3d)p["direction"];
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
    string[] GetParamNames ();
    void ApplyParams (Dictionary<string, object> p);
  }
}
