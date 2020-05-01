using OpenTK;
using System;

namespace Rendering
{
    public class DoFCamera : StaticCamera, ICamera
    {
        protected double focus_distance;
        protected double max_shift;

        public DoFCamera(Vector3d cen, Vector3d dir, double ang, double focus_distance = 1.0, double max_shift = 0.1) : base(cen, dir, ang)
        {
            this.focus_distance = focus_distance;
            this.max_shift = max_shift;
        }

        public new bool GetRay (double x, double y, out Vector3d from, out Vector3d direction)
        {
            bool ret = base.GetRay(x, y, out Vector3d orig_from, out Vector3d orig_direction);

            Vector3d focus = orig_from + focus_distance * orig_direction;
            from = orig_from + max_shift * new Vector3d(MT.rnd.UniformNumber(), MT.rnd.UniformNumber(), MT.rnd.UniformNumber());
            direction = focus - from;
            direction.Normalize();

            return ret;
        }

    }
}
