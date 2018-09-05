Camera Control:
- Both pitch and yaw are manipulated using Input.GetAxis of the mouse which constantly updates per frame
- The roll implementation is abit different where I used lerp to rotate the object in the Z-axis till a maximum value. This
only happens if the player move either left or right. It will roll back to normal state if the player is neither moving or
just moving forward
- A problem with roll is that the camera move down when it rotates around the Z-axis, a solution I used is to have an empty 
gameobject be a parent of the camera, so the camera moves in all axis while the gameobject only move in the X and Y axis
- In order to avoid going through terrain, I used a sphere collider and a rigidbody for the camera, therefore the movement
of the camera is based on the rigidbody, using addforce for moving.

Water
- In order to increase the number of vertices, I made a C# script that adds a number of triangles based on what value the player
wants. The script takes a grid size as an input and equally distribute the vertices among the grid.
- The script also made the texture move by updating the texture offset in the update function
- The shader updates the vertex of the water by applying a combination of sine and cosine functions to change the y axis and using _Time.y