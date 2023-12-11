/*
Ren Bot is a discord bot with some silly features included.
Copyright (C) 2023 - queenoworld

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
using FFMpegCore;
using FFMpegCore.Arguments;

namespace VdcrptR
{
    /// <summary>
    /// A Video contains raw AVI video data and provides methods for manipulating it. It simplifies import and export
    /// from formats supported by FFMpeg.
    /// </summary>
    public class Video
    {
        private readonly List<byte> _videoData;

        public int Size => _videoData.Count;

        private Video(List<byte> videoData)
        {
            _videoData = videoData;
        }

        private static string GetCachedFilePath(string path)
        {
            using var md5 = MD5.Create();
            using var fileStream = File.OpenRead(path);
            var hashString = BitConverter.ToString(md5.ComputeHash(fileStream));
            return Path.Join(Path.GetTempPath(), Path.ChangeExtension(hashString, ".vdcrpt-data"));
        }

        /// <summary>
        /// Loads (and optionally caches) an input video from the given path. Converts the container to AVI for easy
        /// corruption.
        /// </summary>
        /// <param name="inputPath">Input video</param>
        /// <param name="videoCodec">FFMpeg-supported video codec to use for corruption</param>
        /// <param name="audioCodec">FFMpeg-supported audio codec to use for corruption</param>
        /// <returns></returns>
        public static Video Load(string inputPath, string videoCodec = "mpeg4", string audioCodec = "pcm_mulaw")
        {
            var outputPath = GetCachedFilePath(inputPath);

            if (!File.Exists(outputPath))
            {
                FFMpegArguments
                    .FromFileInput(inputPath)
                    .OutputToFile(outputPath, true, args => args
                        .WithVideoCodec(videoCodec)
                        .WithAudioCodec(audioCodec)
                        .ForceFormat("avi"))
                    .ProcessSynchronously();
            }

            return new Video(new List<byte>(File.ReadAllBytes(outputPath)));
        }

        public static bool CacheExists(string inputPath)
        {
            return File.Exists(GetCachedFilePath(inputPath));
        }

        /// <summary>
        /// Mutates this Video's binary data using a given function.
        /// </summary>
        /// <param name="action">Function to apply to binary data blob</param>
        /// <returns>This Video for chaining</returns>
        public Video Transform(Action<List<byte>> action)
        {
            action(_videoData);
            return this;
        }

        /// <summary>
        /// Re-renders this video using a given video and audio codec.
        /// </summary>
        /// <param name="outputPath">Path to save video to, will be overwritten</param>
        /// <param name="videoCodec">FFMpeg-supported video codec to use for output</param>
        /// <param name="audioCodec">FFMpeg-supported audio codec to use for output</param>
        public void Save(string outputPath, string videoCodec = "libx264", string audioCodec = "aac")
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, _videoData.ToArray());

            FFMpegArguments
                .FromFileInput(tempFile)
                .WithGlobalOptions(args => args
                    .WithVerbosityLevel(VerbosityLevel.Fatal))
                .OutputToFile(outputPath, true, args => args
                    .WithoutMetadata()
                    .WithCustomArgument("-fflags +genpts")
                    .WithVideoCodec(videoCodec)
                    .WithAudioCodec(audioCodec))
                .ProcessSynchronously();
        }
    }
}