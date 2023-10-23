using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;
using System.Collections;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using YoutubeExplode.Playlists;
using YoutubeExplode.Common;

namespace RenBotSharp
{
    public class AudioCommandsModule : ApplicationCommandModule
    {
        public static bool ExternalJoin = false;
        public static bool ExternalRTS = false;
        HttpClient client = new HttpClient();
        CleverBot WisdomBot = new CleverBot();

        [SlashCommand("rts", "Talk through Ren Bot, or be annoying :3")]
        private async Task RTS(InteractionContext ctx, [Option("text", "what to say")] string text)
        {
            if (!ExternalRTS)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

                ExternalJoin = true;
                await Join(ctx);
                ExternalJoin = false;

                if (text.Length > 200)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Text can't be more than 200 characters long"));
                    return;
                }
            }

            GoogleTextToSpeech.Option option = new GoogleTextToSpeech.Option();
            option.Lang = Settings.CurrentLanguage;

            string audioUrl = GoogleTextToSpeech.GetAudioUrl(text, option);

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("The Lavalink connection is not established"));
                return;
            }

            var loadResult = await node.Rest.GetTracksAsync(new Uri(audioUrl));

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);

            if (!ExternalRTS)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Sent RTS: {text}"));
            }
        }
        [SlashCommand("play", "Can play youtube videos & media links, as well as use search queries like \"Tenebre Rosso Sangue\"")]
        private async Task Play(InteractionContext ctx, [Option("query", "what to play")] string query)
        {
            ExternalJoin = true;
            await Join(ctx);
            ExternalJoin = false;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The Lavalink connection is not established"));
                return;
            }

            var loadResult = await node.Rest.GetTracksAsync(query);

            //If something went wrong on Lavalink's end                          
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed

                //or it just couldn't find anything.
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                loadResult = await node.Rest.GetTracksAsync(new Uri(query));

                //If something went wrong on Lavalink's end                          
                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed

                    //or it just couldn't find anything.
                    || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Track search failed for {query}."));
                    return;
                }
            }

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Now playing {track.Uri}!"));
        }
        [SlashCommand("playlist", "play a youtube playlist")]
        private async Task Playlist(InteractionContext ctx, [Option("playlist", "playlist url to play")] string url)
        {
            ExternalJoin = true;
            await Join(ctx);
            ExternalJoin = false;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The Lavalink connection is not established"));
                return;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            YoutubeExplode.YoutubeClient client = new YoutubeExplode.YoutubeClient();

            var playlist = client.Playlists.GetVideosAsync(url);

            List<string> urls = new List<string>();

            await foreach (var gock in playlist)
            {
                urls.Add(gock.Url);
            }

            foreach (var video in urls)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Now playing {video}"));
                await Play(ctx, video);
            }
        }
        [SlashCommand("seek", "seek through a video")]
        private async Task Seek(InteractionContext ctx, [Option("seconds", "how many seconds to seek")] long seconds)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The Lavalink connection is not established"));
                return;
            }

            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);

            await conn.SeekAsync(timeSpan);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Seeked {seconds} seconds"));
        }
        [SlashCommand("playrandom", "Plays a random youtube video")]
        private async Task PlayRandom(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            ExternalJoin = true;
            await Join(ctx);
            ExternalJoin = false;

            string query = string.Join(' ', JsonConvert.DeserializeObject<string[]>(await client.GetStringAsync("https://random-word-api.herokuapp.com/word?number=2")));

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("The Lavalink connection is not established"));
                return;
            }

            var loadResult = await node.Rest.GetTracksAsync(query);

            //If something went wrong on Lavalink's end                          
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed

                //or it just couldn't find anything.
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                loadResult = await node.Rest.GetTracksAsync(new Uri(query));

                //If something went wrong on Lavalink's end                          
                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed

                    //or it just couldn't find anything.
                    || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Track search failed for {query}."));
                    return;
                }
            }

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Now playing {track.Uri}!"));
        }
        [SlashCommand("leave", "Leaves current voice channel if in one")]
        private async Task Leave(InteractionContext ctx, [Option("channel", "channel to leave")][ChannelTypes(ChannelType.Voice)] DiscordChannel channel = null)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The Lavalink connection is not established"));
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel == null)
            {
                if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You are not in a voice channel"));
                    return;
                }
                channel = ctx.Member.VoiceState.Channel;
            }

            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The Lavalink connection is not established"));
                return;
            }

            await conn.DisconnectAsync();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Left {channel.Name}!"));
        }
        [SlashCommand("join", "Joins a vc, leave empty to have it join your vc")]
        private async Task Join(InteractionContext ctx, [Option("channel", "channel to join")] [ChannelTypes(ChannelType.Voice)] DiscordChannel channel = null)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The Lavalink connection is not established"));
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel == null) {
                if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You are not in a voice channel"));
                    return;
                }
                channel = ctx.Member.VoiceState.Channel;
            }

            await node.ConnectAsync(channel);
            if (!ExternalJoin)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Joined {channel.Name}!"));
            }
        }
        [SlashCommand("pause", "Pauses current playing audio")]
        public async Task Pause(InteractionContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You are not in a voice channel"));
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The Lavalink connection is not established"));
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("There are no tracks loaded"));
                return;
            }

            await conn.PauseAsync();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Paused audio!"));
        }
        [SlashCommand("resume", "Resumes current audio if it is paused")]
        public async Task Resume(InteractionContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You are not in a voice channel"));
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The Lavalink connection is not established"));
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("There are no tracks loaded"));
                return;
            }

            await conn.ResumeAsync();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Resumed Audio!"));
        }
        [SlashCommand("stop", "Stops the audio player")]
        public async Task Stop(InteractionContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You are not in a voice channel"));
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The Lavalink connection is not established"));
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("There are no tracks loaded"));
                return;
            }

            await conn.StopAsync();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Stopped Audio!"));
        }

        [SlashCommand("vcwisdom", "Ren Bot joins vc and gives his wisdom through RTS")]
        private async Task VCWisdom(InteractionContext ctx)
        {
            ExternalJoin = true;
            await Join(ctx);
            ExternalJoin = false;

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

            string words = string.Join(' ', JsonConvert.DeserializeObject<string[]>(await client.GetStringAsync("https://random-word-api.herokuapp.com/word?number=3")));

            string toSay = await WisdomBot.SendCleverbotMessage(words);

            ExternalRTS = true;
            await RTS(ctx, toSay);
            ExternalRTS = false;

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(toSay));
        }
        [SlashCommand("volume", "Changes volume of audio (you don't need to put a number but I haven\'t tested that so have fun)")]
        public async Task Volume(InteractionContext ctx, [Option("volume", "what to set the volume to")] string volume)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You are not in a voice channel"));
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("The Lavalink connection is not established"));
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("There are no tracks loaded"));
                return;
            }

            int Volume;

            try
            {
                Volume = int.Parse(volume);
            }
            catch
            {
                Volume = volume.GetHashCode();
            }

            await conn.SetVolumeAsync(Volume);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Set volume to {volume}"));
        }
        [SlashCommand("language", "Changes the RTS language, you can set it through its name or short name (ex: Spanish or es-ES)")]
        public async Task Language(InteractionContext ctx, [Option("language", "what to set the language to, leave empty to get current language")] string language = null)
        {
            if (language == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Current language: `{Settings.LanguageDictionary.FirstOrDefault(x => x.Value == Settings.CurrentLanguage).Key} - {Settings.CurrentLanguage}`"));
                return;
            }

            if (Settings.LanguageDictionary.ContainsKey(language.ToLower()))
            {
                Settings.CurrentLanguage = Settings.LanguageDictionary[language.ToLower()];
                File.WriteAllText($"{Environment.CurrentDirectory}\\CurrentLanguage.Ren", Settings.LanguageDictionary[language.ToLower()]);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Set language to `{language} - {Settings.LanguageDictionary[language.ToLower()]}`"));
            }
            else if (Settings.LanguageDictionary.ContainsValue(language))
            {
                Settings.CurrentLanguage = language;
                File.WriteAllText($"{Environment.CurrentDirectory}\\CurrentLanguage.Ren", language);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Set language to `{Settings.LanguageDictionary.FirstOrDefault(x => x.Value == language).Key} - {language}`"));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"The language `{language}` does not exist\nLanguage list: <https://cloud.google.com/text-to-speech/docs/voices>"));
            }
        }
    }
}