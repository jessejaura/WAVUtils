namespace WAVUtils.Models
{
    public class Fmt
    {
        public string subchunk1ID { get; set; }
        
        public int subchunk1Size { get; set; }
        
        public int audioFormat { get; set; }
        
        public int numChannels { get; set; }
        
        public int sampleRate { get; set; }
        
        public int byteRate { get; set; }
        
        public int blockAlign { get; set; }
        
        public int bitsPerSample { get; set; }
    }
}