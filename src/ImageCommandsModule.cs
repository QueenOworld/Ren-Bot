using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SEAMonster;
using SEAMonster.EnergyFunctions;
using SEAMonster.SeamFunctions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Vdcrpt;
using Vdcrpt.Desktop;

namespace RenBot
{
    public class ImageCommands : ApplicationCommandModule
    {
        private readonly HttpClient _httpClient;
        private readonly CleverBot _cleverbot;

        public ImageCommands(HttpClient httpClient, CleverBot cleverbot)
        {
            _httpClient = httpClient;
            _cleverbot = cleverbot;
        }

        public static byte[] ImageToByte(System.Drawing.Image img)
        {
            System.Drawing.ImageConverter converter = new System.Drawing.ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        [SlashCommand("blur", "blurs an image")]
        private async Task Blur(InteractionContext ctx, [Option("image", "image to manipulate")] DiscordAttachment attachment)
        {
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage httpResponse = await _httpClient.GetAsync(attachment.Url);

                using (Stream inStream = await httpResponse.Content.ReadAsStreamAsync())
                {
                    using (var image = Image.Load(inStream))
                    {
                        image.Mutate(x => x.BoxBlur(15));
                        await image.SaveAsync($"./tmp/{attachment.FileName}");
                    }
                }

                using (FileStream outStream = new FileStream($"./tmp/{attachment.FileName}", FileMode.Open))
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                              .WithContent("Here is your blurred image!").AddFile(attachment.FileName, outStream));
                }

