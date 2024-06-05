using Lab3_Matrix_Dair;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Lab_3_Matrix
{
    public static class MatrixIO
    {
        public static async Task WriteMatrixAsync(Matrix matrix, Stream stream, char sep = ' ')
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync($"{matrix.RowCount}{sep}{matrix.ColCount}");
                for (int i = 0; i < matrix.RowCount; i++)
                {
                    for (int j = 0; j < matrix.ColCount; j++)
                    {
                        await writer.WriteAsync(matrix[i, j].ToString("F2"));
                        if (j < matrix.ColCount - 1)
                        {
                            await writer.WriteAsync(sep);
                        }
                    }
                    await writer.WriteLineAsync();
                }
            }
        }

        public static async Task<Matrix> ReadMatrixAsync(Stream stream, char sep = ' ')
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                string[] dimensions = (await reader.ReadLineAsync()).Split(sep);
                int rows = int.Parse(dimensions[0]);
                int cols = int.Parse(dimensions[1]);
                double[,] values = new double[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    string[] elements = (await reader.ReadLineAsync()).Split(sep);
                    for (int j = 0; j < cols; j++)
                    {
                        values[i, j] = double.Parse(elements[j]);
                    }
                }
                return new Matrix(values);
            }
        }

        public static void WriteMatrixBinary(Matrix matrix, Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(matrix.RowCount);
                writer.Write(matrix.ColCount);
                for (int i = 0; i < matrix.RowCount; i++)
                {
                    for (int j = 0; j < matrix.ColCount; j++)
                    {
                        writer.Write(matrix[i, j]);
                    }
                }
            }
        }

        public static Matrix ReadMatrixBinary(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                int rows = reader.ReadInt32();
                int cols = reader.ReadInt32();
                double[,] values = new double[rows, cols];
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        values[i, j] = reader.ReadDouble();
                    }
                }
                return new Matrix(values);
            }
        }

        public static async Task WriteMatrixJsonAsync(Matrix matrix, Stream stream)
        {
            double[][] tempArray = new double[matrix.RowCount][];
            for (int i = 0; i < matrix.RowCount; i++)
            {
                tempArray[i] = new double[matrix.ColCount];
                for (int j = 0; j < matrix.ColCount; j++)
                {
                    tempArray[i][j] = matrix[i, j];
                }
            }

            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                string json = JsonConvert.SerializeObject(tempArray);
                await writer.WriteAsync(json);
            }
        }

        public static async Task<Matrix> ReadMatrixJsonAsync(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                string json = await reader.ReadToEndAsync();
                double[][] tempArray = JsonConvert.DeserializeObject<double[][]>(json);

                int rows = tempArray.Length;
                int cols = tempArray[0].Length;
                double[,] values = new double[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        values[i, j] = tempArray[i][j];
                    }
                }
                return new Matrix(values);
            }
        }

        public static void WriteMatrixToFile(string directory, string fileName, Matrix matrix, Action<Matrix, Stream> writeMethod)
        {
            string filePath = Path.Combine(directory, fileName);
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                writeMethod(matrix, fs);
            }
        }

        public static async Task WriteMatrixToFileAsync(string directory, string fileName, Matrix matrix, Func<Matrix, Stream, Task> writeMethod)
        {
            string filePath = Path.Combine(directory, fileName);
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await writeMethod(matrix, fs);
            }
        }

        public static Matrix ReadMatrixFromFile(string filePath, Func<Stream, Matrix> readMethod)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return readMethod(fs);
            }
        }

        public static async Task<Matrix> ReadMatrixFromFileAsync(string filePath, Func<Stream, Task<Matrix>> readMethod)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return await readMethod(fs);
            }
        }
    }
}
