/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using UnityEngine;
using System.Collections;
using TETCSharpClient;
using TETCSharpClient.Data;
using Assets.Scripts;
using System;
using System.Collections.Generic;

/// <summary>
/// Component attached to 'Main Camera' of '/Scenes/calib_scene.unity'.
/// This script handles the main menu GUI and the process of calibrating the 
/// EyeTribe Server.
/// </summary>
public class CalibCamera : MonoBehaviour, IGazeListener, ICalibrationProcessHandler
{
    private Camera cam;

    private GameObject leftEye;
    private GameObject rightEye;

    private double eyesDistance;
    private Vector3 eyeBaseScale;
    private double depthMod;

    private GameObject calibPointGO;
    private Point2D calibPoint;

    private List<Point2D> calibrationPoints;

    private GazeDataValidator gazeUtils;

    public delegate void Callback();
    private Queue<Callback> _callbackQueue;

    private const int NUM_MAX_CALIBRATION_ATTEMPTS = 3;
    private const int NUM_MAX_RESAMPLE_POINTS = 4;
    private int resampleCount;

    void Start()
    {
        //Stay in landscape
        Screen.autorotateToPortrait = false;

        //fetches scene object handles
        cam = GetComponent<Camera>();

        leftEye = GameObject.FindGameObjectWithTag("leftEye");
        rightEye = GameObject.FindGameObjectWithTag("rightEye");
        eyeBaseScale = leftEye.transform.localScale;

        calibPointGO = GameObject.FindGameObjectWithTag("CalibPoint");
        calibPointGO.transform.position = Vector3.zero;
        SetRendererEnabled(calibPointGO, false);

        //preprare calibration point container
        calibrationPoints = new List<Point2D>();

        //init call back queue
        _callbackQueue = new Queue<Callback>();

        //initialising GazeData stabilizer
        gazeUtils = new GazeDataValidator(30);
		
        //activate C# TET client, default port
        GazeManager.Instance.Activate
        (
            GazeManager.ApiVersion.VERSION_1_0,
            GazeManager.ClientMode.Push
        );

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
        if (!GazeManager.Instance.IsCalibrating)
        {
            Point2D userPos = gazeUtils.GetLastValidUserPosition();

            if (null != userPos)
            {

                //Make eyes visible
                if (!leftEye.renderer.enabled)
                    leftEye.renderer.enabled = true;
                if (!rightEye.renderer.enabled)
                    rightEye.renderer.enabled = true;

                //Set eyes size based on distance
                eyesDistance = gazeUtils.GetLastValidUserDistance();
                depthMod = eyesDistance * .5f;
                Vector3 scaleVec = new Vector3((float)(depthMod), (float)(depthMod), (float)eyeBaseScale.z);

                Eye left = gazeUtils.GetLastValidLeftEye();
                Eye right = gazeUtils.GetLastValidRightEye();

                double angle = gazeUtils.GetLastValidEyesAngle();

                if (null != left)
                {
                    //position GO based on screen coordinates
                    Point2D gp = UnityGazeUtils.getRelativeToScreenSpace(left.PupilCenterCoordinates);
                    PositionGOFromScreenCoords(leftEye, gp);
                    leftEye.transform.localScale = scaleVec;
                    leftEye.transform.eulerAngles = new Vector3(leftEye.transform.eulerAngles.x, leftEye.transform.eulerAngles.y, (float)angle);
                }

                if (null != right)
                {
                    //position GO based on screen coordinates
                    Point2D gp = UnityGazeUtils.getRelativeToScreenSpace(right.PupilCenterCoordinates);
                    PositionGOFromScreenCoords(rightEye, gp);
                    rightEye.transform.localScale = scaleVec;
                    rightEye.transform.eulerAngles = new Vector3(rightEye.transform.eulerAngles.x, rightEye.transform.eulerAngles.y, (float)angle);
                }
            }
        }
        else
        {
            //Make eyes invisible eyes
            if (leftEye.renderer.enabled)
                leftEye.renderer.enabled = false;
            if (rightEye.renderer.enabled)
                rightEye.renderer.enabled = false;
        }

        lock (_callbackQueue)
        {
            //we handle queued callback in the update loop
            while (_callbackQueue.Count > 0)
                _callbackQueue.Dequeue()();
        }

        //handle keypress
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void OnGUI()
    {
        //Setting up main menu GUI
        int padding = 10;
        int btnHeight;
        int btnWidth;

        int numBtns = 0;
        if (!GazeManager.Instance.IsActivated)
            ++numBtns;
        if (!GazeManager.Instance.IsCalibrating)
            ++numBtns;
        if (GazeManager.Instance.IsCalibrated && !GazeManager.Instance.IsCalibrating)
            ++numBtns;

        if (numBtns > 0)
        {
            int width = (int)(Screen.width * .2f);
            int height = (int)(Screen.height * .2f);
            int btnPadding = (int)(height * .3f);
            btnHeight = height - btnPadding - btnPadding;
            btnWidth = width - btnPadding - btnPadding;
            int x = (int)((Screen.width - width) / 2);
            height = btnPadding + ((btnHeight + btnPadding) * numBtns); //adjust size to num btns
            int y = (int)((Screen.height - height) / 2);

            string boxText = "The Eye Tribe - Unity Sample\n";

            //add calibration rating if available
            if (GazeManager.Instance.IsCalibrated)
            {
                y += 10;

                string calibText;
                int rating;
                CalibrationResult result = GazeManager.Instance.LastCalibrationResult;
                CalibrationRatingFunction(result, out rating, out calibText);
                boxText += "\nCalibration Result: " + calibText;
            }

            GUI.Box(new Rect(x, y, width, height), boxText);

            if (!GazeManager.Instance.IsActivated)
            {
                String btnText = "Reconnect to server";

                if (GUI.Button(new Rect(x + btnPadding, y + btnPadding, btnWidth, btnHeight), btnText))
                {
                    //activate C# TET client, default port
                    GazeManager.Instance.Activate
                    (
                        GazeManager.ApiVersion.VERSION_1_0,
                        GazeManager.ClientMode.Push
                    );
                }

                y += (btnPadding + btnHeight);
            }

            if (!GazeManager.Instance.IsCalibrating)
            {
                String btnText = GazeManager.Instance.IsCalibrated ? "Re-Calibrate" : "Calibrate";

                if (GUI.Button(new Rect(x + btnPadding, y + btnPadding, btnWidth, btnHeight), btnText))
                {
                    //Start new calibration
                    GenerateCalibrationPoints();
                    GazeManager.Instance.CalibrationStart(9, this);
                }

                y += (btnPadding + btnHeight);
            }

            if (GazeManager.Instance.IsCalibrated && !GazeManager.Instance.IsCalibrating)
            {
                String btnText = "Start GazeCam";

                if (GUI.Button(new Rect(x + btnPadding, y + btnPadding, btnWidth, btnHeight), btnText))
                {
                    Application.LoadLevel(1);
                }
            }
        }

        btnWidth = 160;
        btnHeight = 40;

        if (GUI.Button(new Rect(padding, padding, btnWidth, btnHeight), "Press to Exit"))
        {
            Application.Quit();
        }
    }

    void OnApplicationQuit()
    {
        GazeManager.Instance.CalibrationAbort();
        GazeManager.Instance.RemoveGazeListener(this);
        GazeManager.Instance.Deactivate();
    }

    public void OnCalibrationStarted()
    {
        //Handle on main UI thread
        QueueCallback(new Callback(delegate
        {
            Invoke("showNextCalibrationPoint", 1);
        }));
    }

    public void OnCalibrationProgress(double progress)
    {
        //Called every time a new calibration point have been sampled
    }

    public void OnCalibrationProcessing()
    {
        //Called when the calculation of the calibration results begins
    }

    public void OnCalibrationResult(CalibrationResult calibResult)
    {
        //Should we resample?
        if (!calibResult.Result)
        {
            //Evaluate results
            foreach (var calPoint in calibResult.Calibpoints)
            {
                if (calPoint.State == CalibrationPoint.STATE_RESAMPLE || calPoint.State == CalibrationPoint.STATE_NO_DATA)
                {
                    calibrationPoints.Add(new Point2D(calPoint.Coordinates.X, calPoint.Coordinates.Y));
                }
            }

            //Should we abort?
            if (resampleCount++ >= NUM_MAX_CALIBRATION_ATTEMPTS || calibrationPoints.Count >= NUM_MAX_RESAMPLE_POINTS)
            {
                calibrationPoints.Clear();
                GazeManager.Instance.CalibrationAbort();
                return;
            }

            //Handle on main UI thread
            QueueCallback(new Callback(delegate
            {
                Invoke("showNextCalibrationPoint", .25f);
            }));
        }
        else
        {
            //Handle on main UI thread
            QueueCallback(new Callback(delegate
            {
                Application.LoadLevel(1);
            }));
        }
    }

    private void shortDelay()
    {
        GazeManager.Instance.CalibrationPointEnd();

        //disable cp
        SetRendererEnabled(calibPointGO, false);

        //short delay before calling next cp
        if (calibrationPoints.Count > 0)
            Invoke("showNextCalibrationPoint", .25f);
    }

    private void showNextCalibrationPoint()
    {
        if (calibrationPoints.Count > 0)
        {
            //fetch next calibration point
            calibPoint = calibrationPoints[0];
            calibrationPoints.RemoveAt(0);

            //position GO based on screen coordinates
            PositionGOFromScreenCoords(calibPointGO, calibPoint);

            //enable cp
            SetRendererEnabled(calibPointGO, true);

            //short delay allowing eye to settle before sampling
            Invoke("sampleCalibrationPoint", .25f);

            //call pause after sampling
            Invoke("shortDelay", 1.5f);
        }
    }

    private void sampleCalibrationPoint()
    {
        GazeManager.Instance.CalibrationPointStart((int)Math.Round(calibPoint.X), (int)Math.Round(calibPoint.Y));
    }

    private void GenerateCalibrationPoints()
    {
        // create 9 calib points according to window size
        var padding = (double)Screen.height * .15f;
        var halfWidth = Screen.width * .5f;
        var halfHeight = Screen.height * .5f;

        calibrationPoints.Clear();

        calibrationPoints.Add(new Point2D(padding, padding));
        calibrationPoints.Add(new Point2D(padding, halfHeight));
        calibrationPoints.Add(new Point2D(padding, Screen.height - padding));

        calibrationPoints.Add(new Point2D(halfWidth, padding));
        calibrationPoints.Add(new Point2D(halfWidth, halfHeight));
        calibrationPoints.Add(new Point2D(halfWidth, Screen.height - padding));

        calibrationPoints.Add(new Point2D(Screen.width - padding, padding));
        calibrationPoints.Add(new Point2D(Screen.width - padding, halfHeight));
        calibrationPoints.Add(new Point2D(Screen.width - padding, Screen.height - padding));

        //Randomize calibration points
        Shuffle<Point2D>(calibrationPoints);
    }

    private void Shuffle<T>(IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void PositionGOFromScreenCoords(GameObject go, Point2D gp)
    {
        //convert to Unity bottom right origo
        Vector3 clone = new Vector3((float)gp.X, (float)(Screen.height - gp.Y), 0);

        //center align calib asset on point
        clone.x = clone.x - (go.transform.localScale.x / 2);
        clone.y = clone.y - (go.transform.localScale.y / 2);

        //map screen to world coords
        Vector3 cpWorld = cam.ScreenToWorldPoint(clone);

        //retain depth info
        cpWorld.z = go.transform.position.z;

        go.transform.position = cpWorld;
    }

    private void SetRendererEnabled(GameObject go, bool isEnabled)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.enabled = isEnabled;
        }
        go.renderer.enabled = isEnabled;
    }


