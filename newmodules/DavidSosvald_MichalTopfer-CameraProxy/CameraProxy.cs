using OpenTK;
using Rendering;
using System;
using System.Collections.Generic;

namespace DavidSosvald_MichalTopfer
{
    public class CameraProxy : ICamera, ITimeDependent
    {
        private readonly string positionParamName;
        private readonly string directionParamName;
        private readonly ICamera innerCamera;
        Vector3d originalPosition, originalDirection, up;
        Matrix4d positionUpdate, directionUpdate;

        public CameraProxy (Animator animator, ICamera innerCamera, string positionParamName = "position", string directionParamName = "direction") : this(innerCamera, positionParamName, directionParamName)
        {
            animator?.RegisterParams(GetParams());
        }

        private CameraProxy (ICamera innerCamera, string positionParamName, string directionParamName)
        {
            this.positionParamName = positionParamName;
            this.directionParamName = directionParamName;
            this.innerCamera = innerCamera;
            Initialize();
        }

        public IEnumerable<Animator.Parameter> GetParams ()
        {
            return new Animator.Parameter[] {
                new Animator.Parameter(positionParamName, Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true),
                new Animator.Parameter(directionParamName, Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true),
            };
        }

        public void Initialize()
        {
            innerCamera.GetRay(Width / 2, Height / 2, out originalPosition, out originalDirection);
            innerCamera.GetRay(Width / 2, 0, out Vector3d top_p0, out Vector3d top_p1);
            innerCamera.GetRay(Width / 2, Height, out Vector3d bot_p0, out Vector3d bot_p1);
            up = (top_p0 + top_p1) - (bot_p0 + bot_p1);
        }

        public double Start { get; set; }
        public double End { get; set; }
        private double time;
        public double Time
        {
            get => time;
            set
            {
                time = value;
                SetTime(value);
            }
        }

        public double AspectRatio { get => innerCamera.AspectRatio; set => innerCamera.AspectRatio = value; }
        public double Width { get => innerCamera.Width; set => innerCamera.Width = value; }
        public double Height { get => innerCamera.Height; set => innerCamera.Height = value; }

        void SetTime (double time)
        {
            if (MT.scene == null)
                return;
            Dictionary<string, object> p = ((Animator)MT.scene.Animator).getParams(time);
            ApplyParams(p);
        }

        void ApplyParams (Dictionary<string, object> p)
        {
            Vector3d position = (Vector3d)p[positionParamName];
            Vector3d direction = (Vector3d)p[directionParamName];

            positionUpdate = Matrix4d.CreateTranslation(position - originalPosition);
            Vector3d axis = Vector3d.Cross(originalDirection, direction);
            double angle = Vector3d.CalculateAngle(originalDirection, direction);
            directionUpdate = Matrix4d.CreateFromAxisAngle(axis, angle);
        }

        public object Clone ()
        {
            return this;
        }

#if DEBUG
        private static volatile int nextSerial = 0;
        private readonly int serial = nextSerial++;
        public int getSerial () => serial;

        public bool GetRay (double x, double y, out Vector3d p0, out Vector3d p1)
        {
            if (innerCamera.GetRay(x, y, out p0, out p1))
            {
                p0 = Vector3d.Transform(p0, positionUpdate);
                p1 = Vector3d.Transform(p1, directionUpdate);
                return true;
            }
            return false;
        }
#endif
    }
}
