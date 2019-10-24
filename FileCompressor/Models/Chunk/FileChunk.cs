namespace FileCompressor.Models
{
    public class FileChunk : BaseChunk
    {
        public FileChunk(long position, byte[] buffer)
        {
            Position = position;
            DataBuffer = buffer;
        }

        public long Position { get; set; }

        public int Length => DataBuffer.Length;
    }
}
