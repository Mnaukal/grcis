using System.Collections.Generic;
using System.IO;
using DavidSosvald_MichalTopfer;

Debug.Assert(scene != null);
Debug.Assert(scene is ITimeDependent);
Debug.Assert(context != null);

Animator a; // 'a' is used to register params (names, parsers, interpolators) during scene creation
if (context.ContainsKey("animator")) {
    scene.Animator = (ITimeDependent)((Animator) context["animator"]).Clone();
    a = null; // params were already registered when Animator was created (scene is the same)
}
else {
    string keyframes_file = Path.Combine(Path.GetDirectoryName((string)context[PropertyName.CTX_SCRIPT_PATH]), "VertigoExample.yaml");
    a = new Animator(keyframes_file);
    scene.Animator = a;
    context["animator"] = a;
}

// CSG scene:
CSGInnerNode root = new CSGInnerNode(SetOperation.Union);
root.SetAttribute(PropertyName.REFLECTANCE_MODEL, new PhongModel() );
root.SetAttribute(PropertyName.MATERIAL, new PhongMaterial( new double[] { 1.0, 0.6, 0.1 }, 0.2, 1.0, 0.1, 16 ) );
scene.Intersectable = root;

// Background color:
scene.BackgroundColor = new double[] { 0.7, 0.7, 0.7 };

// Camera:
scene.Camera = new VertigoEffectCamera(new KeyframesAnimatedStaticCamera(a), a, 8);

// Light sources:
scene.Sources = new LinkedList<ILightSource>();
scene.Sources.Add( new AmbientLightSource( 0.8 ) );
scene.Sources.Add( new PointLightSource( new Vector3d( -4.0, 10.0, 10.0 ), 2.5 ) );

// --- NODE DEFINITIONS ----------------------------------------------------

// Materials
PhongMaterial glossy_pink = new PhongMaterial(new double[] { 1.0, 0.44, 0.37 }, 0.4, 0.5, 0.07, 64);
PhongMaterial glossy_magenta = new PhongMaterial(new double[] { 1.0, 0.3, 0.8 }, 0.4, 0.5, 0.07, 64);
PhongMaterial yellow = new PhongMaterial(new double[] { 1.0, 0.95, 0.45 }, 0.4, 0.5, 0.0, 16);
PhongMaterial glossy_red = new PhongMaterial(new double[] { 1.0, 0.3, 0.2 }, 0.4, 0.5, 0.07, 64);
PhongMaterial white_diffuse = new PhongMaterial(new double[] { 1.0, 1.0, 1.0 }, 0.4, 0.6, 0.0, 16);

// Base plane
Plane plane = new Plane();
plane.SetAttribute(PropertyName.MATERIAL, white_diffuse);
root.InsertChild(plane, Matrix4d.RotateX( -MathHelper.PiOver2 ) * Matrix4d.CreateTranslation( 0.0, 0.0, 0.0 ) );

// Cubes
Sphere sphere;
// front row:
sphere = new Sphere();
sphere.SetAttribute(PropertyName.MATERIAL, glossy_pink);
root.InsertChild(sphere, Matrix4d.Scale(2) * Matrix4d.CreateTranslation(0, 2, 0));

sphere = new Sphere();
sphere.SetAttribute(PropertyName.MATERIAL, yellow);
root.InsertChild(sphere, Matrix4d.Scale(0.4) * Matrix4d.CreateTranslation(-3, 0.4, 1));

sphere = new Sphere();
sphere.SetAttribute(PropertyName.MATERIAL, glossy_magenta);
root.InsertChild(sphere, Matrix4d.Scale(4) * Matrix4d.CreateTranslation(4, 4, -6.5));

sphere = new Sphere();
sphere.SetAttribute(PropertyName.MATERIAL, glossy_pink);
root.InsertChild(sphere, Matrix4d.Scale(0.7) * Matrix4d.CreateTranslation(-3.5, 0.7, -1.7));

sphere = new Sphere();
sphere.SetAttribute(PropertyName.MATERIAL, glossy_red);
root.InsertChild(sphere, Matrix4d.Scale(0.8) * Matrix4d.CreateTranslation(-2, 0.8, 2.7));

sphere = new Sphere();
sphere.SetAttribute(PropertyName.MATERIAL, glossy_red);
root.InsertChild(sphere, Matrix4d.Scale(0.5) * Matrix4d.CreateTranslation(-6, 0.5, 2));

sphere = new Sphere();
sphere.SetAttribute(PropertyName.MATERIAL, yellow);
root.InsertChild(sphere, Matrix4d.Scale(3) * Matrix4d.CreateTranslation(3, 3, 4));

// If animator was created in this run of script, load keyframes based on parameters set during scene creation
if (a != null) {
    a.LoadKeyframes();
    context[PropertyName.CTX_START_ANIM] = a.Start;
    context[PropertyName.CTX_END_ANIM] = a.End;
    context[PropertyName.CTX_FPS] = 25.0;
}
