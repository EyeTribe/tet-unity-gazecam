using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TETCSharpClient.Data;
using UnityEngine;
using FilterUtils;

namespace Assets.Scripts
{
    /// <summary>
    /// Utility class that maintains a run-time cache of GazeData frames. Based on the cache 
    /// the class analyzes the frame history and finds the currently valid gaze data.
    /// Use this class to avoid the 'glitch' effect of occasional poor tracking.
    /// </summary>
    class GazeDataValidator
    {
        private double _MinimumEyesDistance = 0.1f;
        private double _MaximumEyesDistance = 0.3f;
        private FixedSizeQueue<GazeData> _Frames;
        private Eye _LastValidLeftEye;
        private Eye _LastValidRightEye;
        private Point2D _LastValidRawGazeCoords;
        private Point2D _LastValidSmoothedGazeCoords;
        private Point2D _LastValidUserPosition;
        private double _LastValidEyeDistance;
        private double _LastValidInterEyes;
        private double _LastValidEyeAngle;
        private double _baseDist;

        // The head pose filtering stuff
        private MotionFilter filteredPoseLeftEye;
        private MotionFilter filteredPoseRightEye;
        private Vector3 _filteredHeadPose;

        public GazeDataValidator (int queueLength)
        {
            _Frames = new FixedSizeQueue<GazeData> (queueLength);
            _LastValidUserPosition = new Point2D ();

            // Initialize the motion filters on both eyes
            filteredPoseLeftEye = new MotionFilter (1, .1);
            filteredPoseRightEye = new MotionFilter (1, .1);
            _LastValidInterEyes = 10;
        }

        public void Update (GazeData frame)
        {
            _Frames.Enqueue (frame);

            // update valid gazedata based on store
            Eye right = null, left = null;
            Point2D gazeCoords = null;
            Point2D gazeCoordsSmooth = null;
            GazeData gd;
            for (int i = _Frames.Count; --i >= 0;) {
                gd = _Frames.ElementAt (i);

                // if no tracking problems, then cache eye data
                if ((gd.State & GazeData.STATE_TRACKING_FAIL) == 0 && (gd.State & GazeData.STATE_TRACKING_LOST) == 0) {
                    // If both eyes are valid (and not observed before), update the _LastValidInterEyes
                    if (null == left && null == right &&
                        null != gd.RightEye && gd.RightEye.PupilCenterCoordinates.X != 0 && gd.RightEye.PupilCenterCoordinates.Y != 0 
                        && null != gd.LeftEye && gd.LeftEye.PupilCenterCoordinates.X != 0 && gd.LeftEye.PupilCenterCoordinates.Y != 0)
                    {
                        double newDistance  = Point2DDistance (_LastValidLeftEye, _LastValidRightEye);
                        _LastValidInterEyes = Math.Max (Math.Min (newDistance, _MaximumEyesDistance), _MinimumEyesDistance);
                    }

                    // Valid left eye is found --> update
                    if (null == left && null != gd.LeftEye && gd.LeftEye.PupilCenterCoordinates.X != 0 && gd.LeftEye.PupilCenterCoordinates.Y != 0)
                        left = gd.LeftEye;

                    // Valid right eye is found --> update
                    if (null == right && null != gd.RightEye && gd.RightEye.PupilCenterCoordinates.X != 0 && gd.RightEye.PupilCenterCoordinates.Y != 0)
                        right = gd.RightEye;                        
                }

                // if gaze coordinates available, cache both raw and smoothed
                if (/*(gd.State & GazeData.STATE_TRACKING_GAZE) != 0 && */null == gazeCoords && gd.RawCoordinates.X != 0 && gd.RawCoordinates.Y != 0) {
                    gazeCoords = gd.RawCoordinates;
                    gazeCoordsSmooth = gd.SmoothedCoordinates;
                }

                // break loop if valid values found
                if (null != right && null != left && null != gazeCoords)
                    break;
            }

            if (null != left)
                _LastValidLeftEye = left;

            if (null != right)
                _LastValidRightEye = right;

            if (null != gazeCoords) {
                _LastValidRawGazeCoords = gazeCoords;
                _LastValidSmoothedGazeCoords = gazeCoordsSmooth;
            }

            // Update user position values if needed data is valid
            if (null != _LastValidLeftEye && null != _LastValidRightEye) {
                // Update raw user position
                lock (_LastValidUserPosition) {
                    _LastValidUserPosition.X = (_LastValidLeftEye.PupilCenterCoordinates.X + _LastValidRightEye.PupilCenterCoordinates.X) / 2;
                    _LastValidUserPosition.Y = (_LastValidLeftEye.PupilCenterCoordinates.Y + _LastValidRightEye.PupilCenterCoordinates.Y) / 2;

                }
               
                //update 'depth' measure
                double dist = Point2DDistance (_LastValidLeftEye, _LastValidRightEye);
        
                if (dist < _MinimumEyesDistance)
                    _MinimumEyesDistance = dist;
        
                if (dist > _MaximumEyesDistance)
                    _MaximumEyesDistance = dist;
        
                _LastValidEyeDistance   = dist / (_MaximumEyesDistance - _MinimumEyesDistance);
        
                //update angle
                _LastValidEyeAngle = ((180 / Math.PI * Math.Atan2 (_LastValidRightEye.PupilCenterCoordinates.Y - _LastValidLeftEye.PupilCenterCoordinates.Y,
                                                          _LastValidRightEye.PupilCenterCoordinates.X - _LastValidLeftEye.PupilCenterCoordinates.X)));

                // Update the filtered position for both eyes
                Vector3 correctedPoseLeft, correctedPoseRight;
                double confidenceLeft, confidenceRight;
                
                UpdatePoseLeftEye (out correctedPoseLeft, out confidenceLeft);
                UpdatePoseRightEye (out correctedPoseRight, out confidenceRight);
                
                // Update the head pose : 
                _filteredHeadPose = MotionFilter.MergePositions (correctedPoseLeft, confidenceLeft,
                                                                 correctedPoseRight, confidenceRight);
                
            } else if (null != _LastValidLeftEye && null == _LastValidRightEye) {
                
                // Only use the left eye to get the head pose
                Vector3 correctedPoseLeft;
                double confidenceLeft;
          
                UpdatePoseLeftEye (out correctedPoseLeft, out confidenceLeft);
          
                _filteredHeadPose = new Vector3 ((float) correctedPoseLeft [0] + 0.03F, 
                                                 (float) correctedPoseLeft [1], 
                                                 (float) correctedPoseLeft [2]);

            } else if (null == _LastValidLeftEye && null != _LastValidRightEye) {
                // Only use the right eye to get the head pose
                Vector3 correctedPoseRight;
                double confidenceRight;
          
                UpdatePoseLeftEye (out correctedPoseRight, out confidenceRight);
          
                _filteredHeadPose = new Vector3 ((float) correctedPoseRight [0] - 0.03F, 
                                                 (float) correctedPoseRight [1], 
                                                 (float) correctedPoseRight [2]);
            }
        }