    /// <summary>
    /// Simple rating of a given calibration.
    /// </summary>
    /// <param name="result">Any given CalibrationResult</param>
    /// <param name="rating">A number between 1 - 5 where 5 is the best othervise -1.</param>
    /// <param name="strRating">A string with a rating name othervise ERROR.</param>
    public void CalibrationRatingFunction(CalibrationResult result, out int rating, out string strRating)
    {
        if (result == null)
        {
            rating = -1;
            strRating = "ERROR";
            return;
        }
        if (result.AverageErrorDegree < 0.5)
        {
            rating = 5;
            strRating = "PERFECT";
            return;
        }
        if (result.AverageErrorDegree < 0.7)
        {
            rating = 4;
            strRating = "GOOD";
            return;
        }
        if (result.AverageErrorDegree < 1)
        {
            rating = 3;
            strRating = "MODERATE";
            return;
        }
        if (result.AverageErrorDegree < 1.5)
        {
            rating = 2;
            strRating = "POOR";
            return;
        }
        rating = 1;
        strRating = "REDO";
    }

    /// <summary>
    /// Utility method for adding callback tasks to a queue
    /// that will eventually be handle in the Unity game loop 
    /// method 'Update()'.
    /// </summary>
    public void QueueCallback(Callback newTask)
    {
        lock (_callbackQueue)
        {
            _callbackQueue.Enqueue(newTask);
        }
    }
}
