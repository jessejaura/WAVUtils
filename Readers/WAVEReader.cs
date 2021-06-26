using System;
using System.IO;
using WAVUtils.Models;

namespace WAVUtils.Controllers
{
    public class WAVEReader
    {
        public static WAVE readWAVFile(string file)
        {
            WAVE wave = new WAVE();
            BinaryReader binReader = new BinaryReader(File.Open(file, FileMode.Open));
            
            // Read "RIFF" chunk
            wave.ChunkID = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(4));
            wave.ChunkSize = Convert.ToInt32(binReader.ReadUInt32());
            wave.Format = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(4));

            if (wave.ChunkID != "RIFF")
            {
                throw new FormatException("File is not in RIFF format");
            }
            if (wave.Format != "WAVE")
            {
                throw new FormatException("File is not in WAVE format");
            }
            
            // Read "fmt" chunk
            wave.fmt = new Fmt
            {
                subchunk1ID = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(4)),
                subchunk1Size = Convert.ToInt32(binReader.ReadUInt32()),
                audioFormat = Convert.ToInt16(binReader.ReadUInt16()),
                numChannels = Convert.ToInt16(binReader.ReadUInt16()),
                sampleRate = Convert.ToInt32(binReader.ReadUInt32()),
                byteRate = Convert.ToInt32(binReader.ReadUInt32()),
                blockAlign = Convert.ToInt16(binReader.ReadUInt16()),
                bitsPerSample = Convert.ToInt16(binReader.ReadUInt16())
            };

            int expectedByteRate = wave.fmt.sampleRate * wave.fmt.numChannels * (wave.fmt.bitsPerSample / 8);
            if (wave.fmt.byteRate != expectedByteRate)
            {
                throw new FormatException(
                    $"Invalid Byte Rate. Expected {expectedByteRate}, but got {wave.fmt.byteRate}");
            }
            
            int expectedBlockAlign = wave.fmt.numChannels * (wave.fmt.bitsPerSample / 8);
            if (wave.fmt.blockAlign != expectedBlockAlign)
            {
                throw new FormatException(
                    $"Invalid Block Align. Expected {expectedBlockAlign}, but got {wave.fmt.blockAlign}");
            }
            
            
            // Read "data" chunk
            wave.subchunk2ID = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(4));
            wave.subchunk2Size = Convert.ToInt32(binReader.ReadUInt32());

            int numSamples = wave.subchunk2Size / wave.fmt.numChannels / (wave.fmt.bitsPerSample / 8);
            wave.samplesByChannel = new int[wave.fmt.numChannels, numSamples];
            for (int sample = 0; sample < numSamples; sample++)
            {
                for (int channel = 0; channel < wave.fmt.numChannels; channel++)
                {
                    int sampleValue = wave.fmt.bitsPerSample switch
                    {
                        8 => binReader.ReadChar(),
                        16 => binReader.ReadInt16(),
                        _ => throw new ArgumentException($"{wave.fmt.bitsPerSample} bits per sample not supported")
                    };

                    wave.samplesByChannel[channel, sample] = sampleValue;
                }
            }

            wave.normalizedSamplesByChannel = normalizeWAVESamples(wave);
            return wave;
        }

        private static float[,] normalizeWAVESamples(WAVE wave)
        {
            int numSamples = wave.subchunk2Size / wave.fmt.numChannels / (wave.fmt.bitsPerSample / 8);
            float[,] normalizedSamples = new float[wave.fmt.numChannels, numSamples];

            for (int channel = 0; channel < wave.fmt.numChannels; channel++)
            {
                for (int sample = 0; sample < numSamples; sample++)
                {
                    normalizedSamples[channel, sample] = wave.fmt.bitsPerSample switch
                    {
                        8 => wave.samplesByChannel[channel, sample] / 255f,
                        16 => (wave.samplesByChannel[channel, sample] + 32768f) / 65535f,
                        _ => throw new ArgumentException($"{wave.fmt.bitsPerSample} bits per sample not supported")
                    };
                    
                }
            }

            return normalizedSamples;
        }

        public static string WAVEToString(WAVE wave)
        {
            string output = "";
            
            // Append RIFF chunk
            output += $"RIFF Chunk\nChunkID: {wave.ChunkID}\n" +
                      $"ChunkSize: {wave.ChunkSize}\n" +
                      $"Format: {wave.Format}\n\n";
            
            // Append fmt chunk
            Fmt fmtChunk = wave.fmt;
            output += $"fmt Chunk\nSubchunk1ID: {fmtChunk.subchunk1ID}\n" +
                      $"Subchunk1Size: {fmtChunk.subchunk1Size}\n" +
                      $"AudioFormat: {fmtChunk.audioFormat}\n" +
                      $"NumChannels: {fmtChunk.numChannels}\n" +
                      $"SampleRate: {fmtChunk.sampleRate}\n" +
                      $"ByteRate: {fmtChunk.byteRate}\n" +
                      $"BlockAlign: {fmtChunk.blockAlign}\n" +
                      $"BitsPerSample: {fmtChunk.bitsPerSample}\n\n";
            
            // Append data chunk
            output += $"data Chunk\nSubchunk2ID: {wave.subchunk2ID}\n" +
                      $"Subchunk2Size: {wave.subchunk2Size}";

            return output;
        }
    }
}