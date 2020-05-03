using OpenTK;
using System;
using System.Collections.Generic;

namespace Rendering
{
    public class DoFCamera : StaticCamera, ICamera
    {
        public enum ShiftMode { Circle, RandomNormal, RandomUniform, CircleJitter };
        const int POINTS_ON_CIRCLE = 12;

        protected double focus_distance;
        protected double max_shift;
        protected ShiftMode shift_mode;
        protected Vector3d shift_dir;

        public DoFCamera() { }

        public DoFCamera(Vector3d cen, Vector3d dir, double ang, double focus_distance = 1.0, double max_shift = 0.03, ShiftMode shift_mode = ShiftMode.CircleJitter) : base(cen, dir, ang)
        {
            this.focus_distance = focus_distance;
            this.max_shift = max_shift;
            this.shift_mode = shift_mode;
            prepare();
        }

        public DoFCamera (Vector3d cen, Vector3d dir, double ang, Vector3d focus_point, double max_shift = 0.05, ShiftMode shift_mode = ShiftMode.CircleJitter) : base(cen, dir, ang)
        {
            this.max_shift = max_shift;
            this.shift_mode = shift_mode;
            SetFocusPoint(focus_point);
            prepare();
        }

        protected void SetFocusPoint (Vector3d focus_point)
        {
            this.focus_distance = Vector3d.Distance(center, focus_point);
        }

        /// <summary>
        /// Should be called after every parameter change..
        /// </summary>
        protected new void prepare ()
        {
            Vector3d a = (direction == Vector3d.UnitX || direction == -Vector3d.UnitX) ? Vector3d.UnitY : Vector3d.UnitX; // arbitrary vector not parallel to direction
            shift_dir = Vector3d.Cross(direction, a).Normalized(); // shift_dir is perpendicular to direction
            base.prepare();
        }

        public new bool GetRay (double x, double y, out Vector3d from, out Vector3d direction)
        {
            bool ret = base.GetRay(x, y, out Vector3d orig_from, out Vector3d orig_direction);

            Vector3d focus = orig_from + focus_distance * orig_direction;
            Vector3d shift = GetShift();
            from = orig_from + shift;
            direction = focus - from;
            direction.Normalize();

            return ret;
        }

        protected Vector3d GetShift()
        {
            switch (shift_mode)
            {
                case ShiftMode.RandomUniform:
                    return GetShift_RandomUniform(max_shift);
                case ShiftMode.RandomNormal:
                    return GetShift_RandomNormal(max_shift);
                case ShiftMode.Circle:
                    return GetShift_Circle(max_shift);
                case ShiftMode.CircleJitter:
                default:
                    return GetShift_CircleJitter(max_shift);
            }
        }

        protected Vector3d GetShift_RandomUniform(double max_shift)
        {
            return max_shift * new Vector3d(MT.rnd.UniformNumber() - 0.5, MT.rnd.UniformNumber() - 0.5, MT.rnd.UniformNumber() - 0.5);
        }

        protected Vector3d GetShift_RandomNormal (double max_shift)
        {
            return max_shift * new Vector3d(MT.rnd.Normal(0, 1), MT.rnd.Normal(0, 1), MT.rnd.Normal(0, 1));
        }

        protected Vector3d GetShift_Circle (double max_shift)
        {
            if (MT.rank == 0)
                return Vector3d.Zero;

            int num_circles = (MT.total - 2) / POINTS_ON_CIRCLE + 1;
            int circle_index = (MT.rank - 1) / POINTS_ON_CIRCLE + 1;
            double distance = max_shift * circle_index / (double)num_circles;
            int point_index = (MT.rank - 1) % POINTS_ON_CIRCLE;
            int points_on_circle = POINTS_ON_CIRCLE;
            if (circle_index == num_circles && (MT.total - 1) % POINTS_ON_CIRCLE != 0)
                points_on_circle = (MT.total - 1) % POINTS_ON_CIRCLE;
            double angle = 2 * Math.PI / points_on_circle * point_index;
            Matrix4d rotation = Matrix4d.Rotate(direction, angle);
            Vector3d shift_direction = Vector3d.Transform(shift_dir, rotation);
            return shift_direction * distance;
        }

        protected Vector3d GetShift_CircleJitter (double max_shift)
        {
            if (MT.rank == 0)
                return Vector3d.Zero;

            int num_circles = (MT.total - 2) / POINTS_ON_CIRCLE + 1;
            double jitter = 1.0 / (num_circles + 1);
            return GetShift_Circle(max_shift) + GetShift_RandomUniform(max_shift * jitter);
        }
    }

    public class AnimatableDoFCamera : DoFCamera, IAnimatableCamera
    {
        public virtual AnimatedCamera.Parameter[] GetParams ()
        {
            return new AnimatedCamera.Parameter[] {
                new AnimatedCamera.Parameter("position", AnimatedCamera.Parsers.ParseVector3, AnimatedCamera.Interpolators.Catmull_Rom, true),
                new AnimatedCamera.Parameter("direction", AnimatedCamera.Parsers.ParseVector3, AnimatedCamera.Interpolators.Catmull_Rom, true),
                new AnimatedCamera.Parameter("angle", AnimatedCamera.Parsers.ParseDouble, AnimatedCamera.Interpolators.LERP),
                new AnimatedCamera.Parameter("max_shift", AnimatedCamera.Parsers.ParseDouble, AnimatedCamera.Interpolators.LERP),
                new AnimatedCamera.Parameter("focus", AnimatedCamera.Parsers.ParseDouble, AnimatedCamera.Interpolators.LERP)
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
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException("Invalid camera script or error when loading it.");
            }
            prepare();
        }
    }
}
