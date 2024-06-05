using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Lab_3_Matrix;


namespace Lab3_Matrix_Dair
{
    public class Matrix
    {
        private readonly double[,] values;

        public Matrix(double[,] initialValues)
        {
            values = initialValues;
        }

        public static Matrix Zero(int rows, int cols)
        {
            return new Matrix(new double[rows, cols]);
        }

        public static Matrix Zero(int n)
        {
            return new Matrix(new double[n, n]);
        }

        public static Matrix Identity(int n)
        {
            var identity = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                identity[i, i] = 1.0;
            }
            return new Matrix(identity);
        }

        public double this[int i, int j]
        {
            get { return values[i, j]; }
        }

        public int RowCount
        {
            get { return values.GetLength(0); }
        }

        public int ColCount
        {
            get { return values.GetLength(1); }
        }

        public Matrix Transpose()
        {
            return MatrixOperations.Transpose(this);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColCount; j++)
                {
                    sb.Append(values[i, j].ToString("F2")).Append("\t");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix other && RowCount == other.RowCount && ColCount == other.ColCount)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < ColCount; j++)
                    {
                        if (this[i, j] != other[i, j]) return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColCount; j++)
                {
                    hash = hash * 23 + values[i, j].GetHashCode();
                }
            }
            return hash;
        }

        public static Matrix operator +(Matrix a, Matrix b)
        {
            return MatrixOperations.Add(a, b);
        }

        public static Matrix operator -(Matrix a, Matrix b)
        {
            return MatrixOperations.Subtract(a, b);
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            return MatrixOperations.Multiply(a, b);
        }

        public static Matrix operator *(Matrix a, double scalar)
        {
            return MatrixOperations.Multiply(a, scalar);
        }

        public static Matrix operator *(double scalar, Matrix a)
        {
            return MatrixOperations.Multiply(a, scalar);
        }

        public static Matrix operator ~(Matrix a)
        {
            return a.Transpose();
        }

        public static Matrix operator +(Matrix a)
        {
            return a;
        }

        public static Matrix operator -(Matrix a)
        {
            return MatrixOperations.Negate(a);
        }
    }
}
