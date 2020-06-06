using OpenTK;
using Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DavidSosvald_MichalTopfer
{
    public class CameraAnimationProxy : ICamera, ITimeDependent
    {
        private readonly string positionParamName;
        private readonly string directionParamName;
        private readonly ICamera innerCamera;
        Vector3d originalPosition, originalDirection, up = Vector3d.Zero;
        Matrix4d positionUpdate, directionUpdate;

        public CameraAnimationProxy (ICamera innerCamera, Vector3d up, Animator animator = null, string positionParamName = "position", string directionParamName = "direction") : this(innerCamera, animator, positionParamName, directionParamName)
        {
            this.up = up;
        }

        public CameraAnimationProxy (ICamera innerCamera, Animator animator = null, string positionParamName = "position", string directionParamName = "direction")
        {
            this.positionParamName = positionParamName;
            this.directionParamName = directionParamName;
            animator?.RegisterParams(GetParams());
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
            if (up == Vector3d.Zero)
            {
                innerCamera.GetRay(Width / 2, 0, out Vector3d top_p0, out Vector3d top_p1);
                innerCamera.GetRay(Width / 2, Height, out Vector3d bot_p0, out Vector3d bot_p1);
                up = (top_p0 + top_p1) - (bot_p0 + bot_p1);
            }
            up.Normalize();
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
            if (innerCamera is ITimeDependent c)
                c.Time = time;
            ApplyParams((ITimeDependentProperty)MT.scene.Animator);
        }

        void ApplyParams (ITimeDependentProperty tdp)
        {
            Vector3d position = originalPosition, direction = originalDirection;
            if (!tdp.TryGetValue(positionParamName, ref position) ||
                !tdp.TryGetValue(directionParamName, ref direction))
            {
#if DEBUG
                Debug.WriteLine($"Position or direction not specified in CameraProxy #{getSerial()} at time {Time}");
#endif
            }

            positionUpdate = Matrix4d.CreateTranslation(position - originalPosition);

            /* up_plane = plane with 'up' as normal
             * the rotation is composed of 3 parts:
             *   1. pitch1 = rotate the 'originalDirection' to the up_plane
             *   2. yaw = rotation in the up_plane
             *   3. pitch2 = rotate from the up_plane to the 'direction'
             */

            Vector3d originalDirection_up_plane = originalDirection - VectorProjection(originalDirection, up);
            Vector3d direction_up_plane = direction - VectorProjection(direction, up);

            double pitch1 = Vector3d.CalculateAngle(originalDirection_up_plane, originalDirection);
            Vector3d pitch1_axis = Vector3d.Cross(originalDirection_up_plane, originalDirection);
            Matrix4d pitch1_rotation = pitch1 > 0 ?
                Matrix4d.CreateFromAxisAngle(pitch1_axis, pitch1) :
                Matrix4d.Identity;

            double yaw = Vector3d.CalculateAngle(originalDirection_up_plane, direction_up_plane);
            Matrix4d yaw_rotation = Matrix4d.CreateFromAxisAngle(up, yaw);

            double pitch2 = Vector3d.CalculateAngle(direction_up_plane, direction);
            Vector3d pitch2_axis = Vector3d.Cross(direction_up_plane, direction);
            Matrix4d pitch2_rotation = pitch2 > 0 ?
                Matrix4d.CreateFromAxisAngle(pitch2_axis, pitch2) :
                Matrix4d.Identity;

            directionUpdate = pitch1_rotation * yaw_rotation * pitch2_rotation;
        }

        private Vector3d VectorProjection (Vector3d vec, Vector3d onto) => Vector3d.Dot(vec, onto) * onto / onto.Length;

        public object Clone ()
        {
            return this;
        }

#if DEBUG
        private static volatile int nextSerial = 0;
        private readonly int serial = nextSerial++;
        public int getSerial () => serial;
#endif

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
    }
}
