using System.Collections.Generic;
using System.IO;
using DavidSosvald_MichalTopfer;

// Optional IImageFunction.
if (outParam != null)
{
    outParam["Algorithm"] = new RayTracing(scene);
}

// Params dictionary:
Dictionary<string, string> p = Util.ParseKeyValueList(param);

// CSG scene:
CSGInnerNode root = new CSGInnerNode(SetOperation.Union);
root.SetAttribute(PropertyName.REFLECTANCE_MODEL, new PhongModel() );
root.SetAttribute(PropertyName.MATERIAL, new PhongMaterial( new double[] { 1.0, 0.6, 0.1 }, 0.2, 1.0, 0.1, 16 ) );
scene.Intersectable = root;

// Background color:
scene.BackgroundColor = new double[] { 0.7, 0.7, 0.7 };

// Camera:
/*double f = 6, s = 0.1;
Util.TryParse(p, "focus", ref f);
Util.TryParse(p, "shift", ref s);
scene.Camera = new DoFCamera(new Vector3d(-6.0, 1.0, 0.0),
                             new Vector3d(1.0, 0.1, 0.0),
                             70.0, Vector3d.Zero, s);*/
string script_file = Path.Combine(Path.GetDirectoryName(scriptFileName), "dof_spheres.yaml");
scene.Camera = new AnimatedCamera(new VertigoEffectCamera(new AnimatableDoFCamera(), true, true, 8), script_file);
if (scene is ITimeDependent s)
    s.End = (scene.Camera as AnimatedCamera).End;
if (outParam != null)
{
    outParam["Start"] =  (scene.Camera as AnimatedCamera).Start;
    outParam["End"] = (scene.Camera as AnimatedCamera).End;
}

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
