using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Lab3_Matrix_Dair;

namespace Lab_3_Matrix
{
    public static class MatrixOperations
    {
        public static Matrix Transpose(Matrix matrix)
        {
            int rows = matrix.RowCount;
            int cols = matrix.ColCount;
            double[,] transposed = new double[cols, rows];

            Parallel.For(0, rows, i =>
            {
                for (int j = 0; j < cols; j++)
                {
                    transposed[j, i] = matrix[i, j];
                }
            });

            return new Matrix(transposed);
        }

        public static Matrix Multiply(Matrix matrix, double scalar)
        {
            int rows = matrix.RowCount;
            int cols = matrix.ColCount;
            double[,] result = new double[rows, cols];

            Parallel.For(0, rows, i =>
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = matrix[i, j] * scalar;
                }
            });

            return new Matrix(result);
        }

        public static Matrix Add(Matrix a, Matrix b)
        {
            if (a.RowCount != b.RowCount || a.ColCount != b.ColCount)
            {
                throw new ArgumentException("Matrix dimensions must match for addition.");
            }

            int rows = a.RowCount;
            int cols = a.ColCount;
            double[,] result = new double[rows, cols];

            Parallel.For(0, rows, i =>
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = a[i, j] + b[i, j];
                }
            });

            return new Matrix(result);
        }

        public static Matrix Subtract(Matrix a, Matrix b)
        {
            if (a.RowCount != b.RowCount || a.ColCount != b.ColCount)
            {
                throw new ArgumentException("Matrix dimensions must match for subtraction.");
            }

            int rows = a.RowCount;
            int cols = a.ColCount;
            double[,] result = new double[rows, cols];

            Parallel.For(0, rows, i =>
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = a[i, j] - b[i, j];
                }
            });

            return new Matrix(result);
        }

        public static Matrix Multiply(Matrix a, Matrix b)
        {
            if (a.ColCount != b.RowCount)
            {
                throw new ArgumentException("Matrix dimensions must match for multiplication.");
            }

            int rows = a.RowCount;
            int cols = b.ColCount;
            int n = a.ColCount;
            double[,] result = new double[rows, cols];

            Parallel.For(0, rows, i =>
            {
                for (int j = 0; j < cols; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < n; k++)
                    {
                        sum += a[i, k] * b[k, j];
                    }
                    result[i, j] = sum;
                }
            });

            return new Matrix(result);
        }

        public static Matrix Negate(Matrix matrix)
        {
            return Multiply(matrix, -1);
        }

        public static (Matrix, double) Inverse(Matrix matrix)
        {
            int n = matrix.RowCount;
            if (n != matrix.ColCount)
            {
                throw new MatrixInverseException("Matrix must be square to find its inverse.");
            }

            double[,] result = new double[n, n];
            double[,] augmented = new double[n, 2 * n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    augmented[i, j] = matrix[i, j];
                    augmented[i, j + n] = (i == j) ? 1.0 : 0.0;
                }
            }

            for (int i = 0; i < n; i++)
            {
                if (augmented[i, i] == 0)
                {
                    bool swapped = false;
                    for (int k = i + 1; k < n; k++)
                    {
                        if (augmented[k, i] != 0)
                        {
                            SwapRows(augmented, i, k);
                            swapped = true;
                            break;
                        }
                    }
                    if (!swapped)
                    {
                        throw new MatrixInverseException("Matrix is singular and cannot be inverted.");
                    }
                }

                double pivot = augmented[i, i];
                for (int j = 0; j < 2 * n; j++)
                {
                    augmented[i, j] /= pivot;
                }

                for (int k = 0; k < n; k++)
                {
                    if (k != i)
                    {
                        double factor = augmented[k, i];
                        for (int j = 0; j < 2 * n; j++)
                        {
                            augmented[k, j] -= factor * augmented[i, j];
                        }
                    }
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i, j] = augmented[i, j + n];
                }
            }

            double determinant = CalculateDeterminant(matrix);
            return (new Matrix(result), determinant);
        }

        private static void SwapRows(double[,] matrix, int row1, int row2)
        {
            int cols = matrix.GetLength(1);
            for (int i = 0; i < cols; i++)
            {
                double temp = matrix[row1, i];
                matrix[row1, i] = matrix[row2, i];
                matrix[row2, i] = temp;
            }
        }

        private static double CalculateDeterminant(Matrix matrix)
        {
            int n = matrix.RowCount;
            double[,] tempMatrix = (double[,])matrix.Clone();
            double det = 1;

            for (int i = 0; i < n; i++)
            {
                if (tempMatrix[i, i] == 0)
                {
                    bool swapped = false;
                    for (int k = i + 1; k < n; k++)
                    {
                        if (tempMatrix[k, i] != 0)
                        {
                            SwapRows(tempMatrix, i, k);
                            det *= -1;
                            swapped = true;
                            break;
                        }
                    }
                    if (!swapped)
                    {
                        return 0;
                    }
                }

                det *= tempMatrix[i, i];
                for (int k = i + 1; k < n; k++)
                {
                    double factor = tempMatrix[k, i] / tempMatrix[i, i];
                    for (int j = i; j < n; j++)
                    {
                        tempMatrix[k, j] -= factor * tempMatrix[i, j];
                    }
                }
            }
            return det;
        }
    }
}
