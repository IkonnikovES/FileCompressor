using System.IO;
using System.IO.Compression;

namespace FileCompressor
{
    public static class CompressHelper
    {
        public const string CompressMode = "COMPRESS";
        public const string DecompressMode = "DECOMPRESS";

        public static byte[] Compress(byte[] buffer)
        {
            using (var compressedStream = new MemoryStream())
            using (var gZipStream = new GZipStream(compressedStream, CompressionLevel.Optimal))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
                gZipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] buffer)
        {
            using (var compressedStream = new MemoryStream(buffer))
            using (var gZipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var decompressedStream = new MemoryStream())
            {
                gZipStream.CopyTo(decompressedStream);
                return decompressedStream.ToArray();
            }
        }
    }
}