                File.Delete($"./tmp/{attachment.FileName}");
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .WithContent("The file you provided is not an image").AsEphemeral());
            }
        }
        [SlashCommand("invert", "invert an image")]
        private async Task Invert(InteractionContext ctx, [Option("image", "image to manipulate")] DiscordAttachment attachment)
        {
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage httpResponse = await _httpClient.GetAsync(attachment.Url);

                using (Stream inStream = await httpResponse.Content.ReadAsStreamAsync())
                {
                    using (var image = Image.Load(inStream))
                    {
                        image.Mutate(x => x.Invert());
                        await image.SaveAsync($"./tmp/{attachment.FileName}");
                    }
                }

                using (FileStream outStream = new FileStream($"./tmp/{attachment.FileName}", FileMode.Open))
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                              .WithContent("Here is your inverted image!").AddFile(attachment.FileName, outStream));
                }

                File.Delete($"./tmp/{attachment.FileName}");
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .WithContent("The file you provided is not an image").AsEphemeral());
            }
        }
        [SlashCommand("jpeg", "Makes images look funy using jpeg compression")]
        private async Task Jpeg(InteractionContext ctx, [Option("image", "image to manipulate")] DiscordAttachment attachment, [Option("quality", "quality of the jpeg, from 0 to 100 (default: 5)")] long quality = 5)
        {
            if (quality < 0 || quality > 100)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Quality is not in the range of 0 to 100").AsEphemeral());
            }
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage httpResponse = await _httpClient.GetAsync(attachment.Url);

                using (Stream inStream = await httpResponse.Content.ReadAsStreamAsync())
                {
                    using (var image = Image.Load(inStream))
                    {
                        await image.SaveAsync($"./tmp/{attachment.FileName}", new JpegEncoder() { Quality = (int)quality });
                    }
                }

                using (FileStream outStream = new FileStream($"./tmp/{attachment.FileName}", FileMode.Open))
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                              .WithContent("Here is your jpeg'd image!").AddFile(attachment.FileName, outStream));
                }

                File.Delete($"./tmp/{attachment.FileName}");
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .WithContent("The file you provided is not an image").AsEphemeral());
            }

        }
        [SlashCommand("quote", "Generates a beautiful inspirational quote")]
        private async Task Quote(InteractionContext ctx)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());
                JObject images = JObject.Parse(await _httpClient.GetStringAsync($"https://pixabay.com/api/?key=38657420-44280e23904e56d081c4ab35b&page={RandomNumberGenerator.GetInt32(1, 167)}&per_page=3"));

                JToken result = images["hits"].ElementAt(RandomNumberGenerator.GetInt32(0, images["hits"].Count()));

                HttpResponseMessage httpResponse = await _httpClient.GetAsync(result["webformatURL"].ToString());

                string tempName = httpResponse.GetHashCode().ToString();

                Font font = SystemFonts.Families.ToList()[RandomNumberGenerator.GetInt32(0, SystemFonts.Families.ToList().Count())].CreateFont(RandomNumberGenerator.GetInt32(8, 24));

                string text = await _cleverbot.SendCleverbotMessage(
                                    string.Join(' ', JsonConvert.DeserializeObject<string[]>(
                                        await _httpClient.GetStringAsync("https://random-word-api.herokuapp.com/word?number=3"))));

                using (Stream inStream = await httpResponse.Content.ReadAsStreamAsync())
                {
                    using (var image = Image.Load(inStream))
                    {
                        image.Mutate(x => x.DrawText(text, font, Color.Black, new PointF(image.Width / 2 + RandomNumberGenerator.GetInt32(-20, 20), image.Height / 2 + RandomNumberGenerator.GetInt32(-20, 20))));
                        await image.SaveAsync($"./tmp/{tempName.ToString()}.png");
                    }
                }

                using (FileStream outStream = new FileStream($"./tmp/{tempName.ToString()}.png", FileMode.Open))
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                              .WithContent("Here is your quote!").AddFile("output.png", outStream));
                }

                File.Delete($"./tmp/{tempName.ToString()}.png");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        [SlashCommand("content-awareness-filter", "Carves an image based on the amount you give it")]
        private async Task ContentAwarenessFilter(InteractionContext ctx, [Option("image", "image to manipulate")] DiscordAttachment attachment, [Option("carve-amount", "ranges from 1 to 2000 (default: 1000)")] long amount = 1000, [Option("rescale", "whether to rescale the image to its orginal size or not (default: false)")] bool rescale = false)
        {
            ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            try
            {
                if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
                {
                    HttpResponseMessage httpResponse = await _httpClient.GetAsync(attachment.Url);

                    SMImage smImage = new SMImage(new System.Drawing.Bitmap(await httpResponse.Content.ReadAsStreamAsync()), new SEAMonster.SeamFunctions.Standard(), new SEAMonster.EnergyFunctions.Random());

                    for (int i = 0; i < amount; i++)
                    {
                        smImage.Carve(Direction.Optimal, ComparisonMethod.Total, new System.Drawing.Size(smImage.Size.Width - 1, smImage.Size.Height - 1));
                        if (i % 100 == 0)
                        {
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Carving progress:").AddFile("output.png", new MemoryStream(ImageToByte(smImage.CarvedBitmap))));
                        }
                    }

                    using (Stream inStream = new MemoryStream(ImageToByte(smImage.CarvedBitmap)))
                    {
                        using (var image = Image.Load(inStream))
                        {
                            await image.SaveAsync($"./tmp/{attachment.FileName}");
                        }
                    }

                    using (FileStream outStream = new FileStream($"./tmp/{attachment.FileName}", FileMode.Open))
                    {
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                                  .WithContent("Here is your inverted image!").AddFile(attachment.FileName, outStream));
                    }

                    File.Delete($"./tmp/{attachment.FileName}");
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                            .WithContent("The file you provided is not an image").AsEphemeral());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Oh brother, this guy stinks!!!!!\nhttps://cdn.discordapp.com/attachments/1064410879594078310/1135743827202822244/oh-brother-this-guy-stinks.mp3"));
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

                HttpResponseMessage httpResponse = await _httpClient.GetAsync(attachment.Url);

                byte[] videoData = await httpResponse.Content.ReadAsByteArrayAsync();

                string inName = Guid.NewGuid().ToString();
                string outName = Guid.NewGuid().ToString();

                File.WriteAllBytes($"./tmp/{inName}.mp4", videoData);

                try
                {
                    if (preset == 7)
                    {
                        var video = VdcrptR.Video.Load($"./tmp/{inName}.mp4");

                        video.Transform(VdcrptR.Effects.Death(10));

                        video.Transform(VdcrptR.Effects.Repeat(Preset.DefaultPresets[0].Iterations, Preset.DefaultPresets[0].BurstSize, Preset.DefaultPresets[0].MinBurstLength, Preset.DefaultPresets[0].UseLengthRange ? Preset.DefaultPresets[0].MaxBurstLength : Preset.DefaultPresets[0].MinBurstLength));

                        video.Transform(VdcrptR.Effects.Repeat(Preset.DefaultPresets[5].Iterations, Preset.DefaultPresets[5].BurstSize, Preset.DefaultPresets[5].MinBurstLength, Preset.DefaultPresets[5].UseLengthRange ? Preset.DefaultPresets[5].MaxBurstLength : Preset.DefaultPresets[5].MinBurstLength));

                        video.Transform(VdcrptR.Effects.Repeat(Preset.DefaultPresets[4].Iterations, Preset.DefaultPresets[4].BurstSize, Preset.DefaultPresets[4].MinBurstLength, Preset.DefaultPresets[4].UseLengthRange ? Preset.DefaultPresets[4].MaxBurstLength : Preset.DefaultPresets[4].MinBurstLength));

                        video.Save($"./tmp/{outName}.mp4");
                    }
                    else if (preset >= 8)
                    {
                        var video = VdcrptR.Video.Load($"./tmp/{inName}.mp4");

                        video.Transform(VdcrptR.Effects.Mosh((int)preset - 6));

                        video.Save($"./tmp/{outName}.mp4");
                    }
                    else
                    {
                        var video = VdcrptR.Video.Load($"./tmp/{inName}.mp4");

                        video.Transform(VdcrptR.Effects.Repeat(Preset.DefaultPresets[(int)preset].Iterations, Preset.DefaultPresets[(int)preset].BurstSize, Preset.DefaultPresets[(int)preset].MinBurstLength, Preset.DefaultPresets[(int)preset].UseLengthRange ? Preset.DefaultPresets[(int)preset].MaxBurstLength : Preset.DefaultPresets[(int)preset].MinBurstLength));

                        video.Save($"./tmp/{outName}.mp4");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Here is your datamoshed video!").AddFile($"{attachment.FileName}.mp4", new MemoryStream(File.ReadAllBytes($"./tmp/{outName}.mp4"))));

                File.Delete($"./tmp/{inName}.mp4");
                File.Delete($"./tmp/{outName}.mp4");
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not a video or gif").AsEphemeral());
            }
        }
    }
}
