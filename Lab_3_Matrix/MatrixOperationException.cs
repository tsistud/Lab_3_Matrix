using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_3_Matrix
{
    public class MatrixOperationException : Exception
    {
        public MatrixOperationException() { }

        public MatrixOperationException(string message) : base(message) { }

        public MatrixOperationException(string message, Exception inner) : base(message, inner) { }

        public static MatrixOperationException DimensionMismatch(string operation, int rowsA, int colsA, int rowsB, int colsB)
        {
            string message = $"Matrix dimension mismatch for {operation}: " +
                             $"Matrix A ({rowsA}x{colsA}) vs Matrix B ({rowsB}x{colsB}).";
            return new MatrixOperationException(message);
        }

        public static MatrixOperationException MultiplicationDimensionMismatch(int colsA, int rowsB)
        {
            string message = $"Matrix multiplication dimension mismatch: " +
                             $"Number of columns in Matrix A ({colsA}) must match number of rows in Matrix B ({rowsB}).";
            return new MatrixOperationException(message);
        }

        public static MatrixOperationException NonInvertibleMatrix()
        {
            string message = "Matrix is non-invertible (determinant is zero).";
            return new MatrixOperationException(message);
        }
    }

    public class MatrixInverseException : Exception
    {
        public MatrixInverseException(string message) : base(message)
        {
        }
    }

}
