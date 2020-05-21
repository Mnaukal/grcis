# Extension: Vertigo Effect Camera

### Author: David Šosvald & Michal Töpfer

### Category: Animated camera

### Namespace: DavidSosvald_MichalTopfer

### Class name:  VertigoEffectCamera : ITimeDependent, ICamera

### ITimeDependent: Yes

### Source file: VertigoEffectCamera.cs

This camera creates the [Dolly zoom](https://en.wikipedia.org/wiki/Dolly_zoom) effect, also known as the [Vertigo Effect](https://youtu.be/je0NhvAQ6fM?t=32). A target point has to be specified and this camera can then update the direction to look at the target point and the viewing angle to keep the width of the scene unchanged while the camera moves.

## Installation

This extension requires the [Animator](https://github.com/Mnaukal/grcis/tree/summer2019-2020/newmodules/DavidSosvald_MichalTopfer-Animator) extension.

To use it, include the `VertigoEffectCamera.cs` file to your project. The `VertigoEffectCamera` class is in the `DavidSosvald_MichalTopfer` namespace.

## Usage

*Please see the documentation of [Animator](https://github.com/Mnaukal/grcis/tree/summer2019-2020/newmodules/DavidSosvald_MichalTopfer-Animator) before reading this.*

This class wraps around an inner camera and modifies the direction and angle of it. The inner camera must implement the `IVertigoInnerCamera` interface. Our other extensions - [`KeyframesAnimatedStaticCamera`](https://github.com/Mnaukal/grcis/tree/summer2019-2020/newmodules/DavidSosvald_MichalTopfer-Animator) and [`KeyframesAnimatedDoFCamera`](https://github.com/Mnaukal/grcis/tree/summer2019-2020/newmodules/DavidSosvald_MichalTopfer-DoFCamera) - implement this interface, but it has to be uncommented in the class definition before using it.

The constructor of the `VertigoEffectCamera` requires an `innerCamera` object of type `IVertigoInnerCamera`. When the animation runs, the `VertigoEffectCamera` modifies the `innerCamera`'s direction and angle. The constructor also takes an `Animator` as a its argument. If it is not null, the `Animator.RegisterParams` method is called in the constructor. Some of the remaining arguments are strings which can be used to specify the names of the animated parameters (used in the keyframes file).

This camera requires that the `target` (`Vector3d` specifying position of the target) is specified in the keyframes file. The `target_width` (width of the visible part of the scene in units of the scene) also has to be specified through the constructor or the keyframes file.

If `updateAngle` is set to `true` in the constructor, the camera will call the `SetAngle` method on the `innerCamera` to set the angle to a value that makes the width of the visible frustum equal to the `target_width` at the position of the `target`. The value is computed based on the distance of camera's position (accessed through `GetPosition` and the `target` point). This ensures that an object placed at `target` position keeps the same width in the picture.

If `updateDirection` is set to `true` in the constructor, the camera will call the `SetDirection` method of the `innerCamera` to set the direction to `(target - position).Normalized()`. This keeps the `target` in the center of the frame.

The effect can be turned on and off by setting the `vertigo` parameter in the keyframes file to a value between 0 and 1. This value will be used to linearly interpolate between the original values of the parameters and the values of the parameters computed by the `VertigoEffectCamera`. By setting `vertigo` to 1, the value from `VertigoEffectCamera` will be used; setting `vertigo` to 0 will disable the effect completely.

## Examples

keyframes file:

```
0:
    position: -15, 1, 0
    direction: 1, 0.05, 0
    angle: 30
    target: 0, 2, 0
    vertigo: 1
10:
    position: -5, 1, 0
    # we set the direction and angle to the approximate values at which the vertigo effects ends and interpolate to these values over the last second
    direction: 1, 0.2, 0
    angle: 75
    vertigo: 1
11:
    vertigo: 0
```

result: https://youtu.be/Wm6YQGB_5A8

This example scene script can also be found in `VertigoExample.cs` (with keyframes in `VertigoExample.yaml`).
