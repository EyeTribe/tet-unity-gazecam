using System;
using System.Runtime.Serialization;

namespace DotNetMatrix
{
	
	/// <summary>QR Decomposition.
	/// For an m-by-n matrix A with m >= n, the QR decomposition is an m-by-n
	/// orthogonal matrix Q and an n-by-n upper triangular matrix R so that
	/// A = Q*R.
	/// 
	/// The QR decompostion always exists, even if the matrix does not have
	/// full rank, so the constructor will never fail.  The primary use of the
	/// QR decomposition is in the least squares solution of nonsquare systems
	/// of simultaneous linear equations.  This will fail if IsFullRank()
	/// returns false.
	/// </summary>
	
	[Serializable]
	public class QRDecomposition : System.Runtime.Serialization.ISerializable
	{
		#region Class variables
		
		/// <summary>Array for internal storage of decomposition.
		/// @serial internal array storage.
		/// </summary>
		private double[][] QR;
		
		/// <summary>Row and column dimensions.
		/// @serial column dimension.
		/// @serial row dimension.
		/// </summary>
		private int m, n;
		
		/// <summary>Array for internal storage of diagonal of R.
		/// @serial diagonal of R.
		/// </summary>
		private double[] Rdiag;

		#endregion //  Class variables
		
		#region Constructor
		
		/// <summary>QR Decomposition, computed by Householder reflections.</summary>
		/// <param name="A">   Rectangular matrix
		/// </param>
		/// <returns>     Structure to access R and the Householder vectors and compute Q.
		/// </returns>
		
		public QRDecomposition(GeneralMatrix A)
		{
			// Initialize.
			QR = A.ArrayCopy;
			m = A.RowDimension;
			n = A.ColumnDimension;
			Rdiag = new double[n];
			
			// Main loop.
			for (int k = 0; k < n; k++)
			{
				// Compute 2-norm of k-th column without under/overflow.
				double nrm = 0;
				for (int i = k; i < m; i++)
				{
					nrm = Maths.Hypot(nrm, QR[i][k]);
				}
				
				if (nrm != 0.0)
				{
					// Form k-th Householder vector.
					if (QR[k][k] < 0)
					{
						nrm = - nrm;
					}
					for (int i = k; i < m; i++)
					{
						QR[i][k] /= nrm;
					}
					QR[k][k] += 1.0;
					
					// Apply transformation to remaining columns.
					for (int j = k + 1; j < n; j++)
					{
						double s = 0.0;
						for (int i = k; i < m; i++)
						{
							s += QR[i][k] * QR[i][j];
						}
						s = (- s) / QR[k][k];
						for (int i = k; i < m; i++)
						{
							QR[i][j] += s * QR[i][k];
						}
					}
				}
				Rdiag[k] = - nrm;
			}
		}

		#endregion //  Constructor
		
		#region Public Properties

		/// <summary>Is the matrix full rank?</summary>
		/// <returns>     true if R, and hence A, has full rank.
		/// </returns>
		virtual public bool FullRank
		{
			get
			{
				for (int j = 0; j < n; j++)
				{
					if (Rdiag[j] == 0)
						return false;
				}
				return true;
			}
		}

		/// <summary>Return the Householder vectors</summary>
		/// <returns>     Lower trapezoidal matrix whose columns define the reflections
		/// </returns>
		virtual public GeneralMatrix H
		{
			get
			{
				GeneralMatrix X = new GeneralMatrix(m, n);
				double[][] H = X.Array;
				for (int i = 0; i < m; i++)
				{
					for (int j = 0; j < n; j++)
					{
						if (i >= j)
						{
							H[i][j] = QR[i][j];
						}
						else
						{
							H[i][j] = 0.0;
						}
					}
				}
				return X;
			}
			
		}

		/// <summary>Return the upper triangular factor</summary>
		/// <returns>     R
		/// </returns>
		virtual public GeneralMatrix R
		{
			get
			{
				GeneralMatrix X = new GeneralMatrix(n, n);
				double[][] R = X.Array;
				for (int i = 0; i < n; i++)
				{
					for (int j = 0; j < n; j++)
					{
						if (i < j)
						{
							R[i][j] = QR[i][j];
						}
						else if (i == j)
						{
							R[i][j] = Rdiag[i];
						}
						else
						{
							R[i][j] = 0.0;
						}
					}
				}
				return X;
			}
		}

		/// <summary>Generate and return the (economy-sized) orthogonal factor</summary>
		/// <returns>     Q
		/// </returns>
		virtual public GeneralMatrix Q
		{
			get
			{
				GeneralMatrix X = new GeneralMatrix(m, n);
				double[][] Q = X.Array;
				for (int k = n - 1; k >= 0; k--)
				{
					for (int i = 0; i < m; i++)
					{
						Q[i][k] = 0.0;
					}
					Q[k][k] = 1.0;
					for (int j = k; j < n; j++)
					{
						if (QR[k][k] != 0)
						{
							double s = 0.0;
							for (int i = k; i < m; i++)
							{
								s += QR[i][k] * Q[i][j];
							}
							s = (- s) / QR[k][k];
							for (int i = k; i < m; i++)
							{
								Q[i][j] += s * QR[i][k];
							}
						}
					}
				}
				return X;
			}
		}
		#endregion //  Public Properties
		
		#region Public Methods
		
		/// <summary>Least squares solution of A*X = B</summary>
		/// <param name="B">   A Matrix with as many rows as A and any number of columns.
		/// </param>
		/// <returns>     X that minimizes the two norm of Q*R*X-B.
		/// </returns>
		/// <exception cref="System.ArgumentException"> Matrix row dimensions must agree.
		/// </exception>
		/// <exception cref="System.SystemException"> Matrix is rank deficient.
		/// </exception>
		
		public virtual GeneralMatrix Solve(GeneralMatrix B)
		{
			if (B.RowDimension != m)
			{
				throw new System.ArgumentException("GeneralMatrix row dimensions must agree.");
			}
			if (!this.FullRank)
			{
				throw new System.SystemException("Matrix is rank deficient.");
			}
			
			// Copy right hand side
			int nx = B.ColumnDimension;
			double[][] X = B.ArrayCopy;
			
			// Compute Y = transpose(Q)*B
			for (int k = 0; k < n; k++)
			{
				for (int j = 0; j < nx; j++)
				{
					double s = 0.0;
					for (int i = k; i < m; i++)
					{
						s += QR[i][k] * X[i][j];
					}
					s = (- s) / QR[k][k];
					for (int i = k; i < m; i++)
					{
						X[i][j] += s * QR[i][k];
					}
				}
			}
			// Solve R*X = Y;
			for (int k = n - 1; k >= 0; k--)
			{
				for (int j = 0; j < nx; j++)
				{
					X[k][j] /= Rdiag[k];
				}
				for (int i = 0; i < k; i++)
				{
					for (int j = 0; j < nx; j++)
					{
						X[i][j] -= X[k][j] * QR[i][k];
					}
				}
			}

			return (new GeneralMatrix(X, n, nx).GetMatrix(0, n - 1, 0, nx - 1));
		}

		#endregion //  Public Methods

		// A method called when serializing this class.
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) 
		{
		}
	}
}