namespace RenBot;

using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using Lavalink4NET;
using Lavalink4NET.Filters;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AudioCommands : ApplicationCommandModule
{
    private readonly IAudioService _audioService;
    private readonly HttpClient _httpClient;
    private readonly CleverBot _cleverBot;

    public AudioCommands(IAudioService audioService, HttpClient httpClient, CleverBot cleverBot)
    {
        ArgumentNullException.ThrowIfNull(audioService);

        _audioService = audioService;
        _httpClient = httpClient;
        _cleverBot = cleverBot;
    }

    // [SlashCommand("template", description: "meow")]
    // public async Task Temp(InteractionContext ctx, [Option("query", "Track to play")] string query)
    // {
    //     await ctx.DeferAsync().ConfigureAwait(false);

    //     var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

    //     if (player is null)
    //     {
    //         return;
    //     }

    //     // Check if the player is playing
    //     if (player.CurrentTrack is null)
    //     {
    //         // If the player is not playing, we send an error message to the user
    //         await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Nothing playing!"));
    //         return;
    //     }

    //     // Stop the player and send a message to the user
    //     await player.StopAsync().ConfigureAwait(false);
    //     await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{ctx.Member.DisplayName} stopped the player!"));
    // }
    public enum SearchMode
    {
        [ChoiceName("AppleMusic")]
        AppleMusic,
        [ChoiceName("Deezer")]
        Deezer,
        [ChoiceName("None")]
        None,
        [ChoiceName("Soundcloud")]
        Soundcloud,
        [ChoiceName("Spotify")]
        Spotify,
        [ChoiceName("YandexMusic")]
        YandexMusic,
        [ChoiceName("YouTube")]
        YouTube,
        [ChoiceName("YouTubeMusic")]
        YouTubeMusic
    }
    [SlashCommand("play", description: "Plays music")]
    public async Task Play(InteractionContext ctx, [Option("query", "Track to play")] string query, [Option("queue", "Whether to queue the track or not (default: true)")] bool queue = true, [Option("loops", "How many times to queue the track (default: 1)")] long loops = 1, [Option("searchmode", "What type of search mode should be used (default: youtube)")] SearchMode searchMode = SearchMode.YouTube)
    {
        await ctx.DeferAsync().ConfigureAwait(false);

        var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        TrackSearchMode mode = TrackSearchMode.None;

        switch (searchMode)
        {
            case SearchMode.AppleMusic:
                mode = TrackSearchMode.AppleMusic;
                break;
            case SearchMode.Deezer:
                mode = TrackSearchMode.Deezer;
                break;
            case SearchMode.None:
                mode = TrackSearchMode.None;
                break;
            case SearchMode.Soundcloud:
                mode = TrackSearchMode.SoundCloud;
                break;
            case SearchMode.Spotify:
                mode = TrackSearchMode.Spotify;
                break;
            case SearchMode.YandexMusic:
                mode = TrackSearchMode.YandexMusic;
                break;
            case SearchMode.YouTube:
                mode = TrackSearchMode.YouTube;
                break;
            case SearchMode.YouTubeMusic:
                mode = TrackSearchMode.YouTubeMusic;
                break;
        }

        var track = await _audioService.Tracks
            .LoadTrackAsync(query, mode)
            .ConfigureAwait(false);

        if (loops < 1)
        {
            var errorResponse = new DiscordFollowupMessageBuilder()
                .WithContent("Stop it")
                .AsEphemeral();

            await ctx
                .FollowUpAsync(errorResponse)
                .ConfigureAwait(false);

            return;
        }

        if (track is null)
        {
            var errorResponse = new DiscordFollowupMessageBuilder()
                .WithContent(">~< No results.")
                .AsEphemeral();

            await ctx
                .FollowUpAsync(errorResponse)
                .ConfigureAwait(false);

            return;
        }

        for (long i = 0; i < loops; i++)
        {
            var position = await player
                .PlayAsync(track, queue)
                .ConfigureAwait(false);

            if (position is 0 && i == 0)
            {
                await ctx
                    .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Now Playing: {track.Uri} {((loops > 1) ? $"(Queued {loops} times)" : string.Empty)}"))
                    .ConfigureAwait(false);
            }
            else if (i == 0)
            {
                await ctx
                    .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{ctx.Member.DisplayName} Added to queue: {track.Uri} {((loops > 1) ? $"(Queued {loops} times)" : string.Empty)}"))
                    .ConfigureAwait(false);
            }
        }
    }
    [SlashCommand("stop", description: "Stops music")]
    public async Task Stop(InteractionContext ctx)
    {
        await ctx.DeferAsync().ConfigureAwait(false);

        var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        // Check if the player is playing
        if (player.CurrentTrack is null)
        {
            // If the player is not playing, we send an error message to the user
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Nothing playing!"));
            return;
        }

        // Stop the player and send a message to the user
        await player.StopAsync().ConfigureAwait(false);
        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{ctx.Member.DisplayName} stopped the player!"));
    }
    [SlashCommand("pause", description: "Pauses music")]
    public async Task Pause(InteractionContext ctx)
    {
        await ctx.DeferAsync().ConfigureAwait(false);

        var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        // Check if the player is playing
        if (player.CurrentTrack is null)
        {
            // If the player is not playing, we send an error message to the user
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Nothing playing!"));
            return;
        }

        // Stop the player and send a message to the user
        await player.PauseAsync().ConfigureAwait(false);
        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{ctx.Member.DisplayName} paused the player!"));
    }
    [SlashCommand("resume", description: "Resumes music")]
    public async Task Resume(InteractionContext ctx)
    {
        await ctx.DeferAsync().ConfigureAwait(false);

        var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        // Check if the player is playing
        if (player.CurrentTrack is null)
        {
            // If the player is not playing, we send an error message to the user
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Nothing playing!"));
            return;
        }

        // Stop the player and send a message to the user
        await player.ResumeAsync().ConfigureAwait(false);
        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{ctx.Member.DisplayName} resumed the player!"));
    }
    [SlashCommand("volume", description: "Sets the player volume (0 - 1000%)")]
    public async Task Volume(InteractionContext ctx, [Option("volume", "What to set the volume to (0 - 1000%)")] double volume)
    {
        await ctx.DeferAsync().ConfigureAwait(false);

        if (volume is > 1000 or < 0)
        {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Volume out of range: 0% - 1000%!"));
            return;
        }

        var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        // Stop the player and send a message to the user
        await player.SetVolumeAsync((float)volume / 100f).ConfigureAwait(false);
        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{ctx.Member.DisplayName} set volume to {volume}%"));
    }
    [SlashCommand("position", description: "Shows the track position")]
    public async Task Position(InteractionContext ctx)
    {
        await ctx.DeferAsync().ConfigureAwait(false);

        var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        if (player.CurrentTrack is null)
        {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Nothing playing!"));
            return;
        }

        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Position: {player.Position?.Position} / {player.CurrentTrack.Duration}."));
    }
    [SlashCommand("shuffle", description: "Whether to shuffle the queue or not")]
    public async Task Shuffle(InteractionContext ctx, [Option("shuffle", "Whether to shuffle or not")] bool shuffle)
    {
        await ctx.DeferAsync().ConfigureAwait(false);

        var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        if (player.CurrentTrack is null)
        {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Nothing playing!"));
            return;
        }

        player.Shuffle = shuffle;

        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{ctx.Member.DisplayName} changed shuffle to: {shuffle}"));
    }
    [SlashCommand("skip", description: "Skips the current track")]
    public async Task Skip(InteractionContext ctx)
    {
        await ctx.DeferAsync().ConfigureAwait(false);

        var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        if (player.CurrentTrack is null)
        {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Nothing playing!"));
            return;
        }

        await player.SkipAsync().ConfigureAwait(false);

        var track = player.CurrentTrack;

        if (track is not null)
        {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{ctx.Member.DisplayName} skipped. Now playing: {track.Uri}"));
        }
        else
        {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{ctx.Member.DisplayName} skipped. Stopped playing because the queue is now empty."));
        }
    }
    [SlashCommand("playlist", description: "Plays music from playlist")]
    public async Task Playlist(InteractionContext ctx, [Option("query", "Track to play")] string query)
    {
        await ctx.DeferAsync().ConfigureAwait(false);

        var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        var tracks = await _audioService.Tracks
            .LoadTracksAsync(query, TrackSearchMode.None)
            .ConfigureAwait(false);

        await player.Queue
            .AddRangeAsync(tracks.Tracks.Select(x => new TrackQueueItem(x)).ToArray())
            .ConfigureAwait(false);

        if (player.CurrentItem is null)
        {
            await player
                .SkipAsync()
                .ConfigureAwait(false);
        }

        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{ctx.Member.DisplayName} played playlist: {query}"));
    }
    [SlashCommand("rts", description: "allows you to talk in vc using tts through ren bot, or be annoying")]
    public async Task Temp(InteractionContext ctx, [Option("text", "What to say")] string text)
    {
        await ctx.DeferAsync().ConfigureAwait(false);

        var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        if (text.Length > 200)
        {
            var errorResponse = new DiscordFollowupMessageBuilder()
                .WithContent("text can't be more than 200 characters")
                .AsEphemeral();

            await ctx
                .FollowUpAsync(errorResponse)
                .ConfigureAwait(false);

            return;
        }

        JObject json = JObject.Parse(File.ReadAllText("../config.json"));

        string LanguageFromJson = json["servers"][ctx.Guild.Id.ToString()]["current_language"].ToString();

        var track = await _audioService.Tracks
            .LoadTrackAsync($"https://translate.google.com/translate_tts?ie=UTF-8&q={Uri.EscapeDataString(text)}&tl={LanguageFromJson}&total=1&idx=0&textlen={text.Length}&client=tw-ob&prev=input&ttsspeed=1", TrackSearchMode.None)
            .ConfigureAwait(false);

        if (track is null)
        {
            var errorResponse = new DiscordFollowupMessageBuilder()
                .WithContent(">~< No results.")
                .AsEphemeral();

            await ctx
                .FollowUpAsync(errorResponse)
                .ConfigureAwait(false);

            return;
        }

        var position = await player
            .PlayAsync(track)
            .ConfigureAwait(false);

        if (position is 0)
        {
            await ctx
                .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"sent rts: {text}"))
                .ConfigureAwait(false);
        }
        else
        {
            await ctx
                .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"added rts to queue: {text}"))
                .ConfigureAwait(false);
        }

        Console.WriteLine($"{ctx.Member.DisplayName} sent rts: {text}");
    }
    [SlashCommand("language", "Changes the RTS language, you can set it through its name or short name (ex: Spanish or es-ES)")]
    public async Task Language(InteractionContext ctx, [Option("language", "what to set the language to, leave empty to get current language")] string language = null)
    {
        JObject json = JObject.Parse(File.ReadAllText("../config.json"));

        string LanguageFromJson = json["servers"][ctx.Guild.Id.ToString()]["current_language"].ToString();

        Dictionary<string, string> LanguageDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("../language_dictionary.json"));

        if (language == null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Current language: `{LanguageDictionary.FirstOrDefault(x => x.Value.ToString() == LanguageFromJson).Key} - {LanguageFromJson}`"));
            return;
        }

        if (LanguageDictionary.ContainsKey(language.ToLower()))
        {
            json["servers"][ctx.Guild.Id.ToString()]["current_language"] = LanguageDictionary[language.ToLower()];

            File.WriteAllText("../config.json", json.ToString());

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Set language to `{language} - {LanguageDictionary[language.ToLower()]}`"));
        }
        else if (LanguageDictionary.ContainsValue(language))
        {
            json["servers"][ctx.Guild.Id.ToString()]["current_language"] = language;

            File.WriteAllText("../config.json", json.ToString());

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Set language to `{LanguageDictionary.FirstOrDefault(x => x.Value == language).Key} - {language}`"));
        }
        else
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"The language `{language}` does not exist\nLanguage list: <https://pastebin.com/ysiKnzpL>"));
        }
    }
    [SlashCommand("playrandom", "Plays a random youtube video")]
    private async Task PlayRandom(InteractionContext ctx)
    {
        await ctx.DeferAsync().ConfigureAwait(false);

        var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        string query = string.Join(' ', JsonConvert.DeserializeObject<string[]>(await _httpClient.GetStringAsync("https://random-word-api.herokuapp.com/word?number=2")));

        var track = await _audioService.Tracks
            .LoadTrackAsync(query, TrackSearchMode.YouTube)
            .ConfigureAwait(false);

        if (track is null)
        {
            var errorResponse = new DiscordFollowupMessageBuilder()
                .WithContent(">~< No results.")
                .AsEphemeral();

            await ctx
                .FollowUpAsync(errorResponse)
                .ConfigureAwait(false);

            return;
        }

        var position = await player
            .PlayAsync(track)
            .ConfigureAwait(false);

        if (position is 0)
        {
            await ctx
                .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Now Playing: {track.Uri}"))
                .ConfigureAwait(false);
        }
        else
        {
            await ctx
                .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{ctx.Member.DisplayName} Added to queue: {track.Uri}"))
                .ConfigureAwait(false);
        }
    }
    [SlashCommand("vcwisdom", description: "Ren Bot joins vc and gives his wisdom through RTS")]
    public async Task VCWisdom(InteractionContext ctx)
    {
        await ctx.DeferAsync().ConfigureAwait(false);

        var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        string words = string.Join(' ', JsonConvert.DeserializeObject<string[]>(await _httpClient.GetStringAsync("https://random-word-api.herokuapp.com/word?number=3")));

        string text = await _cleverBot.SendCleverbotMessage(words);

        JObject json = JObject.Parse(File.ReadAllText("../config.json"));

        string LanguageFromJson = json["servers"][ctx.Guild.Id.ToString()]["current_language"].ToString();

        var track = await _audioService.Tracks
            .LoadTrackAsync($"https://translate.google.com/translate_tts?ie=UTF-8&q={Uri.EscapeDataString(text)}&tl={LanguageFromJson}&total=1&idx=0&textlen={text.Length}&client=tw-ob&prev=input&ttsspeed=1", TrackSearchMode.None)
            .ConfigureAwait(false);

        if (track is null)
        {
            var errorResponse = new DiscordFollowupMessageBuilder()
                .WithContent(">~< No results.")
                .AsEphemeral();

            await ctx
                .FollowUpAsync(errorResponse)
                .ConfigureAwait(false);

            return;
        }

        var position = await player
            .PlayAsync(track)
            .ConfigureAwait(false);

        if (position is 0)
        {
            await ctx
                .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"sent wisdom: {text}"))
                .ConfigureAwait(false);
        }
        else
        {
            await ctx
                .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"added wisdom to queue: {text}"))
                .ConfigureAwait(false);
        }

        Console.WriteLine($"{ctx.Member.DisplayName} sent wisdom: {text}");
    }
    private async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(InteractionContext interactionContext, bool connectToVoiceChannel = true)
    {
        ArgumentNullException.ThrowIfNull(interactionContext);

        var retrieveOptions = new PlayerRetrieveOptions(
            ChannelBehavior: connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

        var playerOptions = new QueuedLavalinkPlayerOptions { HistoryCapacity = 10000 };

        var result = await _audioService.Players
            .RetrieveAsync(interactionContext.Guild.Id, interactionContext.Member?.VoiceState.Channel.Id, playerFactory: PlayerFactory.Queued, Options.Create(playerOptions), retrieveOptions)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Status switch
            {
                PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
                PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
                _ => "Unknown error.",
            };

            var errorResponse = new DiscordFollowupMessageBuilder()
                .WithContent(errorMessage)
                .AsEphemeral();

            await interactionContext
                .FollowUpAsync(errorResponse)
                .ConfigureAwait(false);

            return null;
        }

        return result.Player;
    }
}
