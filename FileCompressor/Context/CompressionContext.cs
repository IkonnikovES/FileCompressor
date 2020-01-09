using FileCompressor.Models;
using System;
using System.Threading;

namespace FileCompressor.Context
{
    public class CompressionContext : BaseContext<FileChunk, ByteChunkModel>
    {
        public CompressionContext(string inFilePath, string toFilePath, CancellationToken cancellationToken) : base(inFilePath, toFilePath, cancellationToken)
        {

        }

        public override ByteChunkModel ConvertReadToWriteModel(FileChunk readChunk)
        {
            var position = BitConverter.GetBytes(readChunk.Position);
            var compressed = CompressHelper.Compress(readChunk.DataBuffer);
            var length = BitConverter.GetBytes(compressed.Length);

            return new ByteChunkModel(length, position, compressed);
        }

        protected override void Write(ByteChunkModel chunk)
        {
            var positionBuffer = chunk.PositionBuffer;
            var lengthBuffer = chunk.LengthBuffer;
            var dataBuffer = chunk.DataBuffer;

            ToStream.Write(positionBuffer, 0, positionBuffer.Length);
            ToStream.Write(lengthBuffer, 0, lengthBuffer.Length);
            ToStream.Write(dataBuffer, 0, dataBuffer.Length);
        }

        protected override FileChunk Read()
        {
            var buffer = new byte[Math.Min(BufferSize, LeftBytes)];
            var position = InStream.Position;
            InStream.Read(buffer, 0, buffer.Length);

            return new FileChunk(position, buffer);
        }

        protected override int InitialPartitionsCount()
        {
            var partiotionsCount = (int)Math.Ceiling(InStream.Length / (double)BufferSize);
            var buffer = BitConverter.GetBytes(partiotionsCount);
            ToStream.Write(buffer, 0, buffer.Length);
            return partiotionsCount;
        }
    }
}
