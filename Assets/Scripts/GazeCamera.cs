using UnityEngine;
using System.Collections;
using TETCSharpClient;
using TETCSharpClient.Data;
using Assets.Scripts;
using Filter.Utils;

/// <summary>
/// Component attached to 'Main Camera' of '/Scenes/std_scene.unity'.
/// This script handles the navigation of the 'Main Camera' according to 
/// the GazeData stream recieved by the EyeTribe Server.
/// </summary>
public class GazeCamera : MonoBehaviour, IGazeListener
{
    private Camera cam;

    private double eyesDistance;
    private double baseDist;
    private double depthMod;

	private bool filteredPose;
	private double currentSmoothing;
	private MotionFilter filteredPoseLeftEye;
	private MotionFilter filteredPoseRightEye;

    private Component gazeIndicator; 

    private Collider currentHit;

    private GazeDataValidator gazeUtils;
		
	void Start () 
    {
        //Stay in landscape
        Screen.autorotateToPortrait = false;

        cam = GetComponent<Camera>();
        baseDist = cam.transform.position.z;
        gazeIndicator = cam.transform.GetChild(0);

        //initialising GazeData stabilizer
        gazeUtils = new GazeDataValidator(30);

		// Initializing the head pose filtering
		filteredPose = false;
		filteredPoseLeftEye = new MotionFilter(1, 1, 3);
		filteredPoseRightEye = new MotionFilter(1, 1, 3);

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
			eyesDistance = gazeUtils.GetLastValidUserDistance();
			Vector3 new3DPos = UnityGazeUtils.BackProjectDepth(userPos, eyesDistance, baseDist);

			// Update the MotionFilter for both eyes anyway (so that the filter is up-to-date)
			double[] correctedPoseLeft;
			double[] correctedPoseRight;
			double confidenceLeft, confidenceRight;

			userPos = gazeUtils.GetLastValidLeftEyePosition();
			Vector3 new3DPosLeft = UnityGazeUtils.BackProjectDepth(userPos, eyesDistance, baseDist);
			filteredPoseLeftEye.Predict(); // Propagate the previous measurement
			filteredPoseLeftEye.Correct(UnityGazeUtils.Vec3ToArray(new3DPosLeft), 3); // Correct with the new observation
			filteredPoseLeftEye.GetPostState(out correctedPoseLeft, out confidenceLeft); // Get the up-to-date estimation

			userPos = gazeUtils.GetLastValidRightEyePosition();
			Vector3 new3DPosRight = UnityGazeUtils.BackProjectDepth(userPos, eyesDistance, baseDist);
			filteredPoseRightEye.Predict();
			filteredPoseRightEye.Correct(UnityGazeUtils.Vec3ToArray(new3DPosRight), 3);
			filteredPoseRightEye.GetPostState(out correctedPoseRight, out confidenceRight);

			// Deal with faulty cases when one of the eyes is not visible, merge the two positions
			Vector3 newFiltered3DPos = MotionFilter.MergePositions(new3DPosLeft, confidenceLeft, new3DPosRight, confidenceRight);

			if (filteredPose) {
				cam.transform.position = newFiltered3DPos;
			} else {
				cam.transform.position = new3DPos;
			}

			// Camera 'look at' origin
            cam.transform.LookAt(Vector3.zero);

            // Tilt cam according to eye angle
            double angle = gazeUtils.GetLastValidEyesAngle();
            cam.transform.eulerAngles = new Vector3(cam.transform.eulerAngles.x, cam.transform.eulerAngles.y, cam.transform.eulerAngles.z + (float)angle);
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
		else if (Input.GetKey(KeyCode.S)) 
		{
			// Increase pose smoothing
			this.currentSmoothing *= 1.1;
			filteredPoseLeftEye.updateSmoothing(currentSmoothing);
			filteredPoseRightEye.updateSmoothing(currentSmoothing);
		} 
		else if (Input.GetKey(KeyCode.D)) 
		{
			// Decrease smoothing
			this.currentSmoothing *= 0.9;
			filteredPoseLeftEye.updateSmoothing(currentSmoothing);
			filteredPoseRightEye.updateSmoothing(currentSmoothing);
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
    }

    void OnApplicationQuit()
    {
        GazeManager.Instance.RemoveGazeListener(this);
    }
}
