using OpenTK;
using System;
using System.Collections.Generic;

namespace DavidSosvald_MichalTopfer
{
    public class AnimatableDoFCamera : DoFCamera, IAnimatableCamera
    {
        public virtual AnimatedCamera.Parameter[] GetParams ()
        {
            return new AnimatedCamera.Parameter[] {
                new AnimatedCamera.Parameter("position", AnimatedCamera.Parsers.ParseVector3, AnimatedCamera.Interpolators.Catmull_Rom, true),
                new AnimatedCamera.Parameter("direction", AnimatedCamera.Parsers.ParseVector3, AnimatedCamera.Interpolators.Catmull_Rom, true),
                new AnimatedCamera.Parameter("angle", AnimatedCamera.Parsers.ParseDouble, AnimatedCamera.Interpolators.LERP),
                new AnimatedCamera.Parameter("max_shift", AnimatedCamera.Parsers.ParseDouble, AnimatedCamera.Interpolators.LERP),
                new AnimatedCamera.Parameter("focus", AnimatedCamera.Parsers.ParseDouble, AnimatedCamera.Interpolators.LERP),
                new AnimatedCamera.Parameter("focus_point", AnimatedCamera.Parsers.ParseVector3, AnimatedCamera.Interpolators.Catmull_Rom),
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
                if (p.ContainsKey("max_shift"))
                    max_shift = (double)p["max_shift"];
                if (p.ContainsKey("focus"))
                    focus_distance = (double)p["focus"];
                else if (p.ContainsKey("focus_point"))
                    SetFocusPoint((Vector3d)p["focus"]);
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException("Invalid camera script or error when loading it.");
            }
            prepare();
        }
    }
}
