/*
Ren Bot is a discord bot with some silly features included.
Copyright (C) 2023 - kingoworld

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using StutterMosher;

namespace VdcrptR
{
    // TODO: Note at top of Video.cs also applies here. Don't build anything new with this, it's going to be redone
    public static class Effects
    {
        public static Action<List<byte>> Death(int iterations)
        {
            return data =>
            {
                using (var istream = new MemoryStream(data.ToArray()))
                using (var ostream = new MemoryStream())
                {
                    bool iFrameYet = false;

                    while (true)
                    {
                        Frame frame = Frame.ReadFromStream(istream);
                        if (frame == null)
                            break;
                        if (!iFrameYet)
                        {
                            for (int i = 0; i < iterations; i++)
                            {
                                if (RandomNumberGenerator.GetInt32(0, 10) > 7)
                                {
                                    frame.WriteToStream(ostream);
                                }
                            }
                            if (frame.IsIFrame) iFrameYet = true;
                        }
                        else if (frame.IsPFrame)
                        {

                            for (int i = 0; i < iterations; i++)
                            {
                                if (RandomNumberGenerator.GetInt32(0, 10) > 7)
                                {
                                    frame.WriteToStream(ostream);
                                }
                            }
                        }
                    }
                    data.Clear();
                    data.AddRange(ostream.ToArray());
                }
            };
        }
        public static Action<List<byte>> Mosh(int iterations)
        {
            return data =>
            {
                using (var istream = new MemoryStream(data.ToArray()))
                using (var ostream = new MemoryStream())
                {
                    bool iFrameYet = false;
                    while (true)
                    {
                        Frame frame = Frame.ReadFromStream(istream);
                        if (frame == null)
                            break;
                        if (!iFrameYet)
                        {
                            frame.WriteToStream(ostream);
                            if (frame.IsIFrame) iFrameYet = true;
                        }
                        else if (frame.IsPFrame)
                        {
                            for (int n = 0; n < iterations; n++)
                                frame.WriteToStream(ostream);
                        }
                    }
                    data.Clear();
                    data.AddRange(ostream.ToArray());
                }
            };
        }
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