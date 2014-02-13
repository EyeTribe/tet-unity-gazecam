using DotNetMatrix;
using UnityEngine;

// Initial code of the KF from http://autospreader.wordpress.com/2011/01/17/a-c-kalman-filter-class/

namespace Filter.Utils
{
	// A motion filter which instanciates a KalmanFilter with a constant motion model
	public class MotionFilter 
	{
		// All we need to feed the Kalman Filter
		private GeneralMatrix f, b, u, q, h, r;
		private KalmanFilter KF;
		private double _timeDilution, _measurementAccuracy;
		private int _dim;
		
		// The motion filter constructor
		public MotionFilter(double measurementAccuracy, double timeDilution, int dim) {
			_measurementAccuracy 	= measurementAccuracy;
			_timeDilution 			= timeDilution;
			_dim					= dim;

			// ------
			// Define the matrices over there
			// F : motion model
			f = new GeneralMatrix(_dim, _dim, 0);
			for (int i=0; i<_dim; ++i) {
				f.SetElement(i,i,1);
			}

			// B : control input model
			b = new GeneralMatrix(_dim, _dim, 0);

			// U : control vector
			u = new GeneralMatrix(_dim, 1, 0);

			// Q : propagation noise
			q = new GeneralMatrix(_dim, _dim, 0);
			for (int i=0; i<_dim; ++i) {
				q.SetElement(i,i,_timeDilution);
			}

			// H : observation model -> simple identity
			h = new GeneralMatrix(_dim, _dim, 0);
			for (int i=0; i<_dim; ++i) {
				h.SetElement(i,i,1);
			}

			// R : observation noise
			r = new GeneralMatrix(_dim, _dim, 0);
			for (int i=0; i<_dim; ++i) {
				r.SetElement(i,i,_measurementAccuracy);
			}

			// Define the initial state and covariance :
			// TODO : define them with the first measurement !
			GeneralMatrix KFState = new GeneralMatrix(_dim, 1, 0);
			GeneralMatrix KFCovariance = new GeneralMatrix(_dim, _dim, 0);
			for (int i=0; i<_dim; ++i) {
				KFCovariance.SetElement (i,i,1);
			}

			// ------


			// Instanciate the KF
			KF = new KalmanFilter(f,b,u,q,h,r, KFState, KFCovariance);
		}

		// Deal with the conditionnal merging of the two eyeballs positions
		public static Vector3 MergePositions(Vector3 pose1, double confidence1, Vector3 pose2, double confidence2) {
			Vector3 mergedPose = new Vector3();
			float threshold = 10;
			float interEyesDistance = 0.06F;

			if  ((confidence2 == 0) || (confidence1/confidence2 > threshold)) {
				// Pick the pose from the left eye
				mergedPose = pose1;

				// TODO : add an offset to use the position in between the eyes as a reference

			} else if ((confidence1 == 0) || (confidence2/confidence1 > threshold)) {
				// Pick the pose from the right eye
				mergedPose = pose2;

				// TODO : add an offset to use the position in between the eyes as a reference

			} else {
				// Set the pose in between the two
				mergedPose = (pose1 + pose2)/2;
			}

			return mergedPose;
		}

		public void updateSmoothing(double new_confidence) {
			// Update the current confidence level in the measurements
			_measurementAccuracy = new_confidence;

			for (int i=0; i<_dim; ++i) {
				r.SetElement(i,i,_measurementAccuracy);
			}
		}

		public void Predict() {
			KF.Predict();
		}

		public void Correct(double[] newMeasure, int dim) {
			GeneralMatrix MeasureVec = new GeneralMatrix(newMeasure, dim);
			KF.Correct(MeasureVec);
		}

		public void GetPreState(out double[] PreState, out double confidence) {
			PreState = new double[_dim];
			for (int i=0; i<_dim; ++i) {
				PreState[i] = KF.X0.GetElement(i,0);
			}

			// TODO: Output a confidence level based on the current covariance
			confidence = KF.P0.GetElement(0,0);
		}

		public void GetPostState(out double[] PostState, out double confidence) {
			PostState = new double[_dim];
			for (int i=0; i<_dim; ++i) {
				PostState[i] = KF.State.GetElement(i,0);
			}
			
			// TODO: Output a confidence level based on the current covariance
			confidence = KF.Covariance.GetElement(0,0);
		}
	}

    public sealed class KalmanFilter
    {
        //System matrices
        public GeneralMatrix X0, P0;

        public GeneralMatrix F { get; private set; }
        public GeneralMatrix B { get; private set; }
        public GeneralMatrix U { get; private set; }
        public GeneralMatrix Q { get; private set; }
        public GeneralMatrix H { get; private set; }
        public GeneralMatrix R { get; private set; }

        public GeneralMatrix State { get; private set; } 
        public GeneralMatrix Covariance { get; private set; }  

        public KalmanFilter(GeneralMatrix f, GeneralMatrix b, GeneralMatrix u, GeneralMatrix q, GeneralMatrix h,
                            GeneralMatrix r, GeneralMatrix iState, GeneralMatrix iCovariance)
        {
            F = f;
            B = b;
            U = u;
            Q = q;
            H = h;
            R = r;

			State = iState;
			Covariance = iCovariance;
        }

        public void Predict()
        {
            X0 = F*State + (B*U);
            P0 = F*Covariance*F.Transpose() + Q;
        }

        public void Correct(GeneralMatrix z)
        {
            GeneralMatrix s = H*P0*H.Transpose() + R;
            GeneralMatrix k = P0*H.Transpose()*s.Inverse();
            State = X0 + (k*(z - (H*X0)));
            GeneralMatrix I = GeneralMatrix.Identity(P0.RowDimension, P0.ColumnDimension);
            Covariance = (I - k*H)*P0;
        }
    }
}