namespace WAVUtils.Models
{
    public class WAVE : RIFF 
    { 
        public Fmt fmt { get; set; }
        
        public string subchunk2ID { get; set; }
        
        public int subchunk2Size { get; set; }
        
        public int[,] samplesByChannel { get; set; }
        
        // Normalized from to range of 0.0f to 1.0f
        public float[,] normalizedSamplesByChannel { get; set; }
    }
}