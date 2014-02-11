using System;
using DotNetMatrix;
namespace DotNetMatrix.test
{
	
	/// <summary>TestMatrix tests the functionality of the DotNetMatrix GeneralMatrix class and associated decompositions.
	/// <P>
	/// Run the test from the command line using
	/// <BLOCKQUOTE><PRE><CODE>
	/// DotNetMatrix.test.TestMatrix 
	/// </CODE></PRE></BLOCKQUOTE>
	/// Detailed output is provided indicating the functionality being tested
	/// and whether the functionality is correctly implemented.   Exception handling
	/// is also tested.  
	/// <P>
	/// The test is designed to run to completion and give a summary of any implementation errors
	/// encountered. The final output should be:
	/// <BLOCKQUOTE><PRE><CODE>
	/// TestMatrix completed.
	/// Total errors reported: n1
	/// Total warning reported: n2
	/// </CODE></PRE></BLOCKQUOTE>
	/// If the test does not run to completion, this indicates that there is a 
	/// substantial problem within the implementation that was not anticipated in the test design.  
	/// The stopping point should give an indication of where the problem exists.
	/// 
	/// </summary>
	public class TestMatrix
	{
		[STAThread]
		public static void  Main(System.String[] argv)
		{
			GeneralMatrix A, B, C, Z, O, I, R, S, X, SUB, M, T, SQ, DEF, SOL;
			int errorCount = 0;
			int warningCount = 0;
			double tmp;
			double[] columnwise = new double[]{1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0};
			double[] rowwise = new double[]{1.0, 4.0, 7.0, 10.0, 2.0, 5.0, 8.0, 11.0, 3.0, 6.0, 9.0, 12.0};
			double[][] avals = {new double[]{1.0, 4.0, 7.0, 10.0}, new double[]{2.0, 5.0, 8.0, 11.0}, new double[]{3.0, 6.0, 9.0, 12.0}};
			double[][] rankdef = avals;
			double[][] tvals = {new double[]{1.0, 2.0, 3.0}, new double[]{4.0, 5.0, 6.0}, new double[]{7.0, 8.0, 9.0}, new double[]{10.0, 11.0, 12.0}};
			double[][] subavals = {new double[]{5.0, 8.0, 11.0}, new double[]{6.0, 9.0, 12.0}};
			double[][] rvals = {new double[]{1.0, 4.0, 7.0}, new double[]{2.0, 5.0, 8.0, 11.0}, new double[]{3.0, 6.0, 9.0, 12.0}};
			double[][] pvals = {new double[]{1.0, 1.0, 1.0}, new double[]{1.0, 2.0, 3.0}, new double[]{1.0, 3.0, 6.0}};
			double[][] ivals = {new double[]{1.0, 0.0, 0.0, 0.0}, new double[]{0.0, 1.0, 0.0, 0.0}, new double[]{0.0, 0.0, 1.0, 0.0}};
			double[][] evals = {new double[]{0.0, 1.0, 0.0, 0.0}, new double[]{1.0, 0.0, 2e-7, 0.0}, new double[]{0.0, - 2e-7, 0.0, 1.0}, new double[]{0.0, 0.0, 1.0, 0.0}};
			double[][] square = {new double[]{166.0, 188.0, 210.0}, new double[]{188.0, 214.0, 240.0}, new double[]{210.0, 240.0, 270.0}};
			double[][] sqSolution = {new double[]{13.0}, new double[]{15.0}};
			double[][] condmat = {new double[]{1.0, 3.0}, new double[]{7.0, 9.0}};
			int rows = 3, cols = 4;
			int invalidld = 5; /* should trigger bad shape for construction with val */
			int raggedr = 0; /* (raggedr,raggedc) should be out of bounds in ragged array */
			int raggedc = 4;
			int validld = 3; /* leading dimension of intended test Matrices */
			int nonconformld = 4; /* leading dimension which is valid, but nonconforming */
			int ib = 1, ie = 2, jb = 1, je = 3; /* index ranges for sub GeneralMatrix */
			int[] rowindexset = new int[]{1, 2};
			int[] badrowindexset = new int[]{1, 3};
			int[] columnindexset = new int[]{1, 2, 3};
			int[] badcolumnindexset = new int[]{1, 2, 4};
			double columnsummax = 33.0;
			double rowsummax = 30.0;
			double sumofdiagonals = 15;
			double sumofsquares = 650;
			
			/// <summary>Constructors and constructor-like methods:
			/// double[], int
			/// double[][]  
			/// int, int
			/// int, int, double
			/// int, int, double[][]
			/// Create(double[][])
			/// Random(int,int)
			/// Identity(int)
			/// 
			/// </summary>
			
			print("\nTesting constructors and constructor-like methods...\n");
			try
			{
				/// <summary>check that exception is thrown in packed constructor with invalid length *</summary>
				A = new GeneralMatrix(columnwise, invalidld);
				errorCount = try_failure(errorCount, "Catch invalid length in packed constructor... ", "exception not thrown for invalid input");
			}
			catch (System.ArgumentException e)
			{
				try_success("Catch invalid length in packed constructor... ", e.Message);
			}
			try
			{
				/// <summary>check that exception is thrown in default constructor
				/// if input array is 'ragged' *
				/// </summary>
				A = new GeneralMatrix(rvals);
				tmp = A.GetElement(raggedr, raggedc);
			}
			catch (System.ArgumentException e)
			{
				try_success("Catch ragged input to default constructor... ", e.Message);
			}
			catch (System.IndexOutOfRangeException e)
			{
				errorCount = try_failure(errorCount, "Catch ragged input to constructor... ", "exception not thrown in construction...ArrayIndexOutOfBoundsException thrown later");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				/// <summary>check that exception is thrown in Create
				/// if input array is 'ragged' *
				/// </summary>
				A = GeneralMatrix.Create(rvals);
				tmp = A.GetElement(raggedr, raggedc);
			}
			catch (System.ArgumentException e)
			{
				try_success("Catch ragged input to Create... ", e.Message);
				System.Console.Out.WriteLine(e.Message);
			}
			catch (System.IndexOutOfRangeException e)
			{
				errorCount = try_failure(errorCount, "Catch ragged input to Create... ", "exception not thrown in construction...ArrayIndexOutOfBoundsException thrown later");
				System.Console.Out.WriteLine(e.Message);
			}
			
			A = new GeneralMatrix(columnwise, validld);
			B = new GeneralMatrix(avals);
			tmp = B.GetElement(0, 0);
			avals[0][0] = 0.0;
			C = B.Subtract(A);
			avals[0][0] = tmp;
			B = GeneralMatrix.Create(avals);
			tmp = B.GetElement(0, 0);
			avals[0][0] = 0.0;
			if ((tmp - B.GetElement(0, 0)) != 0.0)
			{
				/// <summary>check that Create behaves properly *</summary>
				errorCount = try_failure(errorCount, "Create... ", "Copy not effected... data visible outside");
			}
			else
			{
				try_success("Create... ", "");
			}
			avals[0][0] = columnwise[0];
			I = new GeneralMatrix(ivals);
			try
			{
				check(I, GeneralMatrix.Identity(3, 4));
				try_success("Identity... ", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "Identity... ", "Identity GeneralMatrix not successfully created");
				System.Console.Out.WriteLine(e.Message);
			}
			
			/// <summary>Access Methods:
			/// getColumnDimension()
			/// getRowDimension()
			/// getArray()
			/// getArrayCopy()
			/// getColumnPackedCopy()
			/// getRowPackedCopy()
			/// get(int,int)
			/// GetMatrix(int,int,int,int)
			/// GetMatrix(int,int,int[])
			/// GetMatrix(int[],int,int)
			/// GetMatrix(int[],int[])
			/// set(int,int,double)
			/// SetMatrix(int,int,int,int,GeneralMatrix)
			/// SetMatrix(int,int,int[],GeneralMatrix)
			/// SetMatrix(int[],int,int,GeneralMatrix)
			/// SetMatrix(int[],int[],GeneralMatrix)
			/// 
			/// </summary>
			
			print("\nTesting access methods...\n");
			
			/// <summary>Various get methods:
			/// 
			/// </summary>
			
			B = new GeneralMatrix(avals);
			if (B.RowDimension != rows)
			{
				errorCount = try_failure(errorCount, "getRowDimension... ", "");
			}
			else
			{
				try_success("getRowDimension... ", "");
			}
			if (B.ColumnDimension != cols)
			{
				errorCount = try_failure(errorCount, "getColumnDimension... ", "");
			}
			else
			{
				try_success("getColumnDimension... ", "");
			}
			B = new GeneralMatrix(avals);
			double[][] barray = B.Array;
			if (barray != avals)
			{
				errorCount = try_failure(errorCount, "getArray... ", "");
			}
			else
			{
				try_success("getArray... ", "");
			}
			barray = B.ArrayCopy;
			if (barray == avals)
			{
				errorCount = try_failure(errorCount, "getArrayCopy... ", "data not (deep) copied");
			}
			try
			{
				check(barray, avals);
				try_success("getArrayCopy... ", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "getArrayCopy... ", "data not successfully (deep) copied");
				System.Console.Out.WriteLine(e.Message);
			}
			double[] bpacked = B.ColumnPackedCopy;
			try
			{
				check(bpacked, columnwise);
				try_success("getColumnPackedCopy... ", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "getColumnPackedCopy... ", "data not successfully (deep) copied by columns");
				System.Console.Out.WriteLine(e.Message);
			}
			bpacked = B.RowPackedCopy;
			try
			{
				check(bpacked, rowwise);
				try_success("getRowPackedCopy... ", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "getRowPackedCopy... ", "data not successfully (deep) copied by rows");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				tmp = B.GetElement(B.RowDimension, B.ColumnDimension - 1);
				errorCount = try_failure(errorCount, "get(int,int)... ", "OutOfBoundsException expected but not thrown");
			}
			catch (System.IndexOutOfRangeException e)
			{
				System.Console.Out.WriteLine(e.Message);
				try
				{
					tmp = B.GetElement(B.RowDimension - 1, B.ColumnDimension);
					errorCount = try_failure(errorCount, "get(int,int)... ", "OutOfBoundsException expected but not thrown");
				}
				catch (System.IndexOutOfRangeException e1)
				{
					try_success("get(int,int)... OutofBoundsException... ", "");
					System.Console.Out.WriteLine(e1.Message);
				}
			}
			catch (System.ArgumentException e1)
			{
				errorCount = try_failure(errorCount, "get(int,int)... ", "OutOfBoundsException expected but not thrown");
				System.Console.Out.WriteLine(e1.Message);
			}
			try
			{
				if (B.GetElement(B.RowDimension - 1, B.ColumnDimension - 1) != avals[B.RowDimension - 1][B.ColumnDimension - 1])
				{
					errorCount = try_failure(errorCount, "get(int,int)... ", "GeneralMatrix entry (i,j) not successfully retreived");
				}
				else
				{
					try_success("get(int,int)... ", "");
				}
			}
			catch (System.IndexOutOfRangeException e)
			{
				errorCount = try_failure(errorCount, "get(int,int)... ", "Unexpected ArrayIndexOutOfBoundsException");
				System.Console.Out.WriteLine(e.Message);
			}
			SUB = new GeneralMatrix(subavals);
			try
			{
				M = B.GetMatrix(ib, ie + B.RowDimension + 1, jb, je);
				errorCount = try_failure(errorCount, "GetMatrix(int,int,int,int)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (System.IndexOutOfRangeException e)
			{
				System.Console.Out.WriteLine(e.Message);
				try
				{
					M = B.GetMatrix(ib, ie, jb, je + B.ColumnDimension + 1);
					errorCount = try_failure(errorCount, "GetMatrix(int,int,int,int)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (System.IndexOutOfRangeException e1)
				{
					try_success("GetMatrix(int,int,int,int)... ArrayIndexOutOfBoundsException... ", "");
					System.Console.Out.WriteLine(e1.Message);
				}
			}
			catch (System.ArgumentException e1)
			{
				errorCount = try_failure(errorCount, "GetMatrix(int,int,int,int)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				System.Console.Out.WriteLine(e1.Message);
			}
			try
			{
				M = B.GetMatrix(ib, ie, jb, je);
				try
				{
					check(SUB, M);
					try_success("GetMatrix(int,int,int,int)... ", "");
				}
				catch (System.SystemException e)
				{
					errorCount = try_failure(errorCount, "GetMatrix(int,int,int,int)... ", "submatrix not successfully retreived");
					System.Console.Out.WriteLine(e.Message);
				}
			}
			catch (System.IndexOutOfRangeException e)
			{
				errorCount = try_failure(errorCount, "GetMatrix(int,int,int,int)... ", "Unexpected ArrayIndexOutOfBoundsException");
				System.Console.Out.WriteLine(e.Message);
			}
			
			try
			{
				M = B.GetMatrix(ib, ie, badcolumnindexset);
				errorCount = try_failure(errorCount, "GetMatrix(int,int,int[])... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (System.IndexOutOfRangeException e)
			{
				System.Console.Out.WriteLine(e.Message);
				try
				{
					M = B.GetMatrix(ib, ie + B.RowDimension + 1, columnindexset);
					errorCount = try_failure(errorCount, "GetMatrix(int,int,int[])... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (System.IndexOutOfRangeException e1)
				{
					try_success("GetMatrix(int,int,int[])... ArrayIndexOutOfBoundsException... ", "");
					System.Console.Out.WriteLine(e1.Message);
				}
			}
			catch (System.ArgumentException e1)
			{
				errorCount = try_failure(errorCount, "GetMatrix(int,int,int[])... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				System.Console.Out.WriteLine(e1.Message);
			}
			try
			{
				M = B.GetMatrix(ib, ie, columnindexset);
				try
				{
					check(SUB, M);
					try_success("GetMatrix(int,int,int[])... ", "");
				}
				catch (System.SystemException e)
				{
					errorCount = try_failure(errorCount, "GetMatrix(int,int,int[])... ", "submatrix not successfully retreived");
					System.Console.Out.WriteLine(e.Message);
				}
			}
			catch (System.IndexOutOfRangeException e)
			{
				errorCount = try_failure(errorCount, "GetMatrix(int,int,int[])... ", "Unexpected ArrayIndexOutOfBoundsException");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				M = B.GetMatrix(badrowindexset, jb, je);
				errorCount = try_failure(errorCount, "GetMatrix(int[],int,int)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (System.IndexOutOfRangeException e)
			{
				System.Console.Out.WriteLine(e.Message);
				try
				{
					M = B.GetMatrix(rowindexset, jb, je + B.ColumnDimension + 1);
					errorCount = try_failure(errorCount, "GetMatrix(int[],int,int)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (System.IndexOutOfRangeException e1)
				{
					try_success("GetMatrix(int[],int,int)... ArrayIndexOutOfBoundsException... ", "");
					System.Console.Out.WriteLine(e1.Message);
				}
			}
			catch (System.ArgumentException e1)
			{
				errorCount = try_failure(errorCount, "GetMatrix(int[],int,int)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				System.Console.Out.WriteLine(e1.Message);
			}
			try
			{
				M = B.GetMatrix(rowindexset, jb, je);
				try
				{
					check(SUB, M);
					try_success("GetMatrix(int[],int,int)... ", "");
				}
				catch (System.SystemException e)
				{
					errorCount = try_failure(errorCount, "GetMatrix(int[],int,int)... ", "submatrix not successfully retreived");
					System.Console.Out.WriteLine(e.Message);
				}
			}
			catch (System.IndexOutOfRangeException e)
			{
				errorCount = try_failure(errorCount, "GetMatrix(int[],int,int)... ", "Unexpected ArrayIndexOutOfBoundsException");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				M = B.GetMatrix(badrowindexset, columnindexset);
				errorCount = try_failure(errorCount, "GetMatrix(int[],int[])... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (System.IndexOutOfRangeException e)
			{
				System.Console.Out.WriteLine(e.Message);
				try
				{
					M = B.GetMatrix(rowindexset, badcolumnindexset);
					errorCount = try_failure(errorCount, "GetMatrix(int[],int[])... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (System.IndexOutOfRangeException e1)
				{
					try_success("GetMatrix(int[],int[])... ArrayIndexOutOfBoundsException... ", "");
					System.Console.Out.WriteLine(e1.Message);
				}
			}
			catch (System.ArgumentException e1)
			{
				errorCount = try_failure(errorCount, "GetMatrix(int[],int[])... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				System.Console.Out.WriteLine(e1.Message);
			}
			try
			{
				M = B.GetMatrix(rowindexset, columnindexset);
				try
				{
					check(SUB, M);
					try_success("GetMatrix(int[],int[])... ", "");
				}
				catch (System.SystemException e)
				{
					errorCount = try_failure(errorCount, "GetMatrix(int[],int[])... ", "submatrix not successfully retreived");
					System.Console.Out.WriteLine(e.Message);
				}
			}
			catch (System.IndexOutOfRangeException e)
			{
				errorCount = try_failure(errorCount, "GetMatrix(int[],int[])... ", "Unexpected ArrayIndexOutOfBoundsException");
				System.Console.Out.WriteLine(e.Message);
			}
			
			/// <summary>Various set methods:
			/// 
			/// </summary>
			
			try
			{
				B.SetElement(B.RowDimension, B.ColumnDimension - 1, 0.0);
				errorCount = try_failure(errorCount, "set(int,int,double)... ", "OutOfBoundsException expected but not thrown");
			}
			catch (System.IndexOutOfRangeException e)
			{
				System.Console.Out.WriteLine(e.Message);
				try
				{
					B.SetElement(B.RowDimension - 1, B.ColumnDimension, 0.0);
					errorCount = try_failure(errorCount, "set(int,int,double)... ", "OutOfBoundsException expected but not thrown");
				}
				catch (System.IndexOutOfRangeException e1)
				{
					try_success("set(int,int,double)... OutofBoundsException... ", "");
					System.Console.Out.WriteLine(e1.Message);
				}
			}
			catch (System.ArgumentException e1)
			{
				errorCount = try_failure(errorCount, "set(int,int,double)... ", "OutOfBoundsException expected but not thrown");
				System.Console.Out.WriteLine(e1.Message);
			}
			try
			{
				B.SetElement(ib, jb, 0.0);
				tmp = B.GetElement(ib, jb);
				try
				{
					check(tmp, 0.0);
					try_success("set(int,int,double)... ", "");
				}
				catch (System.SystemException e)
				{
					errorCount = try_failure(errorCount, "set(int,int,double)... ", "GeneralMatrix element not successfully set");
					System.Console.Out.WriteLine(e.Message);
				}
			}
			catch (System.IndexOutOfRangeException e1)
			{
				errorCount = try_failure(errorCount, "set(int,int,double)... ", "Unexpected ArrayIndexOutOfBoundsException");
				System.Console.Out.WriteLine(e1.Message);
			}
			M = new GeneralMatrix(2, 3, 0.0);
			try
			{
				B.SetMatrix(ib, ie + B.RowDimension + 1, jb, je, M);
				errorCount = try_failure(errorCount, "SetMatrix(int,int,int,int,GeneralMatrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (System.IndexOutOfRangeException e)
			{
				System.Console.Out.WriteLine(e.Message);
				try
				{
					B.SetMatrix(ib, ie, jb, je + B.ColumnDimension + 1, M);
					errorCount = try_failure(errorCount, "SetMatrix(int,int,int,int,GeneralMatrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (System.IndexOutOfRangeException e1)
				{
					try_success("SetMatrix(int,int,int,int,GeneralMatrix)... ArrayIndexOutOfBoundsException... ", "");
					System.Console.Out.WriteLine(e1.Message);
				}
			}
			catch (System.ArgumentException e1)
			{
				errorCount = try_failure(errorCount, "SetMatrix(int,int,int,int,GeneralMatrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				System.Console.Out.WriteLine(e1.Message);
			}
			try
			{
				B.SetMatrix(ib, ie, jb, je, M);
				try
				{
					check(M.Subtract(B.GetMatrix(ib, ie, jb, je)), M);
					try_success("SetMatrix(int,int,int,int,GeneralMatrix)... ", "");
				}
				catch (System.SystemException e)
				{
					errorCount = try_failure(errorCount, "SetMatrix(int,int,int,int,GeneralMatrix)... ", "submatrix not successfully set");
					System.Console.Out.WriteLine(e.Message);
				}
				B.SetMatrix(ib, ie, jb, je, SUB);
			}
			catch (System.IndexOutOfRangeException e1)
			{
				errorCount = try_failure(errorCount, "SetMatrix(int,int,int,int,GeneralMatrix)... ", "Unexpected ArrayIndexOutOfBoundsException");
				System.Console.Out.WriteLine(e1.Message);
			}
			try
			{
				B.SetMatrix(ib, ie + B.RowDimension + 1, columnindexset, M);
				errorCount = try_failure(errorCount, "SetMatrix(int,int,int[],GeneralMatrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (System.IndexOutOfRangeException e)
			{
				System.Console.Out.WriteLine(e.Message);
				try
				{
					B.SetMatrix(ib, ie, badcolumnindexset, M);
					errorCount = try_failure(errorCount, "SetMatrix(int,int,int[],GeneralMatrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (System.IndexOutOfRangeException e1)
				{
					try_success("SetMatrix(int,int,int[],GeneralMatrix)... ArrayIndexOutOfBoundsException... ", "");
					System.Console.Out.WriteLine(e1.Message);
				}
			}
			catch (System.ArgumentException e1)
			{
				errorCount = try_failure(errorCount, "SetMatrix(int,int,int[],GeneralMatrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				System.Console.Out.WriteLine(e1.Message);
			}
			try
			{
				B.SetMatrix(ib, ie, columnindexset, M);
				try
				{
					check(M.Subtract(B.GetMatrix(ib, ie, columnindexset)), M);
					try_success("SetMatrix(int,int,int[],GeneralMatrix)... ", "");
				}
				catch (System.SystemException e)
				{
					errorCount = try_failure(errorCount, "SetMatrix(int,int,int[],GeneralMatrix)... ", "submatrix not successfully set");
					System.Console.Out.WriteLine(e.Message);
				}
				B.SetMatrix(ib, ie, jb, je, SUB);
			}
			catch (System.IndexOutOfRangeException e1)
			{
				errorCount = try_failure(errorCount, "SetMatrix(int,int,int[],GeneralMatrix)... ", "Unexpected ArrayIndexOutOfBoundsException");
				System.Console.Out.WriteLine(e1.Message);
			}
			try
			{
				B.SetMatrix(rowindexset, jb, je + B.ColumnDimension + 1, M);
				errorCount = try_failure(errorCount, "SetMatrix(int[],int,int,GeneralMatrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (System.IndexOutOfRangeException e)
			{
				System.Console.Out.WriteLine(e.Message);
				try
				{
					B.SetMatrix(badrowindexset, jb, je, M);
					errorCount = try_failure(errorCount, "SetMatrix(int[],int,int,GeneralMatrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (System.IndexOutOfRangeException e1)
				{
					try_success("SetMatrix(int[],int,int,GeneralMatrix)... ArrayIndexOutOfBoundsException... ", "");
					System.Console.Out.WriteLine(e1.Message);
				}
			}
			catch (System.ArgumentException e1)
			{
				errorCount = try_failure(errorCount, "SetMatrix(int[],int,int,GeneralMatrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				System.Console.Out.WriteLine(e1.Message);
			}
			try
			{
				B.SetMatrix(rowindexset, jb, je, M);
				try
				{
					check(M.Subtract(B.GetMatrix(rowindexset, jb, je)), M);
					try_success("SetMatrix(int[],int,int,GeneralMatrix)... ", "");
				}
				catch (System.SystemException e)
				{
					errorCount = try_failure(errorCount, "SetMatrix(int[],int,int,GeneralMatrix)... ", "submatrix not successfully set");
					System.Console.Out.WriteLine(e.Message);
				}
				B.SetMatrix(ib, ie, jb, je, SUB);
			}
			catch (System.IndexOutOfRangeException e1)
			{
				errorCount = try_failure(errorCount, "SetMatrix(int[],int,int,GeneralMatrix)... ", "Unexpected ArrayIndexOutOfBoundsException");
				System.Console.Out.WriteLine(e1.Message);
			}
			try
			{
				B.SetMatrix(rowindexset, badcolumnindexset, M);
				errorCount = try_failure(errorCount, "SetMatrix(int[],int[],GeneralMatrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
			}
			catch (System.IndexOutOfRangeException e)
			{
				System.Console.Out.WriteLine(e.Message);
				try
				{
					B.SetMatrix(badrowindexset, columnindexset, M);
					errorCount = try_failure(errorCount, "SetMatrix(int[],int[],GeneralMatrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				}
				catch (System.IndexOutOfRangeException e1)
				{
					try_success("SetMatrix(int[],int[],GeneralMatrix)... ArrayIndexOutOfBoundsException... ", "");
					System.Console.Out.WriteLine(e1.Message);
				}
			}
			catch (System.ArgumentException e1)
			{
				errorCount = try_failure(errorCount, "SetMatrix(int[],int[],GeneralMatrix)... ", "ArrayIndexOutOfBoundsException expected but not thrown");
				System.Console.Out.WriteLine(e1.Message);
			}
			try
			{
				B.SetMatrix(rowindexset, columnindexset, M);
				try
				{
					check(M.Subtract(B.GetMatrix(rowindexset, columnindexset)), M);
					try_success("SetMatrix(int[],int[],GeneralMatrix)... ", "");
				}
				catch (System.SystemException e)
				{
					errorCount = try_failure(errorCount, "SetMatrix(int[],int[],GeneralMatrix)... ", "submatrix not successfully set");
					System.Console.Out.WriteLine(e.Message);
				}
			}
			catch (System.IndexOutOfRangeException e1)
			{
				errorCount = try_failure(errorCount, "SetMatrix(int[],int[],GeneralMatrix)... ", "Unexpected ArrayIndexOutOfBoundsException");
				System.Console.Out.WriteLine(e1.Message);
			}
			
			/// <summary>Array-like methods:
			/// Subtract
			/// SubtractEquals
			/// Add
			/// AddEquals
			/// ArrayLeftDivide
			/// ArrayLeftDivideEquals
			/// ArrayRightDivide
			/// ArrayRightDivideEquals
			/// arrayTimes
			/// ArrayMultiplyEquals
			/// uminus
			/// 
			/// </summary>
			
			print("\nTesting array-like methods...\n");
			S = new GeneralMatrix(columnwise, nonconformld);
			R = GeneralMatrix.Random(A.RowDimension, A.ColumnDimension);
			A = R;
			try
			{
				S = A.Subtract(S);
				errorCount = try_failure(errorCount, "Subtract conformance check... ", "nonconformance not raised");
			}
			catch (System.ArgumentException e)
			{
				try_success("Subtract conformance check... ", "");
				System.Console.Out.WriteLine(e.Message);
			}
			if (A.Subtract(R).Norm1() != 0.0)
			{
				errorCount = try_failure(errorCount, "Subtract... ", "(difference of identical Matrices is nonzero,\nSubsequent use of Subtract should be suspect)");
			}
			else
			{
				try_success("Subtract... ", "");
			}
			A = R.Copy();
			A.SubtractEquals(R);
			Z = new GeneralMatrix(A.RowDimension, A.ColumnDimension);
			try
			{
				A.SubtractEquals(S);
				errorCount = try_failure(errorCount, "SubtractEquals conformance check... ", "nonconformance not raised");
			}
			catch (System.ArgumentException e)
			{
				try_success("SubtractEquals conformance check... ", "");
				System.Console.Out.WriteLine(e.Message);
			}
			if (A.Subtract(Z).Norm1() != 0.0)
			{
				errorCount = try_failure(errorCount, "SubtractEquals... ", "(difference of identical Matrices is nonzero,\nSubsequent use of Subtract should be suspect)");
			}
			else
			{
				try_success("SubtractEquals... ", "");
			}
			
			A = R.Copy();
			B = GeneralMatrix.Random(A.RowDimension, A.ColumnDimension);
			C = A.Subtract(B);
			try
			{
				S = A.Add(S);
				errorCount = try_failure(errorCount, "Add conformance check... ", "nonconformance not raised");
			}
			catch (System.ArgumentException e)
			{
				try_success("Add conformance check... ", "");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				check(C.Add(B), A);
				try_success("Add... ", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "Add... ", "(C = A - B, but C + B != A)");
				System.Console.Out.WriteLine(e.Message);
			}
			C = A.Subtract(B);
			C.AddEquals(B);
			try
			{
				A.AddEquals(S);
				errorCount = try_failure(errorCount, "AddEquals conformance check... ", "nonconformance not raised");
			}
			catch (System.ArgumentException e)
			{
				try_success("AddEquals conformance check... ", "");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				check(C, A);
				try_success("AddEquals... ", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "AddEquals... ", "(C = A - B, but C = C + B != A)");
				System.Console.Out.WriteLine(e.Message);
			}
			A = R.UnaryMinus();
			try
			{
				check(A.Add(R), Z);
				try_success("UnaryMinus... ", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "uminus... ", "(-A + A != zeros)");
				System.Console.Out.WriteLine(e.Message);
			}
			A = R.Copy();
			O = new GeneralMatrix(A.RowDimension, A.ColumnDimension, 1.0);
			C = A.ArrayLeftDivide(R);
			try
			{
				S = A.ArrayLeftDivide(S);
				errorCount = try_failure(errorCount, "ArrayLeftDivide conformance check... ", "nonconformance not raised");
			}
			catch (System.ArgumentException e)
			{
				try_success("ArrayLeftDivide conformance check... ", "");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				check(C, O);
				try_success("ArrayLeftDivide... ", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "ArrayLeftDivide... ", "(M.\\M != ones)");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				A.ArrayLeftDivideEquals(S);
				errorCount = try_failure(errorCount, "ArrayLeftDivideEquals conformance check... ", "nonconformance not raised");
			}
			catch (System.ArgumentException e)
			{
				try_success("ArrayLeftDivideEquals conformance check... ", "");
				System.Console.Out.WriteLine(e.Message);
			}
			A.ArrayLeftDivideEquals(R);
			try
			{
				check(A, O);
				try_success("ArrayLeftDivideEquals... ", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "ArrayLeftDivideEquals... ", "(M.\\M != ones)");
				System.Console.Out.WriteLine(e.Message);
			}
			A = R.Copy();
			try
			{
				A.ArrayRightDivide(S);
				errorCount = try_failure(errorCount, "ArrayRightDivide conformance check... ", "nonconformance not raised");
			}
			catch (System.ArgumentException e)
			{
				try_success("ArrayRightDivide conformance check... ", "");
				System.Console.Out.WriteLine(e.Message);
			}
			C = A.ArrayRightDivide(R);
			try
			{
				check(C, O);
				try_success("ArrayRightDivide... ", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "ArrayRightDivide... ", "(M./M != ones)");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				A.ArrayRightDivideEquals(S);
				errorCount = try_failure(errorCount, "ArrayRightDivideEquals conformance check... ", "nonconformance not raised");
			}
			catch (System.ArgumentException e)
			{
				try_success("ArrayRightDivideEquals conformance check... ", "");
				System.Console.Out.WriteLine(e.Message);
			}
			A.ArrayRightDivideEquals(R);
			try
			{
				check(A, O);
				try_success("ArrayRightDivideEquals... ", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "ArrayRightDivideEquals... ", "(M./M != ones)");
				System.Console.Out.WriteLine(e.Message);
			}
			A = R.Copy();
			B = GeneralMatrix.Random(A.RowDimension, A.ColumnDimension);
			try
			{
				S = A.ArrayMultiply(S);
				errorCount = try_failure(errorCount, "arrayTimes conformance check... ", "nonconformance not raised");
			}
			catch (System.ArgumentException e)
			{
				try_success("arrayTimes conformance check... ", "");
				System.Console.Out.WriteLine(e.Message);
			}
			C = A.ArrayMultiply(B);
			try
			{
				check(C.ArrayRightDivideEquals(B), A);
				try_success("arrayTimes... ", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "arrayTimes... ", "(A = R, C = A.*B, but C./B != A)");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				A.ArrayMultiplyEquals(S);
				errorCount = try_failure(errorCount, "ArrayMultiplyEquals conformance check... ", "nonconformance not raised");
			}
			catch (System.ArgumentException e)
			{
				try_success("ArrayMultiplyEquals conformance check... ", "");
				System.Console.Out.WriteLine(e.Message);
			}
			A.ArrayMultiplyEquals(B);
			try
			{
				check(A.ArrayRightDivideEquals(B), R);
				try_success("ArrayMultiplyEquals... ", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "ArrayMultiplyEquals... ", "(A = R, A = A.*B, but A./B != R)");
				System.Console.Out.WriteLine(e.Message);
			}
			
			/// <summary>LA methods:
			/// Transpose
			/// Multiply
			/// Condition
			/// Rank
			/// Determinant
			/// trace
			/// Norm1
			/// norm2
			/// normF
			/// normInf
			/// Solve
			/// solveTranspose
			/// Inverse
			/// chol
			/// Eigen
			/// lu
			/// qr
			/// svd 
			/// 
			/// </summary>
			
			print("\nTesting linear algebra methods...\n");
			A = new GeneralMatrix(columnwise, 3);
			T = new GeneralMatrix(tvals);
			T = A.Transpose();
			try
			{
				check(A.Transpose(), T);
				try_success("Transpose...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "Transpose()...", "Transpose unsuccessful");
				System.Console.Out.WriteLine(e.Message);
			}
			A.Transpose();
			try
			{
				check(A.Norm1(), columnsummax);
				try_success("Norm1...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "Norm1()...", "incorrect norm calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				check(A.NormInf(), rowsummax);
				try_success("normInf()...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "normInf()...", "incorrect norm calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				check(A.NormF(), System.Math.Sqrt(sumofsquares));
				try_success("normF...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "normF()...", "incorrect norm calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				check(A.Trace(), sumofdiagonals);
				try_success("trace()...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "trace()...", "incorrect trace calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				check(A.GetMatrix(0, A.RowDimension - 1, 0, A.RowDimension - 1).Determinant(), 0.0);
				try_success("Determinant()...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "Determinant()...", "incorrect determinant calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			SQ = new GeneralMatrix(square);
			try
			{
				check(A.Multiply(A.Transpose()), SQ);
				try_success("Multiply(GeneralMatrix)...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "Multiply(GeneralMatrix)...", "incorrect GeneralMatrix-GeneralMatrix product calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			try
			{
				check(A.Multiply(0.0), Z);
				try_success("Multiply(double)...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "Multiply(double)...", "incorrect GeneralMatrix-scalar product calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			
			A = new GeneralMatrix(columnwise, 4);
			QRDecomposition QR = A.QRD();
			R = QR.R;
			try
			{
				check(A, QR.Q.Multiply(R));
				try_success("QRDecomposition...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "QRDecomposition...", "incorrect QR decomposition calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			SingularValueDecomposition SVD = A.SVD();
			try
			{
				check(A, SVD.GetU().Multiply(SVD.S.Multiply(SVD.GetV().Transpose())));
				try_success("SingularValueDecomposition...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "SingularValueDecomposition...", "incorrect singular value decomposition calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			DEF = new GeneralMatrix(rankdef);
			try
			{
				check(DEF.Rank(), System.Math.Min(DEF.RowDimension, DEF.ColumnDimension) - 1);
				try_success("Rank()...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "Rank()...", "incorrect Rank calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			B = new GeneralMatrix(condmat);
			SVD = B.SVD();
			double[] singularvalues = SVD.SingularValues;
			try
			{
				check(B.Condition(), singularvalues[0] / singularvalues[System.Math.Min(B.RowDimension, B.ColumnDimension) - 1]);
				try_success("Condition()...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "Condition()...", "incorrect condition number calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			int n = A.ColumnDimension;
			A = A.GetMatrix(0, n - 1, 0, n - 1);
			A.SetElement(0, 0, 0.0);
			LUDecomposition LU = A.LUD();
			try
			{
				check(A.GetMatrix(LU.Pivot, 0, n - 1), LU.L.Multiply(LU.U));
				try_success("LUDecomposition...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "LUDecomposition...", "incorrect LU decomposition calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			X = A.Inverse();
			try
			{
				check(A.Multiply(X), GeneralMatrix.Identity(3, 3));
				try_success("Inverse()...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "Inverse()...", "incorrect Inverse calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			O = new GeneralMatrix(SUB.RowDimension, 1, 1.0);
			SOL = new GeneralMatrix(sqSolution);
			SQ = SUB.GetMatrix(0, SUB.RowDimension - 1, 0, SUB.RowDimension - 1);
			try
			{
				check(SQ.Solve(SOL), O);
				try_success("Solve()...", "");
			}
			catch (System.ArgumentException e1)
			{
				errorCount = try_failure(errorCount, "Solve()...", e1.Message);
				System.Console.Out.WriteLine(e1.Message);
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "Solve()...", e.Message);
				System.Console.Out.WriteLine(e.Message);
			}
			A = new GeneralMatrix(pvals);
			CholeskyDecomposition Chol = A.chol();
			GeneralMatrix L = Chol.GetL();
			try
			{
				check(A, L.Multiply(L.Transpose()));
				try_success("CholeskyDecomposition...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "CholeskyDecomposition...", "incorrect Cholesky decomposition calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			X = Chol.Solve(GeneralMatrix.Identity(3, 3));
			try
			{
				check(A.Multiply(X), GeneralMatrix.Identity(3, 3));
				try_success("CholeskyDecomposition Solve()...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "CholeskyDecomposition Solve()...", "incorrect Choleskydecomposition Solve calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			EigenvalueDecomposition Eig = A.Eigen();
			GeneralMatrix D = Eig.D;
			GeneralMatrix V = Eig.GetV();
			try
			{
				check(A.Multiply(V), V.Multiply(D));
				try_success("EigenvalueDecomposition (symmetric)...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "EigenvalueDecomposition (symmetric)...", "incorrect symmetric Eigenvalue decomposition calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			A = new GeneralMatrix(evals);
			Eig = A.Eigen();
			D = Eig.D;
			V = Eig.GetV();
			try
			{
				check(A.Multiply(V), V.Multiply(D));
				try_success("EigenvalueDecomposition (nonsymmetric)...", "");
			}
			catch (System.SystemException e)
			{
				errorCount = try_failure(errorCount, "EigenvalueDecomposition (nonsymmetric)...", "incorrect nonsymmetric Eigenvalue decomposition calculation");
				System.Console.Out.WriteLine(e.Message);
			}
			
			print("\nTestMatrix completed.\n");
			print("Total errors reported: " + System.Convert.ToString(errorCount) + "\n");
			print("Total warnings reported: " + System.Convert.ToString(warningCount) + "\n");
		}
		
		/// <summary>private utility routines *</summary>
		
		/// <summary>Check magnitude of difference of scalars. *</summary>
		
		private static void  check(double x, double y)
		{
			double eps = System.Math.Pow(2.0, - 52.0);
			if (x == 0 & System.Math.Abs(y) < 10 * eps)
				return ;
			if (y == 0 & System.Math.Abs(x) < 10 * eps)
				return ;
			if (System.Math.Abs(x - y) > 10 * eps * System.Math.Max(System.Math.Abs(x), System.Math.Abs(y)))
			{
				throw new System.SystemException("The difference x-y is too large: x = " + x.ToString() + "  y = " + y.ToString());
			}
		}
		
		/// <summary>Check norm of difference of "vectors". *</summary>
		
		private static void  check(double[] x, double[] y)
		{
			if (x.Length == y.Length)
			{
				for (int i = 0; i < x.Length; i++)
				{
					check(x[i], y[i]);
				}
			}
			else
			{
				throw new System.SystemException("Attempt to compare vectors of different lengths");
			}
		}
		
		/// <summary>Check norm of difference of arrays. *</summary>
		
		private static void  check(double[][] x, double[][] y)
		{
			GeneralMatrix A = new GeneralMatrix(x);
			GeneralMatrix B = new GeneralMatrix(y);
			check(A, B);
		}
		
		/// <summary>Check norm of difference of Matrices. *</summary>
		
		private static void  check(GeneralMatrix X, GeneralMatrix Y)
		{
			double eps = System.Math.Pow(2.0, - 52.0);
			if (X.Norm1() == 0.0 & Y.Norm1() < 10 * eps)
				return ;
			if (Y.Norm1() == 0.0 & X.Norm1() < 10 * eps)
				return ;
			if (X.Subtract(Y).Norm1() > 1000 * eps * System.Math.Max(X.Norm1(), Y.Norm1()))
			{
				throw new System.SystemException("The norm of (X-Y) is too large: " + X.Subtract(Y).Norm1().ToString());
			}
		}
		
		/// <summary>Shorten spelling of print. *</summary>
		
		private static void  print(System.String s)
		{
			System.Console.Out.Write(s);
		}
		
		/// <summary>Print appropriate messages for successful outcome try *</summary>
		
		private static void  try_success(System.String s, System.String e)
		{
			print(">    " + s + "success\n");
			if ((System.Object) e != (System.Object) "")
			{
				print(">      Message: " + e + "\n");
			}
		}
		/// <summary>Print appropriate messages for unsuccessful outcome try *</summary>
		
		private static int try_failure(int count, System.String s, System.String e)
		{
			print(">    " + s + "*** failure ***\n>      Message: " + e + "\n");
			return ++count;
		}
		
		/// <summary>Print appropriate messages for unsuccessful outcome try *</summary>
		
		private static int try_warning(int count, System.String s, System.String e)
		{
			print(">    " + s + "*** warning ***\n>      Message: " + e + "\n");
			return ++count;
		}
	}
}