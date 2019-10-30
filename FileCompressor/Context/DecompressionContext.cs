﻿using FileCompressor.Models;
using System;
using System.IO;

namespace FileCompressor.Context
{
    public class DecompressionContext : BaseContext<ChunkForDecompressModel, FileChunk>
    {
        public DecompressionContext(string inFilePath, string toFilePath) : base(inFilePath, toFilePath)
        {
        }

        public override FileChunk ConvertReadToWriteModel(ChunkForDecompressModel readChunk)
        {
            var position = BitConverter.ToInt64(readChunk.PositionBuffer, 0);
            var decompressed = CompressHelper.Decompress(readChunk.DataBuffer);
            return new FileChunk(position, decompressed);
        }

        public override void WriteChunk(FileChunk chunk)
        {
            ToStream.Seek(chunk.Position, SeekOrigin.Begin);
            ToStream.Write(chunk.DataBuffer, 0, chunk.Length);
        }

        protected override ChunkForDecompressModel ReadChunk()
        {
            var positionBuffer = new byte[8];
            var lengthBuffer = new byte[4];

            InStream.Read(positionBuffer, 0, 8);
            InStream.Read(lengthBuffer, 0, 4);

            var length = BitConverter.ToInt32(lengthBuffer, 0);

            var buffer = new byte[length];
            InStream.Read(buffer, 0, length);

            return new ChunkForDecompressModel(positionBuffer, buffer);
        }

        protected override int InitialPartitionsCount()
        {
            var buffer = new byte[4];
            InStream.Read(buffer, 0, buffer.Length);
            var partitionsCount = BitConverter.ToInt32(buffer, 0);
            return partitionsCount;
        }
    }
}
