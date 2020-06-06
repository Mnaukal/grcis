# Extension: Camera Animation Proxy

### Author: David Šosvald & Michal Töpfer

### Category: Animated Camera

### Namespace: DavidSosvald_MichalTopfer

### Class name:  CameraAnimationProxy : ICamera, ITimeDependent

### ITimeDependent: Yes

### Source file: CameraAnimationProxy.cs

The `CameraAnimationProxy` is an object which wraps around any `ICamera` and tries to animate its position and direction while preserving the `up` vector. It is done by calling the `GetRay` method on the inner camera and then transforming the returned ray. This animated camera works with our [Animator](https://github.com/Mnaukal/grcis/tree/summer2019-2020/newmodules/DavidSosvald_MichalTopfer-Animator) extension and also with any `ITimeDependentProperty`.

## Usage

The constructor of the `CameraAnimationProxy` takes the camera as its first argument. The camera must be already initialized when it is passed to the `CameraAnimationProxy`'s constructor as the `GetRay` method is called inside. The `GetRay` method is used to determine the camera's original position and direction. Later, when the `Time` is set, a transformation matrix from the original position and direction to the desired position and direction (retreived from the `ITimeDependentProperty` or `Animator`) is computed. When `GetRay` method is called on the `CameraAnimationProxy`, the `GetRay` method from the inner camera is called and the ray is transformed using the computed transformation matrix.

The `up` vector can be either passed as an argument to the constructor or it is infered using the `GetRay` method (see the `Initialize` method for details).

 An `Animator` can be passed as an optional parameter. If it is not null, the `Animator.RegisterParams` method is called in the constructor. The rest of the parameters are strings which can be used to specify the names of the animated parameters (used in the keyframes file).

## Example

An example scene (based on the Cubes scene) can be found in file `CameraAnimationProxyExample.cs` (with keyframes in `CameraAnimationProxyExample.yaml`).

## Images, videos

https://youtu.be/bn7-018hNZE
