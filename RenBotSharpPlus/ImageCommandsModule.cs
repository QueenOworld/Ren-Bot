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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using ImageProcessor;
using ImageProcessor.Imaging.Filters.Photo;
using SEAMonster.EnergyFunctions;
using SEAMonster.SeamFunctions;
using System.Drawing;
using SEAMonster;
using ImageProcessor.Processors;
using ImageProcessor.Imaging.Filters.EdgeDetection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Drawing.Drawing2D;
using System.Data;
using System.Drawing.Imaging;
using static System.Net.Mime.MediaTypeNames;
using Vdcrpt;
using Vdcrpt.Desktop;
using FFMpegCore;

namespace RenBotSharp
{
    public class ImageCommandsModule : ApplicationCommandModule
    {
        public static HttpClient client = new HttpClient();
        public static CleverBot WisdomGiver = new CleverBot();

        [SlashCommand("jpeg", "Makes images look funy using jpeg compression")]
        private async Task Jpeg(InteractionContext ctx, [Option("image", "image to compress")] DiscordAttachment attachment, [Option("quality", "quality of the jpeg, from 0 to 100 (default: 5)")] long quality = 5)
        {
            if (quality < 0 || quality > 100)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Quality is not in the range of 0 to 100").AsEphemeral());
            }
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage a = await client.GetAsync(attachment.Url);

