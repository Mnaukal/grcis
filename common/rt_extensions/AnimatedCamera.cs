using System;
using System.Collections.Generic;
using MathSupport;
using OpenTK;

namespace Rendering
{
  class AnimatedCamera : ICamera, ITimeDependent
  {
    // TODO: read keyframes, save them
    // TODO: GetRay should interpolate the keyframes, apply them to IAnimatableCamera inside and call GetRay on it
  }

  class AnimatableStaticCamera : StaticCamera, IAnimatableCamera
  {
    public virtual string[] GetParamNames ()
    {
      return new string[] { "position", "direction" };
    }

    public virtual void ApplyParams (Dictionary<string, object> p)
    {
      // TODO: handle missing keys in dictionary
      center = (Vector3d)p["position"];
      direction = (Vector3d)p["direction"];
      prepare();
    }
  }

  interface IAnimatableCamera
  {
    string[] GetParamNames ();
    void ApplyParams (Dictionary<string, object> p);
  }
}
