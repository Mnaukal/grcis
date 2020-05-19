using System.Collections.Generic;
using OpenTK;
using Rendering;

namespace DavidSosvald_MichalTopfer
{
    public class AnimatedNodeTransform : AnimatedCSGInnerNode
    {
        string translationParamName, rotationParamName, scaleParamName;
        Vector3d defaultTranslation, defaultRotation, defaultScale;

        public AnimatedNodeTransform (Animator animator, string translationParamName = null, string rotationParamName = null, string scaleParamName = null, Vector3d? defaultTranslation = null, Vector3d? defaultRotation = null, Vector3d? defaultScale = null) : base(SetOperation.Union)
        {
            this.translationParamName = translationParamName;
            this.rotationParamName = rotationParamName;
            this.scaleParamName = scaleParamName;
            this.defaultTranslation = defaultTranslation ?? Vector3d.Zero;
            this.defaultRotation = defaultRotation ?? Vector3d.Zero;
            this.defaultScale = defaultScale ?? Vector3d.One;
            animator?.RegisterParams(GetParams());
        }

        public IEnumerable<Animator.Parameter> GetParams ()
        {
            List<Animator.Parameter> p = new List<Animator.Parameter>();

            if (translationParamName != null)
                p.Add(new Animator.Parameter(translationParamName, Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true));
            if (rotationParamName != null)
                p.Add(new Animator.Parameter(rotationParamName, Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true));
            if (scaleParamName != null)
                p.Add(new Animator.Parameter(scaleParamName, Animator.Parsers.ParseVector3, Animator.Interpolators.Catmull_Rom, true));
            return p;
        }

        protected override void setTime (double time)
        {
            if (MT.scene == null)
                return;
            Dictionary<string, object> p = ((Animator)MT.scene.Animator).getParams(time);
            ApplyParams(p);

            base.setTime(time);
        }

        public void ApplyParams (Dictionary<string, object> p)
        {
            Vector3d translation = translationParamName != null ? (Vector3d)p[translationParamName] : defaultTranslation;
            Vector3d rotation = rotationParamName != null ? (Vector3d)p[rotationParamName] : defaultRotation;
            Vector3d scale = scaleParamName != null ? (Vector3d)p[scaleParamName] : defaultScale;

            ToParent = Matrix4d.Scale(scale) * Matrix4d.Rotate(Quaterniond.FromEulerAngles(rotation)) * Matrix4d.CreateTranslation(translation);
            FromParent = ToParent.Inverted();
        }

        public override object Clone()
        {
            AnimatedNodeTransform a = new AnimatedNodeTransform(null, translationParamName, rotationParamName, scaleParamName, defaultTranslation, defaultRotation, defaultScale);
            a.Start = Start;
            a.End = End;
            ShareCloneAttributes(a);
            ShareCloneChildren(a);
            a.Time = time;
            return a;
        }
    }
}
