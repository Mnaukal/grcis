# Extension: Keyframes Animator

### Author: David Šosvald & Michal Töpfer

### Category: Animator

### Namespace: DavidSosvald_MichalTopfer

### Class name:  Animator : ITimeDependent

### ITimeDependent: Yes

### Source file: Animator.cs

This extension implements an animator which can interpolate properties according to keyframes specified in a file. The animator should be used as `scene.Animator` in order to interpolate the properties when the time is set. The Animator implements `ITimeDependentProperty` interface, so that the values of properties can be accessed through it or using the `Animator.getParams` method.

**TODO** it works with [our extensions] and [Pepèa's AnimatedNodeTranslate]

When constructing the Animator, a path to the keyframes file must be specified.

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
