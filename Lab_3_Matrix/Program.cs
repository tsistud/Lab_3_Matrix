using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Lab3_Matrix_Dair;
using Lab_3_Matrix;

namespace Lab_3_Matrix_Dair
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //var matrix1 = CreateRandomMatrix(3, 3);
            //var matrix2 = CreateRandomMatrix(3, 3);

            //Console.WriteLine("Matrix 1:");
            //Console.WriteLine(matrix1);

            //Console.WriteLine("Matrix 2:");
            //Console.WriteLine(matrix2);

            //var product = MatrixOperations.Multiply(matrix1, matrix2);
            //Console.WriteLine("Product of Matrix 1 and Matrix 2:");
            //Console.WriteLine(product);

            const int matrixCount = 50;
            const int matrixSize1 = 500;
            const int matrixSize2 = 100;

            string resultsDirectory = "Results";
            if (Directory.Exists(resultsDirectory))
            {
                Directory.Delete(resultsDirectory, true);
            }
            Directory.CreateDirectory(resultsDirectory);

            Matrix[] a = new Matrix[matrixCount];
            Matrix[] b = new Matrix[matrixCount];

            for (int i = 0; i < matrixCount; i++)
            {
                a[i] = CreateRandomMatrix(matrixSize1, matrixSize2);
                b[i] = CreateRandomMatrix(matrixSize2, matrixSize1);
            }

            string binaryFormatDir = Path.Combine(resultsDirectory, "BinaryFormat");
            string textFormatDir = Path.Combine(resultsDirectory, "TextFormat");
            string jsonFormatDir = Path.Combine(resultsDirectory, "JsonFormat");

            Directory.CreateDirectory(binaryFormatDir);
            Directory.CreateDirectory(textFormatDir);
            Directory.CreateDirectory(jsonFormatDir);

            var tasks = new List<Task>
            {
                Task.Run(() => MultiplyAndSave(a, b, Path.Combine(resultsDirectory, "Multiply_a_b.tsv"))),
                Task.Run(() => MultiplyAndSave(b, a, Path.Combine(resultsDirectory, "Multiply_b_a.tsv"))),
                Task.Run(() => ScalarProductAndSave(a, b, Path.Combine(resultsDirectory, "Scalar_a_b.tsv"))),
                Task.Run(() => ScalarProductAndSave(b, a, Path.Combine(resultsDirectory, "Scalar_b_a.tsv"))),
                Task.Run(() => SaveMatricesAsync(a, textFormatDir, "a", "csv", MatrixIO.WriteMatrixToTextAsync)),
                Task.Run(() => SaveMatricesAsync(b, textFormatDir, "b", "csv", MatrixIO.WriteMatrixToTextAsync)),
                Task.Run(() => SaveMatricesAsync(a, jsonFormatDir, "a", "json", MatrixIO.WriteMatrixToJsonAsync)),
                Task.Run(() => SaveMatricesAsync(b, jsonFormatDir, "b", "json", MatrixIO.WriteMatrixToJsonAsync)),
            };

            await Task.WhenAll(tasks);
            Console.WriteLine("Completed saving text and json formats.");

            var readTextTask = ReadMatricesAsync(textFormatDir, "a", "csv", MatrixIO.ReadMatrixFromTextAsync);
            var readJsonTask = ReadMatricesAsync(jsonFormatDir, "a", "json", MatrixIO.ReadMatrixFromJsonAsync);

            var completedReadTask = await Task.WhenAny(readTextTask, readJsonTask);
            string readType = completedReadTask == readTextTask ? "text" : "json";
            Console.WriteLine($"Completed reading matrices from {readType} format.");

            var readMatrices = await completedReadTask;
            var comparisonTasks = new List<Task>
            {
                Task.Run(() => CompareAndPrintResults(a, readMatrices, "Comparison with original a matrices"))
            };

            await Task.WhenAll(comparisonTasks);

            SaveMatrices(a, binaryFormatDir, "a", "bin", MatrixIO.WriteMatrixToBinary);
            SaveMatrices(b, binaryFormatDir, "b", "bin", MatrixIO.WriteMatrixToBinary);
            Console.WriteLine("Completed saving binary format.");

            var readBinaryMatricesA = ReadMatrices(binaryFormatDir, "a", "bin", MatrixIO.ReadMatrixFromBinary);
            var readBinaryMatricesB = ReadMatrices(binaryFormatDir, "b", "bin", MatrixIO.ReadMatrixFromBinary);

            var binaryComparisonTasks = new List<Task>
            {
                Task.Run(() => CompareAndPrintResults(a, readBinaryMatricesA, "Binary comparison with original a matrices")),
                Task.Run(() => CompareAndPrintResults(b, readBinaryMatricesB, "Binary comparison with original b matrices"))
            };

            await Task.WhenAll(binaryComparisonTasks);
        }

        public static Matrix CreateRandomMatrix(int rows, int cols)
        {
            Random rand = new Random();
            double[,] values = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    values[i, j] = rand.NextDouble() * 20 - 10; 
                }
            }
            return new Matrix(values);
        }

        public static Matrix[] MultiplyMatricesSequentially(Matrix[] first, Matrix[] second)
        {
            if (first.Length != second.Length)
            {
                throw new ArgumentException("Both arrays must have the same length.");
            }

            Matrix[] result = new Matrix[first.Length];
            for (int i = 0; i < first.Length; i++)
            {
                result[i] = MatrixOperations.Multiply(first[i], second[i]);
            }

            return result;
        }

        public static Matrix ScalarProductMatrices(Matrix[] first, Matrix[] second)
        {
            if (first.Length != second.Length)
            {
                throw new ArgumentException("Both arrays must have the same length.");
            }

            Matrix result = Matrix.Zero(first[0].RowCount, first[0].ColCount);
            for (int i = 0; i < first.Length; i++)
            {
                result = MatrixOperations.Add(result, MatrixOperations.Multiply(first[i], second[i]));
            }

            return result;
        }

        public static void WriteMatricesToDirectory(Matrix[] matrices, string directory, string prefix, string extension, Action<Matrix, Stream> writeMethod)
        {
            Directory.CreateDirectory(directory);
            for (int i = 0; i < matrices.Length; i++)
            {
                string filePath = Path.Combine(directory, $"{prefix}{i}.{extension}");
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    writeMethod(matrices[i], fs);
                }

                if ((i + 1) % 10 == 0)
                {
                    Console.WriteLine($"Written {i + 1} matrices.");
                }
            }
        }

        public static async Task WriteMatricesToDirectoryAsync(Matrix[] matrices, string directory, string prefix, string extension, Func<Matrix, Stream, Task> writeMethod)
        {
            Directory.CreateDirectory(directory);
            for (int i = 0; i < matrices.Length; i++)
            {
                string filePath = Path.Combine(directory, $"{prefix}{i}.{extension}");
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await writeMethod(matrices[i], fs);
                }

                if ((i + 1) % 10 == 0)
                {
                    Console.WriteLine($"Written {i + 1} matrices.");
                }
            }
        }

        public static Matrix[] ReadMatricesFromDirectory(string directory, string prefix, string extension, Func<Stream, Matrix> readMethod)
        {
            var files = Directory.GetFiles(directory, $"{prefix}*.{extension}");
            var matrices = new Matrix[files.Length];
            foreach (var file in files)
            {
                int index = int.Parse(Path.GetFileNameWithoutExtension(file).Substring(prefix.Length));
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    matrices[index] = readMethod(fs);
                }
            }
            return matrices;
        }

        public static async Task<Matrix[]> ReadMatricesFromDirectoryAsync(string directory, string prefix, string extension, Func<Stream, Task<Matrix>> readMethod)
        {
            var files = Directory.GetFiles(directory, $"{prefix}*.{extension}");
            var matrices = new Matrix[files.Length];
            foreach (var file in files)
            {
                int index = int.Parse(Path.GetFileNameWithoutExtension(file).Substring(prefix.Length));
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    matrices[index] = await readMethod(fs);
                }
            }
            return matrices;
        }

        public static bool CompareMatrixArrays(Matrix[] first, Matrix[] second)
        {
            if (first.Length != second.Length)
            {
                return false;
            }

            for (int i = 0; i < first.Length; i++)
            {
                if (!first[i].Equals(second[i]))
                {
                    return false;
                }
            }
            return true;
        }

        static async Task MultiplyAndSave(Matrix[] first, Matrix[] second, string filePath)
        {
            if (first.Length != second.Length)
            {
                throw new ArgumentException("Both arrays must have the same length.");
            }

            var results = new List<string>();
            for (int i = 0; i < first.Length; i++)
            {
                var result = MatrixOperations.Multiply(first[i], second[i]);
                results.Add(result.ToString());
            }

            await File.WriteAllLinesAsync(filePath, results);
        }

        static async Task ScalarProductAndSave(Matrix[] first, Matrix[] second, string filePath)
        {
            if (first.Length != second.Length)
            {
                throw new ArgumentException("Both arrays must have the same length.");
            }

            Matrix result = Matrix.Zero(first[0].RowCount, first[0].ColCount);
            for (int i = 0; i < first.Length; i++)
            {
                result = MatrixOperations.Add(result, MatrixOperations.Multiply(first[i], second[i]));
            }

            await File.WriteAllTextAsync(filePath, result.ToString());
        }

        static void SaveMatrices(Matrix[] matrices, string directory, string prefix, string extension, Action<Matrix, Stream> writeMethod)
        {
            Directory.CreateDirectory(directory);
            for (int i = 0; i < matrices.Length; i++)
            {
                string filePath = Path.Combine(directory, $"{prefix}{i}.{extension}");
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    writeMethod(matrices[i], fs);
                }
            }
        }

        static async Task SaveMatricesAsync(Matrix[] matrices, string directory, string prefix, string extension, Func<Matrix, Stream, Task> writeMethod)
        {
            Directory.CreateDirectory(directory);
            for (int i = 0; i < matrices.Length; i++)
            {
                string filePath = Path.Combine(directory, $"{prefix}{i}.{extension}");
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await writeMethod(matrices[i], fs);
                }
            }
        }

        static Matrix[] ReadMatrices(string directory, string prefix, string extension, Func<Stream, Matrix> readMethod)
        {
            var files = Directory.GetFiles(directory, $"{prefix}*.{extension}");
            var matrices = new Matrix[files.Length];
            foreach (var file in files)
            {
                int index = int.Parse(Path.GetFileNameWithoutExtension(file).Substring(prefix.Length));
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    matrices[index] = readMethod(fs);
                }
            }
            return matrices;
        }

        static async Task<Matrix[]> ReadMatricesAsync(string directory, string prefix, string extension, Func<Stream, Task<Matrix>> readMethod)
        {
            var files = Directory.GetFiles(directory, $"{prefix}*.{extension}");
            var matrices = new Matrix[files.Length];
            foreach (var file in files)
            {
                int index = int.Parse(Path.GetFileNameWithoutExtension(file).Substring(prefix.Length));
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    matrices[index] = await readMethod(fs);
                }
            }
            return matrices;
        }

        static void CompareAndPrintResults(Matrix[] original, Matrix[] read, string description)
        {
            bool areEqual = Program.CompareMatrixArrays(original, read);
            Console.WriteLine($"{description}: {areEqual}");
        }
    }
}
