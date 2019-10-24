namespace FileCompressor.Models
{
    public class ByteChunkModel : BaseChunk
    {
        public ByteChunkModel(byte[] length, byte[] position, byte[] data)
        {
            LengthBuffer = length;
            PositionBuffer = position;
            DataBuffer = data;
        }

        public byte[] LengthBuffer { get; set; }

        public byte[] PositionBuffer { get; set; }
    }
}
