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
		/// Converts a coordinate on picture space to a 3D pose
		/// </summary>
		public static Vector3 BackProjectDepth(Point2D eyePictCoord, double eyesDistance, double baseDist) {
			
			//mapping cam panning to 3:2 aspect ratio
			double tx = (eyePictCoord.X * 5) - 2.5f;
			double ty = (eyePictCoord.Y * 3) - 1.5f;
			
			//position camera X-Y plane and adjust distance
			double depthMod = 2 * eyesDistance;
			
			return new Vector3((float)tx,
			                   (float)ty,
			                   (float)(baseDist + depthMod));
		}

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
		/// Convert a Unity Vector3 to a double[].
		/// </summary>
		/// <param name="gp"/>Vector to convert</param>
		/// <returns>double array</returns>
		public static double[] Vec3ToArray(Vector3 vec)
		{
			return new double[3]{vec.x, vec.y, vec.z};
		}

		/// <summary>
		/// Convert a double[3] to a Unity Vector.
		/// </summary>
		/// <param name="gp"/>Array to convert</param>
		/// <returns>Unity Vector3</returns>
		public static Vector3 ArrayToVec3(double[] array)
		{
			return new Vector3((float)array[0], (float)array[1], (float)array[2]);
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
