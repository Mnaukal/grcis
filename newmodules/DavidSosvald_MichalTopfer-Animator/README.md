# Extension: Keyframes Animator

### Author: David Šosvald & Michal Töpfer

### Category: Animator

### Namespace: DavidSosvald_MichalTopfer

### Class name:  Animator : ITimeDependent

### ITimeDependent: Yes

### Source file: Animator.cs

This extension implements an animator which can interpolate properties according to keyframes specified in a file. The animator should be used as `scene.Animator` in order to interpolate the properties when the time is set. The Animator implements `ITimeDependentProperty` interface, so that the values of properties can be accessed through it or using the `Animator.getParams` method.

The Animator is designed to be used together with `ITimeDependent` objects such as one of our other extensions (extensions marked with * are described below):

- KeyframeAnimatedStaticCamera*,
- [KeyframeAnimatedDofCamera](https://github.com/Mnaukal/grcis/tree/summer2019-2020/newmodules/DavidSosvald_MichalTopfer-DoFCamera),
- AnimatedNodeTransform*,
- AnimatedNodeMaterialColor*,

and other:

- [AnimatedNodeTranslate](https://github.com/pepcape/grcis/tree/master/newmodules/JosefPelikan-AnimatedNodeTranslate) (by Josef Pelikán)
- etc.

## Usage

When constructing the Animator, a path to the keyframes file must be specified. The animator should be used as `scene.Animator`.

All params which are to be animated must be registered first using the `RegisterParams` or `RegisterParam` methods. This is necassary in order to parse them correctly from the keyframes file and to set the interpolation mode. Both these methods expect `Animator.Parameter` objects with following constructor:

```c#
Parameter (string name, Parser parser, Interpolator interpolator, bool required = false)
```

- name - identifier of the parameter in the keyframes file
- parser - a functions which parses the values of the parameter from the keyframes file
  - must satisfy the `Animator.Parser` delegate: `delegate object Parser (string s)`
  - the prepared parsers can be found in `Animator.Parsers` class (we currently support `double`, `Vector3d` and `double[]`)
- interpolator - a function which interpolates the values between keyframes
  - must satisfy the `Animator.Interpolator` delegate: `delegate object Interpolator (object previous, object current, object next, object later, double t)` (it gets values of the parameter in keyframes and should return the interpolated value)
  - we provide LERP (linear interpolation) and Catmull-Rom interpolators for `double`, `Vector3d` and `double[]` in `Animator.Interpolators` class
- required - if set to `true` and a value of the parameter is not specified in the first keyframe, an exception is thrown

*All our extensions listed below call the `RegisterParams` method inside their constructor if the `Animator` is specified in the first argument.*

After registering all parameters, the keyframes must be loaded from file using the `LoadKeyframes` method.

## Script file

When creating a new `Animator`, you need to provide a path to your script file. We support script files in YAML format. For each keyframe, you must first specify its time, and the provide the values of parameters. For required parameters, a value must be specified in every keyframe, else an exception is thrown. Lines that begin with a "#" will be ignored. You can see an example script file in `AnimatorExample.yaml`.

## Example

The example scene script can be found in file `AnimatorExample.cs` (with keyframes in `AnimatorExample.yaml`).

## Images, videos

https://youtu.be/2ri3-RG7Zgc

# Extension: KeyframeAnimatedStaticCamera

### Author: David Šosvald & Michal Töpfer

### Category: Animated camera

### Namespace: DavidSosvald_MichalTopfer

### Class name:  KeyframesAnimatedStaticCamera : StaticCamera, ITimeDependent

*`IVertigoInnerCamera` interface is also implemented, so this camera can be used together with [VertigoEffectCamera](https://github.com/Mnaukal/grcis/tree/summer2019-2020/newmodules/DavidSosvald_MichalTopfer-VertigoEffectCamera) extension.*

### ITimeDependent: Yes

### Source file: KeyframesAnimatedStaticCamera.cs

The `KeyframesAnimatedStaticCamera` is built on top of the `StaticCamera` and its position, direction and viewing angle can be animated using `Animator` (described above).

The constructor of the `KeyframesAnimatedStaticCamera` takes an `Animator` as a first parameter. If it is not null, the `Animator.RegisterParams` method is called in the constructor. The rest of the parameters are strings which can be used to specify the names of the animated parameters (used in the keyframes file).

The position and direction are of type `Vector3d` and the viewing angle is expected in degrees.

## Example

The example scene script can be found in file `AnimatorExample.cs` (with keyframes in `AnimatorExample.yaml`).

# Extension: AnimatedNodeTransform

### Author: David Šosvald & Michal Töpfer

### Category: Animated scene component

### Namespace: DavidSosvald_MichalTopfer

### Class name:  AnimatedNodeTransform : AnimatedCSGInnerNode

### ITimeDependent: Yes

### Source file: AnimatedNodeTransform.cs

This extension defines a scene node whose translation, rotation and scale can be animated. It is designed to work with `Animator` (described above), but can be easily modified to work with any `ITimeDependentProperty`.

Every time the `Time` property of the node is set, it requests the scene's current Animator (`MT.scene.Animator`), gets the associated animated properties and updates its transform. 

The constructor of the `AnimatedNodeTransform` takes an `Animator` as a first parameter. If it is not null, the `Animator.RegisterParams` method is called in the constructor. The rest of the parameters are strings which can be used to specify the names of the animated parameters (used in the keyframes file) and default values of the translation, rotation and scale. If the names of some of the parameters are set to null, these parameters are not animated and the default value is used.

## Example

The example scene script can be found in file `AnimatorExample.cs` (with keyframes in `AnimatorExample.yaml`).

# Extension: AnimatedNodeMaterialColor

### Author: David Šosvald & Michal Töpfer

### Category: Animated scene component

### Namespace: DavidSosvald_MichalTopfer

### Class name:  AnimatedNodeMaterialColor : AnimatedCSGInnerNode

### ITimeDependent: Yes

### Source file: AnimatedNodeMaterialColor.cs

This extension defines a scene node which can animate the color of its material. It is designed to work with `Animator` (described above), but can be easily modified to work with any `ITimeDependentProperty`.

Every time the `Time` property of the node is set, it requests the scene's current Animator (`MT.scene.Animator`), gets the associated animated property and updates its material's color.

The constructor of the `AnimatedNodeMaterialColor` takes an `Animator` as a first parameter. If it is not null, the `Animator.RegisterParams` method is called in the constructor. The second optional parameter can be used to specify the name of the animated parameter (used in the keyframes file).

## Example

The example scene script can be found in file `AnimatorExample.cs` (with keyframes in `AnimatorExample.yaml`).
