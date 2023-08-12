using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace VdcrptR
{
    // TODO: Note at top of Video.cs also applies here. Don't build anything new with this, it's going to be redone
    public static class Effects
    {
        public static Action<List<byte>> Repeat(int iterations,
            int chunkSize,
            int minRepetitions,
            int maxRepetitions 
        )
        {
            return data =>
            {
                var bytes = data.ToArray();
                
                var repetitions = new int[iterations];
                if (minRepetitions == maxRepetitions)
                {
                    Array.Fill(repetitions, minRepetitions);
                }
                else
                {
                    for (var i = 0; i < iterations; i++)
                    {
                        repetitions[i] = RandomNumberGenerator.GetInt32(minRepetitions, maxRepetitions + 1);
                    }
                }

                var positions = new int[iterations];
                for (var i = 0; i < iterations; i++)
                {
                    positions[i] = RandomNumberGenerator.GetInt32(32, data.Count - chunkSize);
                }

                Array.Sort(positions);

                using var stream = new MemoryStream();
                using var writer = new BinaryWriter(stream);

                var lastEnd = 0;
                for (var i = 0; i < positions.Length; i++)
                {
                    var pos = positions[i];
                    writer.Write(bytes, lastEnd, pos - lastEnd);
                    
                    for (var j = 0; j < repetitions[i]; j++)
                    {
                        writer.Write(bytes, pos, chunkSize);
                    }

                    lastEnd = pos;
                }

                writer.Write(bytes, lastEnd, data.Count - lastEnd);

                data.Clear();
                data.AddRange(stream.ToArray());
            };
        }
    }
}