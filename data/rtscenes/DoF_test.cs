using System.Collections.Generic;
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
root.SetAttribute(PropertyName.MATERIAL, new PhongMaterial( new double[] { 1.0, 0.6, 0.1 }, 0.1, 0.8, 0.2, 16 ) );
scene.Intersectable = root;

// Background color:
scene.BackgroundColor = new double[] { 0.0, 0.05, 0.07 };

// Camera:
double f = 4, s = 0.05;
Util.TryParse(p, "focus", ref f);
Util.TryParse(p, "shift", ref s);
scene.Camera = new DoFCamera( new Vector3d( 7, 0.6,  0 ),
                           new Vector3d( -1.0, -0.1, 0.0 ),
                           50.0, f, s);

// Light sources:
scene.Sources = new LinkedList<ILightSource>();
scene.Sources.Add( new AmbientLightSource( 0.8 ) );
scene.Sources.Add( new PointLightSource( new Vector3d( -5.0, 3.0, -3.0 ), 1.0 ) );

// --- NODE DEFINITIONS ----------------------------------------------------

      

// n = <index-of-refraction>
double n = 1.6;
Util.TryParse(p, "n", ref n );

// mat = {mirror|glass|diffuse}
PhongMaterial pm = new PhongMaterial(new double[] { 1.0, 0.6, 0.1 }, 0.1, 0.8, 0, 16);
string mat;
if (p.TryGetValue( "mat", out mat ) )
    switch (mat )
    {
        case "mirror":
        pm = new PhongMaterial( new double[] { 1.0, 1.0, 0.8 }, 0.0, 0.1, 0.9, 128 );
        break;

        case "glass":
        pm = new PhongMaterial( new double[] { 0.0, 0.2, 0.1 }, 0.05, 0.05, 0.1, 128 );
        pm.n = n;
        pm.Kt = 0.9;
        break;
    }

// Base plane
Plane pl = new Plane();
pl.SetAttribute(PropertyName.COLOR, new double[] { 0.6, 0.0, 0.0 } );
pl.SetAttribute(PropertyName.TEXTURE, new CheckerTexture( 0.5, 0.5, new double[] { 1.0, 1.0, 1.0 } ) );
root.InsertChild(pl, Matrix4d.RotateX( -MathHelper.PiOver2 ) * Matrix4d.CreateTranslation( 0.0, -1.0, 0.0 ) );

// Cubes
Cube c;
// front row:
c = new Cube();
root.InsertChild(c, Matrix4d.RotateY( 0.6 ) * Matrix4d.CreateTranslation( -3.5, -0.8, 0.0 ) );
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateY( 1.2 ) * Matrix4d.CreateTranslation( -1.5, -0.8, 0.0 ) );
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateY( 1.8 ) * Matrix4d.CreateTranslation( 0.5, -0.8, 0.0 ) );
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateY( 2.4 ) * Matrix4d.CreateTranslation( 2.5, -0.8, 0.0 ) );
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateY( 3.0 ) * Matrix4d.CreateTranslation( 4.5, -0.8, 0.0 ) );
c.SetAttribute(PropertyName.MATERIAL, pm);
// back row:
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX( 3.5 ) * Matrix4d.CreateTranslation( -4.0, 1.0, 2.0 ) );
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX( 3.0 ) * Matrix4d.CreateTranslation( -2.5, 1.0, 2.0 ) );
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX( 2.5 ) * Matrix4d.CreateTranslation( -1.0, 1.0, 2.0 ) );
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX( 2.0 ) * Matrix4d.CreateTranslation( 0.5, 1.0, 2.0 ) );
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX( 1.5 ) * Matrix4d.CreateTranslation( 2.0, 1.0, 2.0 ) );
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX( 1.0 ) * Matrix4d.CreateTranslation( 3.5, 1.0, 2.0 ) );
c.SetAttribute(PropertyName.MATERIAL, pm);
c = new Cube();
root.InsertChild(c, Matrix4d.RotateX( 0.5 ) * Matrix4d.CreateTranslation( 5.0, 1.0, 2.0 ) );
c.SetAttribute(PropertyName.MATERIAL, pm);
