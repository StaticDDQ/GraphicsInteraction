Camera Control:
- Both pitch and yaw are manipulated using Input.GetAxis of the mouse which constantly updates per frame.
- Used lerp for the roll by rotatating the object in the Z-axis till a maximum value. This only happens if the player move either left or right and roll back if not.
- Used an empty gameobject be a parent of the camera, so the camera moves in all axis while the gameobject only move in the X and Y axis. Prevent camera from moving down when rolling.
- Used a sphere collider and a rigidbody for the camera to prevent collision to the terrain and water.

Water
- A C# script was made to increase number of polygons by adding a number of triangles based on what the player wants. 
- The script also made the texture move by updating the texture offset in the update function.
- The shader updates the vertex of the water by applying a combination of sine and cosine functions to change the y axis and using _Time.y

Sun:
- Particle Systems are used to create fire effect just above the underlying sun sphere, emitting particles that takes a smoke texture image aligned to the surface of the sun that rotates in a sphere shape to mimic a real sun.
- The Sun is rotating about the center (an Empty Object with a script attached to it), with adjustable speed.
- Point Light at its center, attached as a child object of the sun, so the lighting would move along with the sun.

Landscape properties:
- Each vertices' color is defined in the script that generated the landscape. Depending on the height of individual vertex, corresponding color is generated from 3 different colors for different regions, with gradient between colors, and is fed to the mesh.

Landscape generation:
- The landscape is generated using the diamond-square algorithm, with the initialization of the height map done in CreateHeightMap(), and the diamond and square steps done iteratively in DiamondSquareGenerator().
- The user can set the landscape size, height limit, diamond-square iterations, and landscape smoothness through public variables in the C# script
- The heightmap is also offset such that half the landscape is above water and the rest submerged by setting average heights to 0
- Smoothness affects the rate at which the random offset applied to the heights decreases as a 2-part linear function seperated by Mathf.abs():
	- 0 to 0.5 : First iteration weight is constant at 1, final iteration weight decreases, gradient steepens
	- 0.5 to 1 : First iteration weight decreases, limit of final iteration weight is constant at 0, gradient flattens
- Ranges:
	- Iterations = [0, 6] (Past 6 iterations, number of vertices exceed 65536)
	- Smoothness = [0, 1] (0 = Sharp surfaces, 0.5 = Slightly rugged, 1 = Entirely smooth)
