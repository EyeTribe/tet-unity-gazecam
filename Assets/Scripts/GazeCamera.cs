using UnityEngine;
using System.Collections;
using TETCSharpClient;
using TETCSharpClient.Data;
using Assets.Scripts;

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

    private Component gazeIndicator;

    private Collider currentHit;

    private GazeDataValidator gazeUtils;

    void Start()
    {
        //Stay in landscape
        Screen.autorotateToPortrait = false;

        cam = GetComponent<Camera>();
        baseDist = cam.transform.position.z;
        gazeIndicator = cam.transform.GetChild(0);

        //initialising GazeData stabilizer
        gazeUtils = new GazeDataValidator(30);

        //register for gaze updates
        GazeManager.Instance.AddGazeListener(this);
    }

    public void OnGazeUpdate(GazeData gazeData)
    {
        //Add frame to GazeData cache handler
        gazeUtils.Update(gazeData);
    }

    void Update()
    {
        Point2D userPos = gazeUtils.GetLastValidUserPosition();

        if (null != userPos)
        {
            //mapping cam panning to 3:2 aspect ratio
            double tx = (userPos.X * 5) - 2.5f;
            double ty = (userPos.Y * 3) - 1.5f;

            //position camera X-Y plane and adjust distance
            eyesDistance = gazeUtils.GetLastValidUserDistance();
            depthMod = 2 * eyesDistance;

            Vector3 newPos = new Vector3(
                (float)tx,
                (float)ty,
                (float)(baseDist + depthMod));
            cam.transform.position = newPos;

            //camera 'look at' origo
            cam.transform.LookAt(Vector3.zero);

            //tilt cam according to eye angle
            double angle = gazeUtils.GetLastValidEyesAngle();
            cam.transform.eulerAngles = new Vector3(cam.transform.eulerAngles.x, cam.transform.eulerAngles.y, cam.transform.eulerAngles.z + (float)angle);
        }

        Point2D gazeCoords = gazeUtils.GetLastValidSmoothedGazeCoordinates();

        if (null != gazeCoords)
        {
            //map gaze indicator
            Point2D gp = UnityGazeUtils.getGazeCoordsToUnityWindowCoords(gazeCoords);

            Vector3 screenPoint = new Vector3((float)gp.X, (float)gp.Y, cam.nearClipPlane + .1f);

            Vector3 planeCoord = cam.ScreenToWorldPoint(screenPoint);
            gazeIndicator.transform.position = planeCoord;

            //handle collision detection
            checkGazeCollision(screenPoint);
        }

        //handle keypress
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            Application.LoadLevel(0);
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
