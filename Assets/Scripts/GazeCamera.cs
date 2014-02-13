using UnityEngine;
using System.Collections;
using TETCSharpClient;
using TETCSharpClient.Data;
using Assets.Scripts;
using FilterUtils;
using System.Diagnostics; // Console output
using System;

/// <summary>
/// Component attached to 'Main Camera' of '/Scenes/std_scene.unity'.
/// This script handles the navigation of the 'Main Camera' according to 
/// the GazeData stream recieved by the EyeTribe Server.
/// </summary>
public class GazeCamera : MonoBehaviour, IGazeListener
{
    private Camera cam;

    private double eyesDistance;
    private double depthMod;
    private double baseDist;

    private Component gazeIndicator; 

	private bool    filteredPose;
    private float   currentSmoothing;

    private Collider currentHit;

    private GazeDataValidator gazeUtils;
		
	void Start () 
    {
        //Stay in landscape
        Screen.autorotateToPortrait = false;

        cam = GetComponent<Camera>();
        gazeIndicator = cam.transform.GetChild(0);

        baseDist = cam.transform.position.z;
            
        //initialising GazeData stabilizer
        gazeUtils = new GazeDataValidator(30);
		gazeUtils.setBaseDist(cam.transform.position.z); // Give the filtering framework the information about the cam pose

		filteredPose = false;
        currentSmoothing = 1F;

        //register for gaze updates
        GazeManager.Instance.AddGazeListener(this);
	}

    public void OnGazeUpdate(GazeData gazeData) 
    {
        //Add frame to GazeData cache handler
        gazeUtils.Update(gazeData);
    }


	void Update ()
    {
        Point2D userPos = gazeUtils.GetLastValidUserPosition();

        if (null != userPos)
        {
			// Get the 3D pose from the picture space measurements
			// TODO: Use directly the 3D pose from the API (?)
			if (filteredPose) {
                Vector3 newCamPose = gazeUtils.GetFilteredHeadPose();

                // Change units to fit with the assets of the 3D scene...
                // TODO: bring back the scene to a proper scale, this breaks the immersion
                // (the move is amplified)
                newCamPose.x = 10*newCamPose.x;
                newCamPose.y = -10*newCamPose.y;
                newCamPose.z = -10*newCamPose.z;
                
                cam.transform.position = newCamPose;

                // We don't tilt the cam to get the 3D effect
                // Camera 'look at' origin
                cam.transform.LookAt(Vector3.zero);
                
            } else {
                gazeUtils.GetFilteredHeadPose();
                eyesDistance = gazeUtils.GetLastValidUserDistance();
                cam.transform.position = UnityGazeUtils.BackProjectDepth(userPos, eyesDistance, baseDist);

                // Camera 'look at' origin
                cam.transform.LookAt(Vector3.zero);
                
                // Tilt cam according to eye angle
                double angle = gazeUtils.GetLastValidEyesAngle();
                cam.transform.eulerAngles = new Vector3(cam.transform.eulerAngles.x, cam.transform.eulerAngles.y, cam.transform.eulerAngles.z + (float)angle);
            }
        }

        Point2D gazeCoords = gazeUtils.GetLastValidSmoothedGazeCoordinates();

        if (null != gazeCoords)
        {
            // Map gaze indicator
			Point2D gp = UnityGazeUtils.getGazeCoordsToUnityWindowCoords(gazeCoords);
			
			Vector3 screenPoint = new Vector3((float)gp.X, (float)gp.Y, cam.nearClipPlane + .1f);

            Vector3 planeCoord = cam.ScreenToWorldPoint(screenPoint);
            gazeIndicator.transform.position = planeCoord;

            // Handle collision detection
            checkGazeCollision(screenPoint);
        }

        // Handle keypress
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if (Input.GetKey(KeyCode.Space))	
		{
            Application.LoadLevel(0);
        } 
		else if (Input.GetKey(KeyCode.F))	
		{
			// Trigger the pose estimation filter
			this.filteredPose = !this.filteredPose;

		} 
		else if (Input.GetKeyDown(KeyCode.S)) 
		{
			// Increase pose smoothing
			currentSmoothing *= 1.1F;
            gazeUtils.setSmoothing(currentSmoothing);
		} 
        else if (Input.GetKeyDown(KeyCode.D)) 
		{
			// Decrease smoothing
			currentSmoothing *= 0.9F;
            gazeUtils.setSmoothing(currentSmoothing);
		}
	}

    private void checkGazeCollision(Vector3 screenPoint)
    {
        Ray collisionRay = cam.ScreenPointToRay(screenPoint);
        RaycastHit hit;
        if (Physics.Raycast(collisionRay, out hit))
        {
            if (null != hit.collider && currentHit != hit.collider)
            {
                //switch colors of cubes according to collision state
                if (null != currentHit)
                    currentHit.renderer.material.color = Color.white;
                currentHit = hit.collider;
                currentHit.renderer.material.color = Color.red;
            }
        }
    }

    void OnGUI()
    {
        int padding = 10;
        int btnWidth = 160;
        int btnHeight = 40;
        int y = padding;

        if (GUI.Button(new Rect(padding, y, btnWidth, btnHeight), "Press to Exit"))
        {
            Application.Quit();
        }

        y += padding + btnHeight;

        if (GUI.Button(new Rect(padding, y, btnWidth, btnHeight), "Press to Re-calibrate"))
        {
            Application.LoadLevel(0);
        }

		y += padding + btnHeight;

		if (!filteredPose) {
			if (GUI.Button(new Rect(padding, y, btnWidth, btnHeight), "Engage Filtering"))
			{
				this.filteredPose = true;
            }
        } else {
			if (GUI.Button(new Rect(padding, y, btnWidth, btnHeight), "Disengage Filtering"))
			{
				this.filteredPose = false;
            }
        }
    }
    
    void OnApplicationQuit()
    {
        GazeManager.Instance.RemoveGazeListener(this);
    }
}
