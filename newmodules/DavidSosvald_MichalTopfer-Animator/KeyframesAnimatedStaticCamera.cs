using System;
using System.Collections.Generic;
using OpenTK;
using Rendering;

namespace DavidSosvald_MichalTopfer
{
    public class KeyframesAnimatedStaticCamera : StaticCamera, ITimeDependent,
        IVertigoInnerCamera // delete this line if you don't use the VertigoEffectCamera extension
    {
        public double Start { get; set; }
        public double End { get; set; }
        private double time;
        public double Time {
            get => time;
            set {
                time = value;
                SetTime(value);
            }
        }

        private readonly string positionParamName;
        private readonly string directionParamName;
        private readonly string angleParamName;

        public KeyframesAnimatedStaticCamera(Animator animator, string positionParamName = "position", string directionParamName = "direction", string angleParamName = "angle") : this(positionParamName, directionParamName, angleParamName)
        {   
            animator?.RegisterParams(GetParams());
        }

        private KeyframesAnimatedStaticCamera(string positionParamName, string directionParamName, string angleParamName)
        {
            this.positionParamName = positionParamName;
            this.directionParamName = directionParamName;
            this.angleParamName = angleParamName;
        }

        public IEnumerable<Animator.Parameter> GetParams ()
        {
            return new Animator.Parameter[] {
                new Animator.Parameter(positionParamName, Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true),
                new Animator.Parameter(directionParamName, Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true),
                new Animator.Parameter(angleParamName, Animator.Parsers.ParseDouble, Animator.Interpolators.LERP)
            };
        }

        void SetTime(double time)
        {
            if (MT.scene == null)
                return;
            Dictionary<string, object> p = ((Animator)MT.scene.Animator).getParams(time);
            ApplyParams(p);
        }

        void ApplyParams (Dictionary<string, object> p)
        {
            try
            {
                center = (Vector3d)p[positionParamName];
                direction = (Vector3d)p[directionParamName];
                if (p.ContainsKey(angleParamName))
                    hAngle = MathHelper.DegreesToRadians((double)p[angleParamName]);
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException("Invalid camera script or error when loading it.");
            }
            prepare();
        }

        public object Clone ()
        {
            KeyframesAnimatedStaticCamera c = new KeyframesAnimatedStaticCamera(positionParamName, directionParamName, angleParamName);
            c.width = width;
            c.height = height;
            c.center = center;
            c.direction = direction;
            c.up = up;
            c.hAngle = hAngle;
            c.prepare();
            c.Start = Start;
            c.End = End;
            c.Time = Time;
            return c;
        }

#if DEBUG
        private static volatile int nextSerial = 0;
        private readonly int serial = nextSerial++;
        public int getSerial () => serial;
#endif

        public Vector3d GetPosition () => center;

        public double GetAngle () => hAngle;

        public void SetAngle (double angle)
        {
            hAngle = angle;
            prepare();
        }

        public Vector3d GetDirection () => direction;

        public void SetDirection (Vector3d direction)
        {
            this.direction = direction;
            prepare();
        }
    }
}
