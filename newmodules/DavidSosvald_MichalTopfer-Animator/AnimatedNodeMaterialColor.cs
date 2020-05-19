using System.Collections.Generic;
using Rendering;

namespace DavidSosvald_MichalTopfer
{
    public class AnimatedNodeMaterialColor : AnimatedCSGInnerNode
    {
        string colorParamName;

        public AnimatedNodeMaterialColor (Animator animator, string colorParamName = "color") : base(SetOperation.Union)
        { 
            this.colorParamName = colorParamName;
            animator?.RegisterParams(GetParams());
        }

        public IEnumerable<Animator.Parameter> GetParams ()
        {
            return new Animator.Parameter[] {
                new Animator.Parameter(colorParamName, Animator.Parsers.ParseDoubleArray, Animator.Interpolators.Catmull_Rom, true)
            };
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
            double[] color = (double[])p[colorParamName];
            IMaterial material = (IMaterial)GetAttribute(PropertyName.MATERIAL);
            material.Color = (double[])color.Clone();
        }

        public override object Clone ()
        {
            AnimatedNodeMaterialColor a = new AnimatedNodeMaterialColor(null, colorParamName);
            a.Start = Start;
            a.End = End;
            ShareCloneAttributes(a);
            ShareCloneChildren(a);
            a.Time = time;
            return a;
        }
    }
}
