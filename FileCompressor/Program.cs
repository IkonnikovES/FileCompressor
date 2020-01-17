﻿using FileCompressor.Models;
using System;
using System.Diagnostics;
using System.Threading;

namespace FileCompressor
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                //var parameters = ParseParameters(args);
                //var compressMode = parameters.CompressionMode.ToUpper();

                //if (compressMode == CompressHelper.CompressMode)
                //{
                //Compressor.Compress(@"E:\test.log", @"E:\test.gz");
                //}
                //else if (compressMode == CompressHelper.DecompressMode)
                //{
                Compressor.Decompress(@"E:\test.gz", @"E:\test2.txt");
                //}
                //else
                //{
                //    throw new ArgumentException("Команда не распознана введите compress/decompress");
                //}

                Console.WriteLine("Процесс завершен с кодом 0");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Процесс завершен с кодом 1");
                Console.WriteLine($"Текст ошибки: {ex.Message}");
            }
            stopWatch.Stop();
            Console.WriteLine(stopWatch.Elapsed);
            Console.ReadLine();
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
