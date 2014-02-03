using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TETCSharpClient;
using TETCSharpClient.Data;
using UnityEngine;

namespace Assets.Scripts
{
    class UnityGazeUtils : GazeUtils
    {
        /// <summary>
        /// Maps a GazeData gaze point (RawCoordinates or SmoothedCoordinates) to Unity screen space. 
        /// Note that gaze points have origo in top left corner, whilst Unity uses lower left.
        /// </summary>
        /// <param name="gp"/>gaze point to map</param>
        /// <returns>2d point mapped to unity window space</returns>
        public static Point2D getGazeCoordsToUnityWindowCoords(Point2D gp)
        {
            double rx = gp.X * ((double)Screen.width / GazeManager.Instance.ScreenResolutionWidth);
            double ry = (GazeManager.Instance.ScreenResolutionHeight - gp.Y) * ((double)Screen.height / GazeManager.Instance.ScreenResolutionHeight);

            return new Point2D(rx, ry);
        }

        /// <summary>
        /// Convert a Point2D to Unity vector.
        /// </summary>
        /// <param name="gp"/>gaze point to convert</param>
        /// <returns>a vector representation of point</returns>
        public static Vector2 Point2DToVec2(Point2D gp)
        {
            return new Vector2((float)gp.X, (float)gp.Y);
        }

        /// <summary>
        /// Converts a relative point to screen point in pixels using Unity classes
        /// </summary>
        public static Point2D getRelativeToScreenSpace(Point2D gp)
        {
            return getRelativeToScreenSpace(gp, Screen.width, Screen.height);
        }
    }
}
