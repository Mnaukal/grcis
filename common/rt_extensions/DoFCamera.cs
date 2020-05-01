using OpenTK;
using System;

namespace Rendering
{
    public class DoFCamera : StaticCamera, ICamera
    {
        public enum RandomMode { Normal, Uniform };

        protected double focus_distance;
        protected double max_shift;
        protected RandomMode random_mode;

        public DoFCamera(Vector3d cen, Vector3d dir, double ang, double focus_distance = 1.0, double max_shift = 0.05, RandomMode random_mode = RandomMode.Uniform) : base(cen, dir, ang)
        {
            this.focus_distance = focus_distance;
            this.max_shift = max_shift;
            this.random_mode = random_mode;
        }

        public new bool GetRay (double x, double y, out Vector3d from, out Vector3d direction)
        {
            bool ret = base.GetRay(x, y, out Vector3d orig_from, out Vector3d orig_direction);

            Vector3d focus = orig_from + focus_distance * orig_direction;
            Vector3d shift;
            switch (random_mode)
            {
                case RandomMode.Uniform:
                    shift = max_shift * new Vector3d(MT.rnd.UniformNumber() - 0.5, MT.rnd.UniformNumber() - 0.5, MT.rnd.UniformNumber() - 0.5);
                    break;
                case RandomMode.Normal:
                default:
                    shift = max_shift * new Vector3d(MT.rnd.Normal(0, 1), MT.rnd.Normal(0, 1), MT.rnd.Normal(0, 1));
                    break;
            }
            from = orig_from + shift;
            direction = focus - from;
            direction.Normalize();

            return ret;
        }

    }
}
