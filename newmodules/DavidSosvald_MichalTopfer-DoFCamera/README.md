# Extension: Depth-of-Field Camera

### Author: David Šosvald & Michal Töpfer

### Category: Camera

### Namespace: DavidSosvald_MichalTopfer

### Class name:  DoFCamera : StaticCamera, ICamera

### ITimeDependent: No

### Source file: DoFCamera.cs

The `DoFCamera` simulates the depth-of-field effect, i. e. blurring objects which are out of focus.

## Images, videos

The following images show the same scene rendered with focus distance set to 2.5 and 4.

![focus distance = 2.5](examples/RenderResult_dof_f2.5.png?raw=True "Focus distance = 2.5, focusing on the first cube")
![focus distance = 4](examples/RenderResult_dof_f4.png?raw=True "Focus distance = 4, focusing on the second cube")

## Installation

To use this extension, include the `DoFCamera.cs` file to your project. The `DoFCamera` class is in the `DavidSosvald_MichalTopfer` namespace.

This extension also contains an animated version of the Depth-of-Field camera compatible with our [Animator](https://github.com/Mnaukal/grcis/tree/summer2019-2020/newmodules/DavidSosvald_MichalTopfer-Animator) extension. To use it, include the `AnimatableDoFCamera.cs` file to your project.

## Overview

The `DoFCamera` uses supersampling to simulate that rays come from the entire surface of the camera's lens. The origin of each of the rays is moved slightly to "cover" the whole surface of the lens. We recommend using at least 16 samples per pixel in order to prevent unwanted artifacts in the image.

*This camera does not work well with adaptive supersampling as that prevents us from taking the samples evenly from the entire surface of the camera's lens.*

## Usage

The `DoFCamera` class inherits from `StaticCamera`, so the following parameters are passed into the costructor of `StaticCamera`:

* **cen** - Center of the projection.
* **dir** - View direction (must not be zero).
* **ang** - Horizontal viewing angle in degrees.

The focus distance can be set in the constructor using one of these parameters:

* **focus_distance** - Objects exactly this far from camera's center will be in focus. Other object will be blurred (bigger difference between object's distance and the `focus_distance` means more blur).
* **focus_point** - The `focus_distance` will be computed as distance between `cen` and `focus_point`.

The rest of arguments of the constructor control the amount of blur and type of bokeh:

* **max_shift** - How much the origins of rays will be shifted. Higher value blurs the image more.
* **shift_mode** - Method of sampling the points on the camera's lens to create the effect. By default, we try to simulate that the lens is a circle and also add a small amount of jittering.

## Example

An example static scene script can be found in file `DoFTest.cs` (copy of the Cubes scene with `DoFCamera`). An example animated scene script can be found in file `DoFExample.cs` (with keyframes in `DoFExample.yaml`).

## KeyframesAnimatedDoFCamera

### Author: David Šosvald & Michal Töpfer

### Category: Animated camera

### Namespace: DavidSosvald_MichalTopfer

### Class name:  KeyframesAnimatedDoFCamera : DoFCamera, ITimeDependent

*`IVertigoInnerCamera` interface is also implemented, so this camera can be used together with [VertigoEffectCamera](https://github.com/Mnaukal/grcis/tree/summer2019-2020/newmodules/DavidSosvald_MichalTopfer-VertigoEffectCamera) extension.*

### ITimeDependent: Yes

### Source file: KeyframesAnimatedDoFCamera.cs

In `KeyframesAnimatedDoFCamera`, the `"position"`, `"direction"`, `"angle"`, `"max_shift"`, `"foucs"` and `"focus_point"` parameters can be animated. It works similarly to the [KeyframesAnimatedStaticCamera](https://github.com/Mnaukal/grcis/tree/summer2019-2020/newmodules/DavidSosvald_MichalTopfer-Animator).
