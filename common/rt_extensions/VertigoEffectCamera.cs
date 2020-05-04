using System;
using System.Collections.Generic;
using System.Linq;
using MathSupport;
using OpenTK;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using Rendering;

namespace Rendering
{
    public class VertigoEffectCamera : IAnimatableCamera
    {
        IAnimatableCamera innerCamera;
        double target_width = -1; 

        public VertigoEffectCamera (IAnimatableCamera innerCamera)
        {
            this.innerCamera = innerCamera;
        }

        public VertigoEffectCamera (IAnimatableCamera innerCamera, double target_width)
        {
            this.innerCamera = innerCamera;
            this.target_width = target_width;
        }

        public virtual AnimatedCamera.Parameter[] GetParams ()
        {
            var innerCameraParams = innerCamera.GetParams().ToList();
            innerCameraParams.Add(new AnimatedCamera.Parameter("target", AnimatedCamera.Parsers.ParseVector3, AnimatedCamera.Interpolators.Catmull_Rom, true));
            innerCameraParams.Add(new AnimatedCamera.Parameter("target_width", AnimatedCamera.Parsers.ParseDouble, AnimatedCamera.Interpolators.LERP, target_width < 0)); // if target_width wasn't specified in the constructor, it is a required param
            if (!innerCameraParams.Any(p => p.Name == "position"))
                innerCameraParams.Add(new AnimatedCamera.Parameter("position", AnimatedCamera.Parsers.ParseVector3, AnimatedCamera.Interpolators.Catmull_Rom, true));
            if (!innerCameraParams.Any(p => p.Name == "angle"))
                innerCameraParams.Add(new AnimatedCamera.Parameter("angle", AnimatedCamera.Parsers.ParseDouble, AnimatedCamera.Interpolators.LERP));

            return innerCameraParams.ToArray();
        }

        public virtual void ApplyParams (Dictionary<string, object> p)
        {
            double target_width = p.ContainsKey("target_width") ? (double)p["target_width"] : this.target_width;
            Vector3d position = (Vector3d)p["position"];
            Vector3d target = (Vector3d)p["target"];
            double distance = Vector3d.Distance(position, target);

            double angle = 2 * Math.Atan(target_width / (2 * distance));
            angle = MathHelper.RadiansToDegrees(angle);
            p["angle"] = angle;

            p["direction"] = (target - position).Normalized();

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