                using (MemoryStream inStream = new MemoryStream(await a.Content.ReadAsByteArrayAsync()))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            imageFactory.Load(inStream)
                                        .Format(new JpegFormat())
                                        .Quality((int)quality)
                                        .Save(outStream);
                        }

                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Here is your jpeg image!").AddFile("output.jpeg", outStream));
                    }
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not an image").AsEphemeral());
            }
        }
        [SlashCommand("compress", "Compresses an image")]
        private async Task Compress(InteractionContext ctx, [Option("image", "Image to compress")] DiscordAttachment attachment, [Option("quality", "Quality of output image, from 1 to 100 (default: 10)")] long quality = 10)
        {
            if (quality < 1 || quality > 100)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Quality is not in the range of 1 to 100").AsEphemeral());
            }
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage a = await client.GetAsync(attachment.Url);

                using (MemoryStream inStream = new MemoryStream(await a.Content.ReadAsByteArrayAsync()))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            imageFactory.Load(inStream)
                                        .Format(new ImageProcessor.Imaging.Formats.PngFormat())
                                        .Resize(new System.Drawing.Size((int)(attachment.Width/(100/quality)), (int)(attachment.Height/(100/quality))))
                                        .Save(outStream);
                        }

                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Here is your compressed image!").AddFile("output.png", outStream));
                    }
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not an image").AsEphemeral());
            }
        }
        [SlashCommand("wide", "Widens an image")]
        private async Task Wide(InteractionContext ctx, [Option("image", "Image to widen")] DiscordAttachment attachment)
        {
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage a = await client.GetAsync(attachment.Url);

                using (MemoryStream inStream = new MemoryStream(await a.Content.ReadAsByteArrayAsync()))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            ResizeLayer layer = new ResizeLayer(new System.Drawing.Size((int)attachment.Width * 2, (int)attachment.Height), ResizeMode.Stretch);
                            imageFactory.Load(inStream)
                                        .Format(new ImageProcessor.Imaging.Formats.PngFormat())
                                        .Resize(layer)
                                        .Save(outStream);
                        }

                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Here is your  w i d e  image!").AddFile("output.png", outStream));
                    }
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not an image").AsEphemeral());
            }
        }
        [SlashCommand("tall", "Heightens an image")]
        private async Task Tall(InteractionContext ctx, [Option("image", "Image to heighten")] DiscordAttachment attachment)
        {
            try
            {
                if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
                {
                    HttpResponseMessage a = await client.GetAsync(attachment.Url);

                    using (MemoryStream inStream = new MemoryStream(await a.Content.ReadAsByteArrayAsync()))
                    {
                        using (MemoryStream outStream = new MemoryStream())
                        {
                            using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                            {
                                ResizeLayer layer = new ResizeLayer(new System.Drawing.Size((int)attachment.Width, (int)attachment.Height * 2), ResizeMode.Stretch);
                                imageFactory.Load(inStream)
                                            .Format(new ImageProcessor.Imaging.Formats.PngFormat())
                                            .Resize(layer)
                                            .Save(outStream);
                            }

                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Here is your \nt\na\nl\nl\n image!").AddFile("output.png", outStream));
                        }
                    }
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not an image").AsEphemeral());
                }
            }
            catch (Exception ex) { Console.WriteLine(ex); }
        }
        [SlashCommand("deepfry", "Completely deepfries an image with jpeg compression and other funny effects")]
        private async Task DeepFry(InteractionContext ctx, [Option("image", "image to compress")] DiscordAttachment attachment)
        {
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage a = await client.GetAsync(attachment.Url);

                using (MemoryStream inStream = new MemoryStream(await a.Content.ReadAsByteArrayAsync()))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            imageFactory.Load(inStream)
                                        .Format(new JpegFormat())
                                        .Quality(13)
                                        .Gamma(70)
                                        .Brightness(20)
                                        .Resize(new System.Drawing.Size((int)attachment.Width/8, (int)attachment.Height/8))
                                        .Saturation(100)
                                        .Contrast(100)
                                        .Save(outStream);

                            imageFactory.Load(outStream)
                                        .Format(new ImageProcessor.Imaging.Formats.PngFormat())
                                        .Saturation(20)
                                        .Contrast(20)
                                        .Resize(new System.Drawing.Size((int)attachment.Width, (int)attachment.Height))
                                        .Save(outStream);
                        }

                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Here is your deepfried image!").AddFile("output.png", outStream));
                    }
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not an image").AsEphemeral());
            }
        }
        [SlashCommand("destroy", "Completely destroys an image with random funny effects")]
        private async Task Destroy(InteractionContext ctx, [Option("image", "image to compress")] DiscordAttachment attachment)
        {
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage a = await client.GetAsync(attachment.Url);

                List<IMatrixFilter> filters = new List<IMatrixFilter>() { MatrixFilters.LoSatch, MatrixFilters.Invert, MatrixFilters.Polaroid, MatrixFilters.HiSatch, MatrixFilters.BlackWhite, MatrixFilters.Comic, MatrixFilters.Gotham, MatrixFilters.GreyScale, MatrixFilters.Lomograph, MatrixFilters.Sepia };

                using (MemoryStream inStream = new MemoryStream(await a.Content.ReadAsByteArrayAsync()))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            if (RandomNumberGenerator.GetInt32(0, 10) >= 5)
                            {
                                imageFactory.Load(inStream)
                                            .Format(new JpegFormat())
                                            .Quality(RandomNumberGenerator.GetInt32(0, 100))
                                            .ReplaceColor(System.Drawing.Color.FromArgb(RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue)), System.Drawing.Color.FromArgb(RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue)))
                                            .Gamma(RandomNumberGenerator.GetInt32(0, 100))
                                            .Brightness(RandomNumberGenerator.GetInt32(0, 100))
                                            .Saturation(RandomNumberGenerator.GetInt32(0, 100))
                                            .Contrast(RandomNumberGenerator.GetInt32(0, 100))
                                            .Filter(filters[RandomNumberGenerator.GetInt32(0, filters.Count)])
                                            .Save(outStream);
                            }
                            else
                            {
                                imageFactory.Load(inStream)
                                            .Format(new JpegFormat())
                                            .Quality(RandomNumberGenerator.GetInt32(0, 100))
                                            .ReplaceColor(System.Drawing.Color.FromArgb(RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue)), System.Drawing.Color.FromArgb(RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue)))
                                            .Gamma(RandomNumberGenerator.GetInt32(0, 100))
                                            .Brightness(RandomNumberGenerator.GetInt32(0, 100))
                                            .Saturation(RandomNumberGenerator.GetInt32(0, 100))
                                            .Contrast(RandomNumberGenerator.GetInt32(0, 100))
                                            .Save(outStream);
                            }
                        }

                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Here is your destroyed image!").AddFile("output.png", outStream));
                    }
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not an image").AsEphemeral());
            }
        }
        [SlashCommand("greyscale", "Greyscales an image")]
        private async Task GreyScale(InteractionContext ctx, [Option("image", "Image to greyscale")] DiscordAttachment attachment)
        {
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage a = await client.GetAsync(attachment.Url);

                using (MemoryStream inStream = new MemoryStream(await a.Content.ReadAsByteArrayAsync()))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            imageFactory.Load(inStream)
                                        .Format(new ImageProcessor.Imaging.Formats.PngFormat())
                                        .Filter(MatrixFilters.GreyScale)
                                        .Save(outStream);
                        }

                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Here is your greyscaled image!").AddFile("output.png", outStream));
                    }
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not an image").AsEphemeral());
            }
        }
        [SlashCommand("negative", "Inverts an image")]
        private async Task Negative(InteractionContext ctx, [Option("image", "Image to invert")] DiscordAttachment attachment)
        {
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage a = await client.GetAsync(attachment.Url);

                using (MemoryStream inStream = new MemoryStream(await a.Content.ReadAsByteArrayAsync()))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            imageFactory.Load(inStream)
                                        .Format(new ImageProcessor.Imaging.Formats.PngFormat())
                                        .Filter(MatrixFilters.Invert)
                                        .Save(outStream);
                        }

                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Here is your inverted image!").AddFile("output.png", outStream));
                    }
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not an image").AsEphemeral());
            }
        }
        [SlashCommand("comic", "Makes an image look like a comic")]
        private async Task Comic(InteractionContext ctx, [Option("image", "Image to turn into a comic style")] DiscordAttachment attachment)
        {
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage a = await client.GetAsync(attachment.Url);

                using (MemoryStream inStream = new MemoryStream(await a.Content.ReadAsByteArrayAsync()))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            imageFactory.Load(inStream)
                                        .Format(new ImageProcessor.Imaging.Formats.PngFormat())
                                        .Filter(MatrixFilters.Comic)
                                        .Save(outStream);
                        }

                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Here is your comic image!").AddFile("output.png", outStream));
                    }
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not an image").AsEphemeral());
            }
        }
        [SlashCommand("mangas", "Very badly turns an image into a manga style, also I misspelled the command and refuse to fix it")]
        private async Task Manga(InteractionContext ctx, [Option("image", "manga an image")] DiscordAttachment attachment)
        {
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage a = await client.GetAsync(attachment.Url);

                using (MemoryStream inStream = new MemoryStream(await a.Content.ReadAsByteArrayAsync()))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            imageFactory.Load(inStream)
                                        .Format(new ImageProcessor.Imaging.Formats.PngFormat())
                                        .DetectEdges(new SobelEdgeFilter())
                                        .Save(outStream);
                        }

                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Here is your manga image!").AddFile("output.png", outStream));
                    }
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not an image").AsEphemeral());
            }
        }
        [SlashCommand("nsfw", "Blurs an image (no porn sorry :3)")]
        private async Task NSFW(InteractionContext ctx, [Option("image", "Image to blur")] DiscordAttachment attachment)
        {
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage a = await client.GetAsync(attachment.Url);

                using (MemoryStream inStream = new MemoryStream(await a.Content.ReadAsByteArrayAsync()))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            imageFactory.Load(inStream)
                                        .Format(new ImageProcessor.Imaging.Formats.PngFormat())
                                        .Resize(new System.Drawing.Size((int)(attachment.Width / (100 / 3)), (int)(attachment.Height / (100 / 3))))
                                        .Save(outStream);
                            imageFactory.Load(outStream)
                                        .Format(new ImageProcessor.Imaging.Formats.PngFormat())
                                        .Resize(new System.Drawing.Size((int)attachment.Width, (int)attachment.Height))
                                        .Save(outStream);
                        }

                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Here is your nsfw image!").AddFile("output.png", outStream));
                    }
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not an image").AsEphemeral());
            }
        }
        public static byte[] ImageToByte(System.Drawing.Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        [SlashCommand("content-awareness-filter", "Carves an image based on the amount you give it")]
        private async Task ContentAwarenessFilter(InteractionContext ctx, [Option("image", "Image to carve")] DiscordAttachment attachment, [Option("carve-amount", "ranges from 1 to 2000 (default: 1000)")] long amount = 1000, [Option("rescale", "whether to rescale the image to its orginal size or not (default: false)")] bool rescale = false)
        {
            ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            try
            {
                if (amount < 1 || amount > 2000)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Amount is not in the range of 1 to 2000"));
                    return;
                }

                if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
                {
                    HttpResponseMessage a = await client.GetAsync(attachment.Url);

                    SMImage smImage = new SMImage(new Bitmap(await a.Content.ReadAsStreamAsync()), new SEAMonster.SeamFunctions.Standard(), new SEAMonster.EnergyFunctions.Random());

                    for (int i = 0; i < amount; i++)
                    {
                        smImage.Carve(Direction.Optimal, ComparisonMethod.Total, new System.Drawing.Size(smImage.Size.Width - 1, smImage.Size.Height - 1));
                        if (i % 100 == 0)
                        {
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Carving progress:").AddFile("output.png", new MemoryStream(ImageToByte(smImage.CarvedBitmap))));
                        }
                    }

                    using (MemoryStream inStream = new MemoryStream(ImageToByte(smImage.CarvedBitmap)))
                    {
                        using (MemoryStream outStream = new MemoryStream())
                        {
                            using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                            {
                                if (rescale)
                                {
                                    ResizeLayer layer = new ResizeLayer(new System.Drawing.Size((int)attachment.Width, (int)attachment.Height), ResizeMode.Stretch);
                                    imageFactory.Load(inStream)
                                                .Format(new ImageProcessor.Imaging.Formats.PngFormat())
                                                .Resize(layer)
                                                .Save(outStream);
                                }
                                else
                                {
                                    imageFactory.Load(inStream)
                                                .Format(new ImageProcessor.Imaging.Formats.PngFormat())
                                                .Save(outStream);
                                }

                                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Here is your carved image!").AddFile("output.png", outStream));
                            }
                        }
                    }
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not an image").AsEphemeral());
                }
            }
            catch
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Oh brother, this guy stinks!!!!!\nhttps://cdn.discordapp.com/attachments/1064410879594078310/1135743827202822244/oh-brother-this-guy-stinks.mp3"));
            }
        }
        [SlashCommand("corrupt", "Corrupts a file")]
        private async Task Corrupt(InteractionContext ctx, [Option("file", "File to corrupt (up to 10 MB)")] DiscordAttachment attachment, [Option("intensity", "Intensity of corruption, ranging from 1-1000 (default:10)")] long intensity = 10)
        {
            if (intensity < 1 || intensity > 1000)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Intensity is not in the range of 1 to 1000").AsEphemeral());
                return;
            }

            if (attachment.FileSize > 10000000)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("File can't be higher than 10 MB").AsEphemeral());
                return;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            HttpResponseMessage a = await client.GetAsync(attachment.Url);

            byte[] sexo = await a.Content.ReadAsByteArrayAsync();

            for (int i = 0; i < intensity; i++)
            {
                sexo[System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, sexo.Length)] = System.Security.Cryptography.RandomNumberGenerator.GetBytes(1)[0];
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Here is your corrupted file!").AddFile(attachment.FileName, new MemoryStream(sexo)));
        }
        [SlashCommand("quote", "Generates a beautiful inspirational quote")]
        private async Task Quote(InteractionContext ctx)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());
                JObject images = JObject.Parse(await client.GetStringAsync($"https://pixabay.com/api/?key=38657420-44280e23904e56d081c4ab35b&page={RandomNumberGenerator.GetInt32(1, 167)}&per_page=3"));

                JToken result = images["hits"].ElementAt(RandomNumberGenerator.GetInt32(0, images["hits"].Count()));

                HttpResponseMessage a = await client.GetAsync(result["webformatURL"].ToString());

                using (MemoryStream inStream = new MemoryStream(await a.Content.ReadAsByteArrayAsync()))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            imageFactory.Load(inStream)
                                        .Format(new ImageProcessor.Imaging.Formats.PngFormat())
                                        .Save(outStream);
                        }

                        Bitmap myBitmap = new Bitmap(outStream);
                        var rectf = new System.Drawing.RectangleF(0, 3, myBitmap.Width - 5, myBitmap.Height / 2); //rectf for My Text
                        using (Graphics g = Graphics.FromImage(myBitmap))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                            g.CompositingQuality = CompositingQuality.HighQuality;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            StringFormat sf = new StringFormat();
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Center;
                            GraphicsPath path = new GraphicsPath();
                            
                            path.AddString(
                                await WisdomGiver.SendCleverbotMessage(
                                    string.Join(' ', JsonConvert.DeserializeObject<string[]>(
                                        await client.GetStringAsync("https://random-word-api.herokuapp.com/word?number=3")))), 
                                FontFamily.Families[RandomNumberGenerator.GetInt32(0, FontFamily.Families.Length)], 
                                (int)Math.Pow(2, RandomNumberGenerator.GetInt32(0, 2)), 
                                g.DpiY * RandomNumberGenerator.GetInt32(16, 42) / 72, rectf, sf);

                            g.DrawPath(Pens.Black, path);
                            g.FillPath(Brushes.White, path);
                            g.Save();
                        }

                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Here is your quote!").AddFile("output.png", new MemoryStream(ImageToByte(myBitmap))));
                    }
                }
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        [SlashCommand("datamosh", "Datamosh a video")]
        private async Task Datamosh(InteractionContext ctx, [Option("video", "Media to datamosh (up to 10 MB)")] DiscordAttachment attachment,
            [Choice("Melt", 0)]
            [Choice("Jitter", 1)]
            [Choice("Source Engine", 2)]
            [Choice("Subtle", 3)]
            [Choice("Many Artifacts", 4)]
            [Choice("Trash", 5)]
            [Choice("Legacy", 6)]
            [Choice("Death", 7)]
            [Choice("Standard Subtle", 8)]
            [Choice("Standard", 9)]
            [Choice("Standard Intense", 11)]
            [Option("preset", "Preset to use (default: Standard)")] long preset = 9)
        {
            if (attachment.FileSize > 10000000)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("File can't be higher than 10 MB").AsEphemeral());
                return;
            }
            if (attachment.MediaType.Contains("video"))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

                HttpResponseMessage a = await client.GetAsync(attachment.Url);

                byte[] sexo = await a.Content.ReadAsByteArrayAsync();

                string inName = Guid.NewGuid().ToString();
                string outName = Guid.NewGuid().ToString();

                File.WriteAllBytes($"{Environment.CurrentDirectory}/temp/{inName}.mp4", sexo);

                try
                {
                    if (preset == 7) 
                    {
                        var video = VdcrptR.Video.Load($"{Environment.CurrentDirectory}/temp/{inName}.mp4");

                        video.Transform(VdcrptR.Effects.Death(10));

                        video.Transform(VdcrptR.Effects.Repeat(Preset.DefaultPresets[0].Iterations, Preset.DefaultPresets[0].BurstSize, Preset.DefaultPresets[0].MinBurstLength, Preset.DefaultPresets[0].UseLengthRange ? Preset.DefaultPresets[0].MaxBurstLength : Preset.DefaultPresets[0].MinBurstLength));

                        video.Transform(VdcrptR.Effects.Repeat(Preset.DefaultPresets[5].Iterations, Preset.DefaultPresets[5].BurstSize, Preset.DefaultPresets[5].MinBurstLength, Preset.DefaultPresets[5].UseLengthRange ? Preset.DefaultPresets[5].MaxBurstLength : Preset.DefaultPresets[5].MinBurstLength));

                        video.Transform(VdcrptR.Effects.Repeat(Preset.DefaultPresets[4].Iterations, Preset.DefaultPresets[4].BurstSize, Preset.DefaultPresets[4].MinBurstLength, Preset.DefaultPresets[4].UseLengthRange ? Preset.DefaultPresets[4].MaxBurstLength : Preset.DefaultPresets[4].MinBurstLength));

                        video.Save($"{Environment.CurrentDirectory}/temp/{outName}.mp4");
                    }
                    else if (preset >= 8)
                    {
                        var video = VdcrptR.Video.Load($"{Environment.CurrentDirectory}/temp/{inName}.mp4");

                        video.Transform(VdcrptR.Effects.Mosh((int)preset - 6));

                        video.Save($"{Environment.CurrentDirectory}/temp/{outName}.mp4");
                    }
                    else
                    {
                        var video = VdcrptR.Video.Load($"{Environment.CurrentDirectory}/temp/{inName}.mp4");

                        video.Transform(VdcrptR.Effects.Repeat(Preset.DefaultPresets[(int)preset].Iterations, Preset.DefaultPresets[(int)preset].BurstSize, Preset.DefaultPresets[(int)preset].MinBurstLength, Preset.DefaultPresets[(int)preset].UseLengthRange ? Preset.DefaultPresets[(int)preset].MaxBurstLength : Preset.DefaultPresets[(int)preset].MinBurstLength));

                        video.Save($"{Environment.CurrentDirectory}/temp/{outName}.mp4");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Here is your datamoshed video!").AddFile($"{attachment.FileName}.mp4", new MemoryStream(File.ReadAllBytes($"{Environment.CurrentDirectory}/temp/{outName}.mp4"))));

                File.Delete($"{Environment.CurrentDirectory}/temp/{inName}.mp4");
                File.Delete($"{Environment.CurrentDirectory}/temp/{outName}.mp4");
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not a video or gif").AsEphemeral());
            }
        }
    }
}