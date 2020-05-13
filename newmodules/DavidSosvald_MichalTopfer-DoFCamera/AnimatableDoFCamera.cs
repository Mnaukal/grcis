using OpenTK;
using System;
using System.Collections.Generic;

namespace DavidSosvald_MichalTopfer
{
    public class AnimatableDoFCamera : DoFCamera //, IAnimatableCamera
    {
        public virtual IEnumerable<Animator.Parameter> GetParams ()
        {
            return new Animator.Parameter[] {
                new Animator.Parameter("position", Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true),
                new Animator.Parameter("direction", Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true),
                new Animator.Parameter("angle", Animator.Parsers.ParseDouble, Animator.Interpolators.LERP),
                new Animator.Parameter("max_shift", Animator.Parsers.ParseDouble, Animator.Interpolators.LERP),
                new Animator.Parameter("focus", Animator.Parsers.ParseDouble, Animator.Interpolators.LERP),
                new Animator.Parameter("focus_point", Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom),
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
