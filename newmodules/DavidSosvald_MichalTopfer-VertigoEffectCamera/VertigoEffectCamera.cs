using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace DavidSosvald_MichalTopfer
{
    public class VertigoEffectCamera : IAnimatableCamera
    {
        IAnimatableCamera innerCamera;
        double target_width = -1;
        bool updateAngle, updateDirection;

        public VertigoEffectCamera (IAnimatableCamera innerCamera, bool updateAngle = true, bool updateDirection = true)
        {
            this.innerCamera = innerCamera;
            this.updateAngle = updateAngle;
            this.updateDirection = updateDirection;
        }

        public VertigoEffectCamera (IAnimatableCamera innerCamera, bool updateAngle, bool updateDirection, double target_width) : this(innerCamera, updateAngle, updateDirection)
        {
            this.target_width = target_width;
        }

        public virtual IEnumerable<Animator.Parameter> GetParams ()
        {
            var innerCameraParams = innerCamera.GetParams().ToList();
            innerCameraParams.Add(new Animator.Parameter("vertigo", Animator.Parsers.ParseDouble, Animator.Interpolators.LERP));
            innerCameraParams.Add(new Animator.Parameter("target", Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true));
            innerCameraParams.Add(new Animator.Parameter("target_width", Animator.Parsers.ParseDouble, Animator.Interpolators.LERP, target_width < 0)); // if target_width wasn't specified in the constructor, it is a required param
            if (!innerCameraParams.Any(p => p.Name == "position"))
                innerCameraParams.Add(new Animator.Parameter("position", Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true));
            if (!innerCameraParams.Any(p => p.Name == "angle"))
                innerCameraParams.Add(new Animator.Parameter("angle", Animator.Parsers.ParseDouble, Animator.Interpolators.LERP));

            return innerCameraParams; 
        }

        public virtual void ApplyParams (Dictionary<string, object> p)
        {
            double vertigo = 1;
            if (p.ContainsKey("vertigo"))
                vertigo = (double)p["vertigo"];

            if (vertigo > 0)
            { 
                double target_width = p.ContainsKey("target_width") ? (double)p["target_width"] : this.target_width;
                Vector3d position = (Vector3d)p["position"];
                Vector3d target = (Vector3d)p["target"];
                double distance = Vector3d.Distance(position, target);

                if (updateAngle)
                {
                    double angle = 2 * Math.Atan(target_width / (2 * distance));
                    angle = MathHelper.RadiansToDegrees(angle);
                    p["angle"] = vertigo * angle + (1 - vertigo) * (double)p["angle"]; // lerp with current value (vertigo == 1 -> completely controlled by vertigo effect)
                }

                if (updateDirection)
                {
                    Vector3d direction = (target - position).Normalized();
                    p["direction"] = (vertigo * direction + (1 - vertigo) * (Vector3d)p["direction"]).Normalized();
                }
            }
            innerCamera.ApplyParams(p);
        }

        public bool GetRay (double x, double y, out Vector3d p0, out Vector3d p1)
        {
            return innerCamera.GetRay(x, y, out p0, out p1);
        }

        public double AspectRatio { get => innerCamera.AspectRatio; set => innerCamera.AspectRatio = value; }
        public double Width { get => innerCamera.Width; set => innerCamera.Width = value; }
        public double Height { get => innerCamera.Height; set => innerCamera.Height = value; }
    }
}
