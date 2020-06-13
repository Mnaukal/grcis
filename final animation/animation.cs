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
scene.BackgroundColor = new double[] { 0.0, 0.01, 0.03 };
scene.Background = new StarBackground(scene.BackgroundColor, 1000, 0.02, 0.5, 4.0, 2.5, 42);

// Light sources:
scene.Sources = new LinkedList<ILightSource>();
scene.Sources.Add(new AmbientLightSource(0.8));
scene.Sources.Add(new PointLightSource(new Vector3d(-5.0, 0.0, 0.0), 1.0));

// Camera:
scene.Camera = new KeyframesAnimatedDoFCamera(a);

// --- NODE DEFINITIONS ----------------------------------------------------

// Params dictionary:
Dictionary<string, string> p = Util.ParseKeyValueList(param);

// materials
var red = new double[] { 0.8, 0.1, 0.1 };
var green = new double[] { 0.1, 1.0, 0.2 };
var yellow = new double[] { 0.9, 0.7, 0.1 };

PhongMaterial r = new PhongMaterial(red, 0.1, 0.8, 0.1, 16);
PhongMaterial g = new PhongMaterial(green, 0.1, 0.8, 0.1, 16);
PhongMaterial y = new PhongMaterial(yellow, 0.1, 0.8, 0.1, 16);

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

// Base plane
//Plane pl = new Plane();
//pl.SetAttribute(PropertyName.COLOR, new double[] { 0.6, 0.0, 0.0 });
//pl.SetAttribute(PropertyName.TEXTURE, new CheckerTexture(0.5, 0.5, new double[] { 1.0, 1.0, 1.0 }));
//root.InsertChild(pl, Matrix4d.RotateX(-MathHelper.PiOver2) * Matrix4d.CreateTranslation(0.0, -1.0, 0.0));

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




//c = new Cube();
//ac = new AnimatedNodeTransform(a, null, null, null);
//ac.InsertChild(c, Matrix4d.CreateTranslation(10.0, 6.0, -5.0));
//root.InsertChild(ac, Matrix4d.Identity);
//c.SetAttribute(PropertyName.MATERIAL, r);

//c = new Cube();
//ac = new AnimatedNodeTransform(a, null, null, null);
//ac.InsertChild(c, Matrix4d.CreateTranslation(10.0, 4.0, 4.0));
//root.InsertChild(ac, Matrix4d.Identity);
//c.SetAttribute(PropertyName.MATERIAL, g);






//// animated cubes
//c = new Cube();
//ac = new AnimatedNodeTransform(a, "t1", null, "s1");
//ac.InsertChild(c, Matrix4d.Identity);
//root.InsertChild(ac, Matrix4d.Identity);
//c.SetAttribute(PropertyName.MATERIAL, r);

//c = new Cube();
//ac = new AnimatedNodeTransform(a, null, "r1", null, new Vector3d(-5, -0.5, 0));
//ac.InsertChild(c, Matrix4d.Identity);
//root.InsertChild(ac, Matrix4d.Identity);
//c.SetAttribute(PropertyName.MATERIAL, g);

//c = new Cube();
//ac = new AnimatedNodeTransform(a, null, "r1", null, new Vector3d(6, -0.5, 0));
//ac.InsertChild(c, Matrix4d.Identity);
//root.InsertChild(ac, Matrix4d.Identity);
//c.SetAttribute(PropertyName.MATERIAL, g);

//// still cubes
//// front row - with animated material
//AnimatedNodeMaterialColor front = new AnimatedNodeMaterialColor(a);
//root.InsertChild(front, Matrix4d.Identity);
//PhongMaterial m = new PhongMaterial(new double[] { 0, 0, 0 }, 0.1, 0.8, 0.1, 16);
//front.SetAttribute(PropertyName.MATERIAL, m);

//// the cubes are copied from the Cubes scene
//c = new Cube();
//front.InsertChild(c, Matrix4d.RotateY(0.6) * Matrix4d.CreateTranslation(-3.5, -0.8, 0.0));
//c = new Cube();
//front.InsertChild(c, Matrix4d.RotateY(1.2) * Matrix4d.CreateTranslation(-1.5, -0.8, 0.0));
//c = new Cube();
//front.InsertChild(c, Matrix4d.RotateY(1.8) * Matrix4d.CreateTranslation(0.5, -0.8, 0.0));
//c = new Cube();
//front.InsertChild(c, Matrix4d.RotateY(2.4) * Matrix4d.CreateTranslation(2.5, -0.8, 0.0));
//c = new Cube();
//front.InsertChild(c, Matrix4d.RotateY(3.0) * Matrix4d.CreateTranslation(4.5, -0.8, 0.0));

//// back row - not animated
//c = new Cube();
//root.InsertChild(c, Matrix4d.RotateX(3.5) * Matrix4d.CreateTranslation(-4.0, 1.0, 2.0));
//c.SetAttribute(PropertyName.MATERIAL, pm);
//c = new Cube();
//root.InsertChild(c, Matrix4d.RotateX(3.0) * Matrix4d.CreateTranslation(-2.5, 1.0, 2.0));
//c.SetAttribute(PropertyName.MATERIAL, pm);
//c = new Cube();
//root.InsertChild(c, Matrix4d.RotateX(2.5) * Matrix4d.CreateTranslation(-1.0, 1.0, 2.0));
//c.SetAttribute(PropertyName.MATERIAL, pm);
//c = new Cube();
//root.InsertChild(c, Matrix4d.RotateX(2.0) * Matrix4d.CreateTranslation(0.5, 1.0, 2.0));
//c.SetAttribute(PropertyName.MATERIAL, pm);
//c = new Cube();
//root.InsertChild(c, Matrix4d.RotateX(1.5) * Matrix4d.CreateTranslation(2.0, 1.0, 2.0));
//c.SetAttribute(PropertyName.MATERIAL, pm);
//c = new Cube();
//root.InsertChild(c, Matrix4d.RotateX(1.0) * Matrix4d.CreateTranslation(3.5, 1.0, 2.0));
//c.SetAttribute(PropertyName.MATERIAL, pm);
//c = new Cube();
//root.InsertChild(c, Matrix4d.RotateX(0.5) * Matrix4d.CreateTranslation(5.0, 1.0, 2.0));
//c.SetAttribute(PropertyName.MATERIAL, pm);

//////////////////////////////////////////////////
// If animator was created in this run of script, load keyframes based on parameters set during scene creation
if (a != null) {
    a.LoadKeyframes();
    context[PropertyName.CTX_START_ANIM] = a.Start;
    context[PropertyName.CTX_END_ANIM] = a.End;
    context[PropertyName.CTX_FPS] = 25.0;
}
