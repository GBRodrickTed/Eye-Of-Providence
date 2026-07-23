# Setup Guide

A guide strictly to get the basic features working. Will not go into depth on how it works or more complex features (using the flexable ray-to-globe implementation)
* Create a Unity project. (This was made in Unity 2019.4.40f1)
* Create a material with the CoolShader shader.
* Add the GlobeCam Prefab to the scene or create an object parenting 6 camera objects (front, back, left, right, up, down in that order)
* Attach PostProcessingBaby script onto the main camera.
* Set PostProcessingBaby parameters as follows:
   * Main Cam - The Main Camera
   * Post Effect Material - The material with the CoolShader
   * Cam Elements 0-5 - Cam Front, Cam Back, Cam Left, Cam Right, Cam Up, and Cam Down in that order
   * Quality - 9
* Set CoolShader Material properties as follows:
   * Field of View - 360
   * Projection Mode - 0
   * Aspect Ratio - Current Screen Aspect Ratio
   * Stretch to Fit - 0
   * Ray Method - 1
* If done properly, you will see the Equirectangular Projection of the scene on the display screen.
* It's advised to play around with the settings and look at / modify the code to get an understanding of how it works