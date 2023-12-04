A comprehensive quality management setup.

Features:

Camera manager and Camera setup script
The cameras in the package needs to be setup using the CameraSetup script, this links them to the CameraManager which couples the cameras to the quality management.

Quality manager
The quality manager itself is responsible for either auto detecting the appropriate quality level or allowing users to set it themselves.
It will have references to all of the sub quality managers and inform them of any change in quality level.
Additionally it will be taking care of loading and saving the quality level.

Tier quality manager
Responsible for changing the quality tier for the project based on the current quality level, this should be setup to include pipeline, shadow quality and so on…

Rendertexture quality manager
Responsible for changing the size of any render textures already populated or that will be requested based on current quality level.
All requests for a render texture sprite should go through here.

Audio quality manager
Responsible for changing the audio to be forced in mono if the quality tier is low enough.

Render scale quality manager
In bad cases it could be needed to cut the render scale down, in that case we have this manager that sets the render scale based on current quality level. 
Use with caution as it makes the game look awful but gives quite the performance boost.

Framerate quality manager
A sub manager responsible for setting the target frame rate based on platform.

Test scene
A fully functional test scene with all the functionality setup.

Test assets
There is included a test sphere, material and scriptable object configs.


Unity dependencies:
- TextMeshPro

Third party dependencies:
- MobileBloom (included)
