using System.Collections.Generic;
using System.Linq;
using System.IO;
using DavidSosvald_MichalTopfer;
using JosefPelikan;

Debug.Assert(scene != null);
Debug.Assert(scene is ITimeDependent);
Debug.Assert(context != null);

//////////////////////////////////////////////////
// CSG scene

Animator a; // 'a' is used to register params (names, parsers, interpolators) during scene creation
if (context.ContainsKey("animator")) {
    scene.Animator = (ITimeDependent) ((Animator) context["animator"]).Clone();
    a = null; // params were already registered when Animator was created (scene is the same)
}
else {
    string keyframes_file = Path.Combine(Path.GetDirectoryName((string)context[PropertyName.CTX_SCRIPT_PATH]), "animation.yaml");
    a = new Animator(keyframes_file);
    scene.Animator = a;
    context["animator"] = a;
}

AnimatedCSGInnerNode root = new AnimatedCSGInnerNode(SetOperation.Union);
root.SetAttribute(PropertyName.REFLECTANCE_MODEL, new PhongModel());
root.SetAttribute(PropertyName.MATERIAL, new PhongMaterial(new double[] { 1.0, 0.6, 0.1 }, 0.1, 0.8, 0.2, 16));
scene.Intersectable = root;

// Background color:
scene.BackgroundColor = new double[] { 0.02, 0.04, 0.06 };
scene.Background = new StarBackground(scene.BackgroundColor, 1000, 0.02, 0.5, 4.0, 2.5, 42);

// Light sources:
scene.Sources = new LinkedList<ILightSource>();
scene.Sources.Add(new AmbientLightSource(0.8));
scene.Sources.Add(new PointLightSource(new Vector3d(-5.0, 4.0, 3.0), 1.0));

// Camera:
scene.Camera = new KeyframesAnimatedDoFCamera(a);

// --- NODE DEFINITIONS ----------------------------------------------------

// Params dictionary:
Dictionary<string, string> p = Util.ParseKeyValueList(param);

// materials
var darkBlue = new double[] { 0.0, 0.309, 0.619 };
var purple = new double[] { 0.211, 0.0, 0.529 };
var darkRed = new double[] { 0.623, 0.184, 0.184 };

PhongMaterial r = new PhongMaterial(darkBlue, 0.6, 0.4, 0.1, 16);
PhongMaterial g = new PhongMaterial(purple, 0.6, 0.4, 0.1, 16);
PhongMaterial y = new PhongMaterial(darkRed, 0.6, 0.4, 0.1, 16);

private PhongMaterial GetMaterial(int j)
{
    switch (j % 3)
    {
        case 0:
            return r;
        case 1:
            return g;
        case 2:
            return y;
    }
    return null;
}

// Cubes
for (int i = 0; i < 5; i += 1)
{
    Cube c;
    AnimatedNodeTransform ac = new AnimatedNodeTransform(a, "p" + i, "r1", "s1");

    int CUBES = 9;
    double angle = Math.PI * 2 / CUBES;

    
    for (int j = 0; j <= CUBES; j += 1)
    {
        c = new Cube();
        ac.InsertChild(c, Matrix4d.CreateTranslation(0.0, 0.0, -4.0) * Matrix4d.CreateRotationX(angle * j));
        c.SetAttribute(PropertyName.MATERIAL, GetMaterial(j));
    }

    root.InsertChild(ac, Matrix4d.Identity);
}

AnimatedNodeTransform mono = new AnimatedNodeTransform(a, "p_mono", "r_mono", null);
Cube monolith = new Cube();
monolith.SetAttribute(PropertyName.MATERIAL, new PhongMaterial(new double[] { 0, 0, 0 }, 0.06, 0.06, 0.12, 128));
mono.InsertChild(monolith, Matrix4d.Scale(0.4, 3.6, 1.6) * Matrix4d.CreateTranslation(-0.2, -1.8, -0.8));
root.InsertChild(mono, Matrix4d.Identity);

//////////////////////////////////////////////////
// If animator was created in this run of script, load keyframes based on parameters set during scene creation
if (a != null) {
    a.LoadKeyframes();
    context[PropertyName.CTX_START_ANIM] = a.Start;
    context[PropertyName.CTX_END_ANIM] = a.End;
    //context[PropertyName.CTX_FPS] = 25.0;
}
