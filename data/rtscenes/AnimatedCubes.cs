using System.Collections.Generic;
using DavidSosvald_MichalTopfer;

// Optional IImageFunction.
if (context != null)
{
  context["Start"] = 0.0;
  context["End"]   = 20.0;
  context["Algorithm"] = new RayTracing(scene);
}

// CSG scene:
CSGInnerNode root = new CSGInnerNode(SetOperation.Union);
root.SetAttribute(PropertyName.REFLECTANCE_MODEL, new PhongModel());
root.SetAttribute(PropertyName.MATERIAL, new PhongMaterial(new double[] { 1.0, 0.6, 0.1 }, 0.1, 0.8, 0.2, 16));
scene.Intersectable = root;

// Background color:
scene.BackgroundColor = new double[] { 0.0, 0.05, 0.07 };

// Light sources:
scene.Sources = new LinkedList<ILightSource>();
scene.Sources.Add(new AmbientLightSource(0.8));
scene.Sources.Add(new PointLightSource(new Vector3d(-5.0, 3.0, -3.0), 1.0));

// --- NODE DEFINITIONS ----------------------------------------------------

// Params dictionary:
Dictionary<string, string> p = Util.ParseKeyValueList(param);

// n = <index-of-refraction>
double n = 1.6;
Util.TryParse(p, "n", ref n);

// mat = {mirror|glass|diffuse}
PhongMaterial pm = new PhongMaterial(new double[] { 1.0, 0.6, 0.1 }, 0.1, 0.8, 0.2, 16);
PhongMaterial r = new PhongMaterial(new double[] { 0.8, 0.1, 0.1 }, 0.1, 0.8, 0.2, 16);
PhongMaterial g = new PhongMaterial(new double[] { 0.1, 1.0, 0.2 }, 0.1, 0.8, 0.2, 16);

string mat;
if (p.TryGetValue("mat", out mat))
switch (mat)
{
    case "mirror":
        pm = new PhongMaterial(new double[] { 1.0, 1.0, 0.8 }, 0.0, 0.1, 0.9, 128);
        break;

    case "glass":
        pm = new PhongMaterial(new double[] { 0.0, 0.2, 0.1 }, 0.05, 0.05, 0.1, 128);
        pm.n = n;
        pm.Kt = 0.9;
        break;
}

// Base plane
Plane pl = new Plane();
pl.SetAttribute(PropertyName.COLOR, new double[] { 0.6, 0.0, 0.0 });
pl.SetAttribute(PropertyName.TEXTURE, new CheckerTexture(0.5, 0.5, new double[] { 1.0, 1.0, 1.0 }));
root.InsertChild(pl, Matrix4d.RotateX(-MathHelper.PiOver2) * Matrix4d.CreateTranslation(0.0, -1.0, 0.0));

// Cubes
Cube c;

// animated cube
c = new Cube();
AnimatableISceneNode movingCube = new AnimatableISceneNode(c, "t1", null, "s1");
root.InsertChild(c, Matrix4d.Identity);
c.SetAttribute(PropertyName.MATERIAL, r);

c = new Cube();
AnimatableISceneNode movingCube2 = new AnimatableISceneNode(c, null, "r1", null, new Vector3d(-5, -0.5, 0));
root.InsertChild(c, Matrix4d.Identity);
c.SetAttribute(PropertyName.MATERIAL, g);

c = new Cube();
AnimatableISceneNode movingCube3 = new AnimatableISceneNode(c, null, "r1", null, new Vector3d(6, -0.5, 0));
root.InsertChild(c, Matrix4d.Identity);
c.SetAttribute(PropertyName.MATERIAL, g);


// front row:
c = new Cube();
root.InsertChild(c, Matrix4d.RotateY(0.6) * Matrix4d.CreateTranslation(-3.5, -0.8, 0.0));
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateY(1.2) * Matrix4d.CreateTranslation(-1.5, -0.8, 0.0));
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateY(1.8) * Matrix4d.CreateTranslation(0.5, -0.8, 0.0));
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateY(2.4) * Matrix4d.CreateTranslation(2.5, -0.8, 0.0));
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateY(3.0) * Matrix4d.CreateTranslation(4.5, -0.8, 0.0));
c.SetAttribute(PropertyName.MATERIAL, pm);
// back row:
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX(3.5) * Matrix4d.CreateTranslation(-4.0, 1.0, 2.0));
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX(3.0) * Matrix4d.CreateTranslation(-2.5, 1.0, 2.0));
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX(2.5) * Matrix4d.CreateTranslation(-1.0, 1.0, 2.0));
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX(2.0) * Matrix4d.CreateTranslation(0.5, 1.0, 2.0));
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX(1.5) * Matrix4d.CreateTranslation(2.0, 1.0, 2.0));
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX(1.0) * Matrix4d.CreateTranslation(3.5, 1.0, 2.0));
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX(0.5) * Matrix4d.CreateTranslation(5.0, 1.0, 2.0));
c.SetAttribute(PropertyName.MATERIAL, pm);

// Camera:
scene.Camera = new CameraAnimator(new AnimatableStaticCamera(), new IAnimatable[] { movingCube, movingCube2, movingCube3 }, "..\\..\\..\\data\\rtscenes\\AnimatedCubes.yaml");
if (scene is ITimeDependent s)
    s.End = (scene.Camera as ITimeDependent).End;
if (context != null)
{
    context["Start"] = (scene.Camera as ITimeDependent).Start;
    context["End"] = (scene.Camera as ITimeDependent).End;
}
