using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using Rendering;

namespace DavidSosvald_MichalTopfer
{
    public class VertigoEffectCamera : ITimeDependent, ICamera
    {
        IVertigoInnerCamera innerCamera;
        double target_width = -1;
        bool updateAngle, updateDirection;
        private readonly string vertigoParamName;
        private readonly string targetParamName;
        private readonly string targetWidthParamName;

#if DEBUG
        private static volatile int nextSerial = 0;
        private readonly int serial = nextSerial++;
        public int getSerial () => serial;
#endif

        public VertigoEffectCamera (IVertigoInnerCamera innerCamera, Animator animator, bool updateAngle = true, bool updateDirection = true, string vertigoParamName = "vertigo", string targetParamName = "target", string targetWidthParamName = "target_width")
        {
            this.innerCamera = innerCamera;
            this.updateAngle = updateAngle;
            this.updateDirection = updateDirection;
            this.vertigoParamName = vertigoParamName;
            this.targetParamName = targetParamName;
            this.targetWidthParamName = targetWidthParamName;
            animator?.RegisterParams(GetParams());
        }

        public VertigoEffectCamera (IVertigoInnerCamera innerCamera, Animator animator, double target_width, bool updateAngle = true, bool updateDirection = true, string vertigoParamName = "vertigo", string targetParamName = "target", string targetWidthParamName = "target_width")
        {
            this.target_width = target_width;
            this.innerCamera = innerCamera;
            this.updateAngle = updateAngle;
            this.updateDirection = updateDirection;
            this.vertigoParamName = vertigoParamName;
            this.targetParamName = targetParamName;
            this.targetWidthParamName = targetWidthParamName;
            animator?.RegisterParams(GetParams());
        }

        public IEnumerable<Animator.Parameter> GetParams ()
        {
            return new Animator.Parameter[] {
                new Animator.Parameter(vertigoParamName, Animator.Parsers.ParseDouble, Animator.Interpolators.LERP),
                new Animator.Parameter(targetParamName, Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true),
                new Animator.Parameter(targetWidthParamName, Animator.Parsers.ParseDouble, Animator.Interpolators.LERP, target_width < 0), // if target_width wasn't specified in the constructor, it is a required param
            };
        }

        public double Time
        {
            get => innerCamera.Time;
            set
            {
                innerCamera.Time = value;
                SetTime(value);
            }
        }

        void SetTime (double time)
        {
            if (MT.scene == null)
                return;
            Dictionary<string, object> p = ((Animator)MT.scene.Animator).getParams(time);
            ApplyParams(p);
        }

        public virtual void ApplyParams (Dictionary<string, object> p)
        {
            double vertigo = 1;
            if (p.ContainsKey(vertigoParamName))
                vertigo = (double)p[vertigoParamName];

            if (vertigo > 0)
            { 
                double target_width = p.ContainsKey(targetWidthParamName) ? (double)p[targetWidthParamName] : this.target_width;
                Vector3d position = innerCamera.GetPosition();
                Vector3d target = (Vector3d)p[targetParamName];
                double distance = Vector3d.Distance(position, target);

                if (updateAngle)
                {
                    double angle = 2 * Math.Atan(target_width / (2 * distance));
                    innerCamera.SetAngle(vertigo * angle + (1 - vertigo) * innerCamera.GetAngle()); // lerp with current value (vertigo == 1 -> completely controlled by vertigo effect)
                }

                if (updateDirection)
                {
                    Vector3d direction = (target - position).Normalized();
                    innerCamera.SetDirection((vertigo * direction + (1 - vertigo) * innerCamera.GetDirection()).Normalized());
                }
            }
        }

        public bool GetRay (double x, double y, out Vector3d p0, out Vector3d p1)
        {
            return innerCamera.GetRay(x, y, out p0, out p1);
        }

        public object Clone ()
        {
            IVertigoInnerCamera clonedInner = (IVertigoInnerCamera)innerCamera.Clone();
            return new VertigoEffectCamera(clonedInner, null, target_width, updateAngle, updateDirection, vertigoParamName, targetParamName, targetWidthParamName);
        }

        public double AspectRatio { get => innerCamera.AspectRatio; set => innerCamera.AspectRatio = value; }
        public double Width { get => innerCamera.Width; set => innerCamera.Width = value; }
        public double Height { get => innerCamera.Height; set => innerCamera.Height = value; }
        public double Start { get => innerCamera.Start; set => innerCamera.Start = value; }
        public double End { get => innerCamera.End; set => innerCamera.End = value; }
    }

    public interface IVertigoInnerCamera : ITimeDependent, ICamera
    {
        Vector3d GetPosition ();
        /// <summary>
        /// Returns current viewing angle of the camera in radians
        /// </summary>
        double GetAngle ();
        /// <summary>
        /// Sets the viewing angle of the camera (in radians)
        /// </summary>
        void SetAngle (double angle);
        Vector3d GetDirection ();
        void SetDirection (Vector3d direction);
    }
}
