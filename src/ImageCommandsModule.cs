using System;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Net;
using System.Net.Http;
using System.IO;

namespace RenBot
{
    public class ImageCommands : ApplicationCommandModule
    {
        private readonly HttpClient _httpClient;
        public ImageCommands(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        [SlashCommand("blur", "blurs an image")]
        private async Task Blur(InteractionContext ctx, [Option("image", "image to manipulate")] DiscordAttachment attachment)
        {
            if (attachment.MediaType.Contains("image") && !attachment.MediaType.Contains("gif"))
            {
                HttpResponseMessage httpResponse = await _httpClient.GetAsync(attachment.Url);

                using (MemoryStream inStream = new MemoryStream(await httpResponse.Content.ReadAsByteArrayAsync()))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (var image = Image.Load(inStream))
                        {
                            image.Mutate(x => x.BoxBlur(15));
                            image.Save(outStream, image.Metadata.DecodedImageFormat);
                        }
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Here is your blurred image!").AddFile(attachment.FileName, outStream));
                    }
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The file you provided is not an image").AsEphemeral());
            }
        }
    }
}
