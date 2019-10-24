namespace FileCompressor.Models
{
    public class ChunkForDecompressModel : BaseChunk
    {
        public ChunkForDecompressModel(byte[] position, byte[] data)
        {
            PositionBuffer = position;
            DataBuffer = data;
        }

        public byte[] PositionBuffer { get; set; }
    }
}
