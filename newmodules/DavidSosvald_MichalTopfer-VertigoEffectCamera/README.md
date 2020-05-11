# Vertigo Effect Camera

This camera creates the [Dolly zoom](https://en.wikipedia.org/wiki/Dolly_zoom) effect, also known as the [Vertigo Effect](https://youtu.be/je0NhvAQ6fM?t=32). A target position has to be specified and this camera can then update the direction to look at the target position and the viewing angle to keep the width of the scene unchanged while the camera moves.

## Installation

This extension requires the [Animated Camera](https://github.com/Mnaukal/grcis/tree/master/common/rt_extensions/AnimatedCamera) extension.

To use it, include the `VertigoEffectCamera.cs` file to your project. The `VertigoEffectCamera` class is in the `DavidSosvald_MichalTopfer` namespace.

## Usage

*Please see the documentation of [Animated Camera](https://github.com/Mnaukal/grcis/tree/master/common/rt_extensions/AnimatedCamera) before reading this.*

This class wraps around an inner camera and modifies the direction and angle before they are passed to it. It can be used for example with `AnimatableStaticCamera`.

The constructor of the `VertigoEffectCamera` requires an `innerCamera` object of type `IAnimatableCamera`. When the animation runs, the `VertigoEffectCamera` modifies the parameters passed to the `innerCamera` in the `ApplyParams` method. The modified parameters are named `"angle"` and `"direction"`.

This camera requires that the `target` (`Vector3d` specifing position of the target) is specified in the keyframes file. The `target_width` (width of the visible part of the scene in units of the scene) also has to be specified through the constructor or the keyframes file.

If `updateAngle` is set to `true` in the constructor, the camera will set the `"angle"` parameter of the `innerCamera` to a value that makes the width of the visible frustrum equal to the `target_width` at the position of the `target`. The value is computed based on the distance of camera `position` and the `target` position. This ensures that an object placed at `target` position keeps the same width in the picture. The `angle` is in degrees.

If `updateDirection` is set to `true` in the constructor, the camera will set the `"direction"` parameter of the `innerCamera` to `(target - position).Normalized()`. This keeps the `target` in the center of the frame.

The effect can be turned on and off by setting the `vertigo` parameter in the keyframes file to a value between 0 and 1. This value will be used to lineary interpolate between the original values of the parameters and the values of the parameters computed by the `VertigoEffectCamera`. By setting `vertigo` to 1, the value from `VertigoEffectCamera` will be used; setting `vertigo` to 0 will disable the effect completely.

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