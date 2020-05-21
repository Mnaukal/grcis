using OpenTK;
using Rendering;
using System;
using System.Collections.Generic;

namespace DavidSosvald_MichalTopfer
{
    public class KeyframesAnimatedDoFCamera : DoFCamera, ITimeDependent
        //, IVertigoInnerCamera // uncomment to use with the VertigoEffectCamera extension
    {
        private readonly string positionParamName;
        private readonly string directionParamName;
        private readonly string angleParamName;
        private readonly string focusParamName;
        private readonly string focusPointParamName;
        private readonly string maxShiftParamName;

        public KeyframesAnimatedDoFCamera (Animator animator, string positionParamName = "position", string directionParamName = "direction", string angleParamName = "angle", string focusParamName = "focus", string focusPointParamName = "focus_point", string maxShiftParamName = "max_shift") : this(positionParamName, directionParamName, angleParamName, focusParamName, focusPointParamName, maxShiftParamName)
        {
            animator?.RegisterParams(GetParams());
        }

        private KeyframesAnimatedDoFCamera (string positionParamName, string directionParamName, string angleParamName, string focusParamName, string focusPointParamName, string maxShiftParamName)
        {
            this.positionParamName = positionParamName;
            this.directionParamName = directionParamName;
            this.angleParamName = angleParamName;
            this.focusParamName = focusParamName;
            this.focusPointParamName = focusPointParamName;
            this.maxShiftParamName = maxShiftParamName;
        }

        public IEnumerable<Animator.Parameter> GetParams ()
        {
            return new Animator.Parameter[] {
                new Animator.Parameter(positionParamName, Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true),
                new Animator.Parameter(directionParamName, Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true),
                new Animator.Parameter(angleParamName, Animator.Parsers.ParseDouble, Animator.Interpolators.LERP),
                new Animator.Parameter(maxShiftParamName, Animator.Parsers.ParseDouble, Animator.Interpolators.LERP),
                new Animator.Parameter(focusParamName, Animator.Parsers.ParseDouble, Animator.Interpolators.LERP),
                new Animator.Parameter(focusPointParamName, Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom),
            };
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

        void SetTime (double time)
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
                if (p.ContainsKey(maxShiftParamName))
                    max_shift = (double)p[maxShiftParamName];
                if (p.ContainsKey(focusParamName))
                    focus_distance = (double)p[focusParamName];
                else if (p.ContainsKey(focusPointParamName))
                    SetFocusPoint((Vector3d)p[focusPointParamName]);
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException("Invalid camera script or error when loading it.");
            }
            prepare();
        }

        public object Clone ()
        {
            KeyframesAnimatedDoFCamera c = new KeyframesAnimatedDoFCamera(positionParamName, directionParamName, angleParamName, focusParamName, focusPointParamName, maxShiftParamName);
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
