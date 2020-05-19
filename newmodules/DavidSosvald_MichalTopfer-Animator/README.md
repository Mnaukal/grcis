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

```
Parameter (string name, Parser parser, Interpolator interpolator, bool required = false)
```
- name – identifier of the parameter in the keyframes file
- parser – a functions which parses the values of the parameter from the keyframes file
  - must satisfy the `Animator.Parser` delegate: `delegate object Parser (string s)`
  - the prepared parsers can be found in `Animator.Parsers` class (we currently support `double`, `Vector3d` and `double[]`)
- interpolator – a function which interpolates the values between keyframes
  - must satisfy the `Animator.Interpolator` delegate: `delegate object Interpolator (object previous, object current, object next, object later, double t)` (it gets values of the parameter in keyframes and should return the interoplated value)
  - we provide LERP (linear interpolation) and Catmull-Rom interpolators for `double`, `Vector3d` and `double[]` in `Animator.Interpolators` class
- required – if set to `true` and a value of the parameter is not specified in the first keyframe, an exception is thrown

*All our extensions listed below call the `RegisterParams` method inside their costructor if the `Animator` is specified in the first argument.*

After registering all params, the keyframes must be loaded from file using the `LoadKeyframes` method.

## Example

The example script can be found in file `AnimatorExample.cs` (with keyframes in `AnimatorExample.yaml`.

## Images, videos

**TODO**

# Extension: KeyframeAnimatedStaticCamera

**TODO**

# Extension: AnimatedNodeTransform

**TODO**

# Extension: AnimatedNodeMaterialColor

**TODO**
