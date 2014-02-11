using DotNetMatrix;

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
			f = new GeneralMatrix(dim, dim, 0);
			for (int i=0; i<dim; ++i) {
				h.SetElement(i,i,1);
			}

			// B : control input model
			b = new GeneralMatrix(dim, dim, 0);

			// U : control vector
			u = new GeneralMatrix(dim, 1, 0);

			// Q : propagation noise
			q = new GeneralMatrix(dim, dim, 0);
			for (int i=0; i<dim; ++i) {
				q.SetElement(i,i,_timeDilution);
			}

			// H : observation model -> simple identity
			h = new GeneralMatrix(dim, dim, 0);
			for (int i=0; i<dim; ++i) {
				h.SetElement(i,i,1);
			}

			// R : observation noise
			r = new GeneralMatrix(dim, dim, 0);
			for (int i=0; i<dim; ++i) {
				r.SetElement(i,i,measurementAccuracy);
			}
			// ------

			// Instanciate the KF
			KF = new KalmanFilter(f,b,u,q,h,r);
		}

		public void Predict() {
			KF.Predict();
		}

		public void Correct(double[] newMeasure, int dim) {
			GeneralMatrix MeasureVec = new GeneralMatrix(newMeasure, dim);
			KF.Correct(MeasureVec);
		}

		public void GetPreState(out double[] PreState) {
			PreState = new double[_dim];

			for (int i=0; i<_dim; ++i) {
				PreState[i] = KF.X0.GetElement(i,0);
			}
		}

		public void GetPostState(out double[] PostState) {
			PostState = new double[_dim];
			
			for (int i=0; i<_dim; ++i) {
				PostState[i] = KF.State.GetElement(i,0);
			}		
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
                            GeneralMatrix r)
        {
            F = f;
            B = b;
            U = u;
            Q = q;
            H = h;
            R = r;
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