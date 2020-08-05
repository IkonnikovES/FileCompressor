using FileCompressor.Models;
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
            lock (ToStream)
            {
                ToStream.Seek(chunk.Position, SeekOrigin.Begin);
                ToStream.Write(chunk.DataBuffer, 0, chunk.Length);
            }
        }

        public override ChunkForDecompressModel ReadChunk()
        {
            lock (InStream)
            {
                var positionBuffer = new byte[Int64Size];
                var lengthBuffer = new byte[Int32Size];

                InStream.Read(positionBuffer, 0, positionBuffer.Length);
                InStream.Read(lengthBuffer, 0, lengthBuffer.Length);

                var length = BitConverter.ToInt32(lengthBuffer, 0);

                var buffer = new byte[length];
                InStream.Read(buffer, 0, buffer.Length);

                return new ChunkForDecompressModel(positionBuffer, buffer);
            }
        }

        protected override int InitialPartitionsCount()
        {
            var buffer = new byte[Int32Size];
            InStream.Read(buffer, 0, buffer.Length);
            var partitionsCount = BitConverter.ToInt32(buffer, 0);
            return partitionsCount;
        }
    }
}
