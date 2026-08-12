\# Screen Color Sampler



Averages camera-rendered color within a world-space box that follows a GameObject's `transform`, exposed as `CurrentColor` and a `ColorSampled` event. No knowledge of what happens with the color, pair it with a purpose-built consumer.



\## Setup

1\. Add a `BFScreenColorSampler` component to a GameObject (e.g. the player).

2\. Set `Offset X` / `Offset Y` and `Width` / `Height` in world units. These define a box relative to the object's position, not the screen. `Offset X` defaults to `1`, placing the box slightly in front of the object along X; `Width` / `Height` default to `1x1`.

3\. Optionally assign a `Target Camera Override`; otherwise it resolves `Camera.main` via `BFCameraResolver`.



\## Usage

Subscribe to `ColorSampled`, or poll `CurrentColor` directly:

```csharp

screenColorSampler.ColorSampled += color => Debug.Log(color);

```

For an example consumer, see `BFControllerLedColorFeeder` in `com.bftools.feedback`, which forwards sampled colors straight to `BFControllerLedManager.SetColor`.



\## How it works

\- Every `Sample Interval` seconds (default `0.1`), the sampled box is rebuilt from the object's current `transform.position` plus offset, then projected into the resolved camera's viewport space via `WorldToViewportPoint`.

\- The camera renders into a small offscreen `RenderTexture` (`Sample Resolution`, default `32x32`), read back asynchronously via `AsyncGPUReadback` so there's no synchronous GPU stall.

\- Once the readback completes, only the pixels within the projected viewport rect are averaged into `CurrentColor`, and `ColorSampled` fires.

\- If every corner of the box falls behind the camera, that interval's sample is skipped.

\- A wire gizmo is drawn around the sampled box in the scene view when the object is selected, using the same world-space corners the sampler itself computes.



\## Notes

\- `Width` / `Height` are world units, not screen fractions. A box tuned for one camera zoom or scene scale may need retuning for another.

\- Sampling is both throttled (`Sample Interval`) and downsampled (`Sample Resolution`) to avoid a full-resolution `ReadPixels` stall every frame. Lowering `Sample Resolution` further reduces cost at the expense of color accuracy.

\- `AsyncGPUReadback` results arrive a frame or more after the request. `CurrentColor` and `ColorSampled` reflect where the object was when the sample was requested, not necessarily its position when the callback fires.

\- Depends only on Unity's core rendering module (`UnityEngine.Rendering`) and `com.bftools.core`'s `BFTools.Core.CameraUtility` / `BFTools.Core.Logger`; no extra package dependency.

