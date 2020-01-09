using FileCompressor.Models;
using System;
using System.Threading;

namespace FileCompressor
{
    class Program
    {
        static void Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            try
            {
                var parameters = ParseParameters(args);
                var compressMode = parameters.CompressionMode.ToUpper();
                var compressor = new Compressor(Environment.ProcessorCount, cancellationTokenSource.Cancel);

                if (compressMode == CompressHelper.CompressMode)
                {
                    compressor.Compress(parameters.FromFilePath, parameters.ToFilePath, cancellationToken);
                }
                else if (compressMode == CompressHelper.DecompressMode)
                {
                    compressor.Decompress(parameters.FromFilePath, parameters.ToFilePath, cancellationToken);
                }
                else
                {
                    throw new ArgumentException("Команда не распознана введите compress/decompress");
                }

                Console.WriteLine("Процесс завершен с кодом 0");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Процесс завершен с кодом 1");
                Console.WriteLine($"Текст ошибки: {ex.Message}");
            }
        }

        static CompressParametersModel ParseParameters(string[] args)
        {
            if (args.Length == 3)
            {
                var model = new CompressParametersModel
                {
                    CompressionMode = args[0],
                    FromFilePath = args[1],
                    ToFilePath = args[2]
                };

                return model;
            }

            var message = "Требуется 3 параметра [compress/decompress] [имя исходного файла] [имя результирующего файла]";
            throw new ArgumentException(message);
        }
    }
}
