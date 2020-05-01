using System;
using System.Collections.Generic;
using MathSupport;
using OpenTK;
using System.IO;

namespace Rendering
{
  class AnimatedCamera : ICamera, ITimeDependent
  {
    private IAnimatableCamera animatableCamera;
    private List<Dictionary<string, object>> parameters;
    private List<int> paramTimesMs;

    public AnimatedCamera (AnimatableStaticCamera animatableCamera, string fileName)
    {
      this.animatableCamera = animatableCamera;
      ReadAndSaveCameraScript(fileName);
    }

    private void ReadAndSaveCameraScript(string fileName)
    {
      string[] paramNames = animatableCamera.GetParamNames();

      StreamReader sr;
      try
      {
        sr = new StreamReader(fileName);
      }
      catch (IOException)
      {
        throw new IOException("Please specify a file with camera script.");
      }
      // TODO: keep params if not changed
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
              parameters.Add(keyFrameParams);
            }
          }
          else
          {
            bool found = false;
            foreach (string param in paramNames)
            {
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
            keyFrameParams[tokens[0]] = new Vector3d
              (Double.Parse(values[0]), Double.Parse(values[1]), Double.Parse(values[2]));
          }
          line = sr.ReadLine();
          if (line == null)
            parameters.Add(keyFrameParams);
        }
        sr.Close();
      }
      catch (IOException)
      {
        throw new IOException("An error occurred while reading the script file.");
      }
    }

    public double AspectRatio { get => animatableCamera.AspectRatio; set => animatableCamera.AspectRatio = value; }
    public double Width { get => animatableCamera.Width; set => animatableCamera.Width = value; }
    public double Height { get => animatableCamera.Height; set => animatableCamera.Height = value; }

    public bool GetRay (double x, double y, out Vector3d p0, out Vector3d p1)
    {
      // TODO: GetRay should interpolate the keyframes, apply them to IAnimatableCamera inside and call GetRay on it
      throw new NotImplementedException();
    }
  }

  class AnimatableStaticCamera : StaticCamera, IAnimatableCamera
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

  interface IAnimatableCamera : ICamera
  {
    string[] GetParamNames ();
    void ApplyParams (Dictionary<string, object> p);
  }
}