        private double Point2DDistance (Eye ge1, Eye ge2)
        {
            return Math.Abs (Math.Sqrt (Math.Pow (ge2.PupilCenterCoordinates.X - ge1.PupilCenterCoordinates.X, 2) + Math.Pow (ge2.PupilCenterCoordinates.Y - ge1.PupilCenterCoordinates.Y, 2)));
        }

        // TODO: Crappy implementation down there, should have built a class to update those eye positions..
        private void UpdatePoseLeftEye (out Vector3 correctedPoseLeft, out double confidenceLeft)
        {

            Vector3 new3DPosLeft = UnityGazeUtils.BackProjectDepthPinhole (new Point2D (_LastValidLeftEye.PupilCenterCoordinates.X, _LastValidLeftEye.PupilCenterCoordinates.Y),
                                                                           _LastValidInterEyes);
      
            filteredPoseLeftEye.Predict (); // Propagate the previous measurement
            filteredPoseLeftEye.Correct (new3DPosLeft);     // Correct with the new observation
            filteredPoseLeftEye.GetPostState (out correctedPoseLeft, out confidenceLeft);   // Get the up-to-date estimation
        }

        private void UpdatePoseRightEye (out Vector3 correctedPoseRight, out double confidenceRight)
        {
      
            Vector3 new3DPosRight = UnityGazeUtils.BackProjectDepthPinhole (new Point2D (_LastValidRightEye.PupilCenterCoordinates.X, _LastValidRightEye.PupilCenterCoordinates.Y),
                                                                            _LastValidInterEyes);
      
            filteredPoseRightEye.Predict (); // Propagate the previous measurement
            filteredPoseRightEye.Correct (new3DPosRight);     // Correct with the new observation
            filteredPoseRightEye.GetPostState (out correctedPoseRight, out confidenceRight);  // Get the up-to-date estimation
        }
    
        public Vector3 GetFilteredHeadPose ()
        {
            return _filteredHeadPose;
        }

        public Point2D GetLastValidUserPosition ()
        {
            return _LastValidUserPosition;
        }

        public Point2D GetLastValidLeftEyePosition ()
        {
            if (null != _LastValidLeftEye) {
                return new Point2D (_LastValidLeftEye.PupilCenterCoordinates.X, _LastValidLeftEye.PupilCenterCoordinates.Y);
            } else {
                return new Point2D ();
            }
        }

        public Point2D GetLastValidRightEyePosition ()
        {
            if (null != _LastValidRightEye) {
                return new Point2D (_LastValidRightEye.PupilCenterCoordinates.X, _LastValidRightEye.PupilCenterCoordinates.Y);
            } else {
                return new Point2D ();
            }
        }

        public Eye GetLastValidLeftEye ()
        {
            return _LastValidLeftEye;
        }

        public Eye GetLastValidRightEye ()
        {
            return _LastValidRightEye;
        }

        public double GetLastValidUserDistance ()
        {
            return _LastValidEyeDistance;
        }

        public double GetLastValidEyesAngle ()
        {
            return _LastValidEyeAngle;
        }

        public Point2D GetLastValidRawGazeCoordinates ()
        {
            return _LastValidRawGazeCoords;
        }

        public Point2D GetLastValidSmoothedGazeCoordinates ()
        {
            return _LastValidSmoothedGazeCoords;
        }

        public void setBaseDist (double baseDist)
        {
            _baseDist = baseDist;
        }

        public void setSmoothing(float smoothing) {
            filteredPoseLeftEye.updateSmoothing(smoothing);
            filteredPoseRightEye.updateSmoothing(smoothing);
        }
    }

    class FixedSizeQueue<T> : Queue<T>
    {
        private int limit = -1;

        public int Limit {
            get { return limit; }
            set { limit = value; }
        }

        public FixedSizeQueue (int limit)
            : base(limit)
        {
            this.Limit = limit;
        }

        public new void Enqueue (T item)
        {
            while (this.Count >= this.Limit) {
                this.Dequeue ();
            }
            base.Enqueue (item);
        }
    }
}
