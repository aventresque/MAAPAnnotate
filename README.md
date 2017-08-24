# MAAPAnnotate

MAAPAnnotate is a HoloLens application which provides interactive methods of annotating 3D models of megalithic art in Augmented Reality.
You can find in this github the Unity files you need to create the HoloLens application.

Dependencies: HoloToolkit https://github.com/Microsoft/MixedRealityToolkit-Unity.

In Unity, start by launching the Unity scene ("Assets" -> "Scenes" -> "AnnotateTool").
After building the app in Unity ("File" -> "Build Settings"), you can deploy it on the HoloLens from Visual Studio 2017:
First plug the HoloLens to your computer, then in Visual Studio 2017 select "Master", "x86", and "Device" and deploy the app by clicking on "Debug" -> "Start without debugging".
You can then directly launch the app from the HoloLens.

In order to view what is displayed in the HoloLens, go to the Windows Device Portal http://127.0.0.1:10080/ when the HoloLens is plugged.
