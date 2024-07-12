using System;
using System.Data;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RenBot;

var builder = new HostApplicationBuilder(args);

// DSharpPlus
builder.Services.AddHostedService<ApplicationHost>();
builder.Services.AddSingleton<DiscordClient>();
builder.Services.AddSingleton(new DiscordConfiguration
{
    TokenType = TokenType.Bot,
    Token = File.ReadAllText("../.token"),
    MinimumLogLevel = LogLevel.Debug,
    AlwaysCacheMembers = true,
    MessageCacheSize = 1024,
    Intents = DiscordIntents.All
});
builder.Services.AddSingleton<CleverBot>();

builder.Services.AddSingleton<GoogleTranslate>();

builder.Services.AddSingleton<Markov>();

builder.Services.AddHttpClient();

builder.Services.AddLavalink();

builder.Services.AddMemoryCache();

// Logging
builder.Services.AddLogging(s => s.AddConsole().SetMinimumLevel(LogLevel.Trace));

// We need to be able to change the lavalink URL in Docker
builder.Services.ConfigureLavalink(config =>
{
    config.BaseAddress = new Uri(Environment.GetEnvironmentVariable("LAVALINK_URL") ?? "http://localhost:2333");
    config.Passphrase = Environment.GetEnvironmentVariable("LAVALINK_PASS") ?? config.Passphrase;
});

builder.Build().Run();

file sealed class ApplicationHost : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DiscordClient _discordClient;
    private readonly CleverBot _cleverBot;
    private readonly HttpClient _httpClient;
    private readonly Markov _markov;
    private List<string> CleverBotContext = new List<string>();

    public ApplicationHost(IServiceProvider serviceProvider, DiscordClient discordClient, CleverBot cleverBot, HttpClient httpClient, Markov markov)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(discordClient);

        _serviceProvider = serviceProvider;
        _discordClient = discordClient;
        _cleverBot = cleverBot;
        _httpClient = httpClient;
        _markov = markov;
    }
    private async Task UpdateStatusAsync()
    {
        DiscordActivity activity = new DiscordActivity();

        int type = RandomNumberGenerator.GetInt32(0, 6);
        if (type == 4) { type = 5; }
        activity.ActivityType = (ActivityType)type;

        var json = JArray.Parse(File.ReadAllText("../statuses.json")).ToList();

        activity.Name = json[RandomNumberGenerator.GetInt32(0, json.Count())].ToString().Replace("${Mem}", RandomMemberName());

        await _discordClient.UpdateStatusAsync(activity);
    }
    private string RandomMemberName()
    {
        var members = _discordClient.Guilds.ElementAt(RandomNumberGenerator.GetInt32(0, _discordClient.Guilds.Count())).Value.Members;
        return members.ElementAt(RandomNumberGenerator.GetInt32(0, members.Count)).Value.DisplayName;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!File.Exists("../config.json"))
        {
            File.Create("../config.json");
            File.WriteAllText("../config.json", JObject.Parse("""{"startup": "0", "word": "", "servers": {}, }""").ToString());
        }
        else if (File.ReadAllText("../config.json") == String.Empty)
        {
            File.WriteAllText("../config.json", JObject.Parse("""{"startup": "0", "word": "", "servers": {}, }""").ToString());
        }

        if (!Directory.Exists("./tmp/"))
        {
            Directory.CreateDirectory("./tmp/");
        }

        JObject json = JObject.Parse(File.ReadAllText("../config.json"));

        var slash = _discordClient.UseSlashCommands(new SlashCommandsConfiguration { Services = _serviceProvider });

        slash.RegisterCommands<BasicCommands>();
        slash.RegisterCommands<AudioCommands>();
        slash.RegisterCommands<BankCommands>();
        slash.RegisterCommands<ImageCommands>();

        _discordClient.MessageDeleted += async (s, e) =>
            {
                JObject message = JObject.Parse(File.ReadAllText("../config.json"));
                message["servers"][e.Guild.Id.ToString()]["deleted_message"]["username"] = e.Message.Author.Username;

                message["servers"][e.Guild.Id.ToString()]["deleted_message"]["avatar"] = e.Message.Author.AvatarUrl;

                message["servers"][e.Guild.Id.ToString()]["deleted_message"]["content"] = e.Message.Content;

                JArray urls = (JArray)message["servers"][e.Guild.Id.ToString()]["deleted_message"]["urls"];
                urls.Clear();
                JArray filenames = (JArray)message["servers"][e.Guild.Id.ToString()]["deleted_message"]["filenames"];
                filenames.Clear();

                foreach (var attachment in e.Message.Attachments)
                {
                    urls.Add(attachment.Url);
                    filenames.Add(attachment.FileName);
                }

                message["servers"][e.Guild.Id.ToString()]["deleted_message"]["timestamp"] = e.Message.Timestamp.ToString();

                File.WriteAllText("../config.json", message.ToString());
            };

        _discordClient.MessageCreated += async (s, e) =>
        {
            if (e.Message.Author.Id != 798285857340522496)
            {
                if (e.Guild.Id == 899811497121828914 || e.Guild.Id == 1225173980173045861)
                {
                    if (e.Channel.Id == 1234259364479504404)
                    {
                        File.AppendAllText("./dataset.txt", e.Message.Content + "\n");
                    }
                }
                else
                {
                    File.AppendAllText("./dataset.txt", e.Message.Content + "\n");
                }

                JObject json = JObject.Parse(File.ReadAllText("../config.json"));

                if ((bool)json["servers"][e.Guild.Id.ToString()]["talky"])
                {
                    if (json["servers"][e.Guild.Id.ToString()]["talky_whitelist"].ToObject<string[]>().Contains(e.Channel.Id.ToString()))
                    {
                        if (!e.Message.Author.IsBot)
                        {
                            string CleverResponse = await _cleverBot.SendCleverbotMessage(e.Message.Content, CleverBotContext.ToArray());
                            CleverBotContext.Add(e.Message.Content);
                            CleverBotContext.Add(CleverResponse);
                            await e.Message.RespondAsync(CleverResponse);
                            if (CleverBotContext.Count > 100)
                            {
                                CleverBotContext.RemoveAt(0);
                                CleverBotContext.RemoveAt(0);
                            }
                        }
                    }
                }
            }
        };

        _discordClient.ComponentInteractionCreated += async (s, e) =>
        {
            List<DiscordColor> Rainbow = new List<DiscordColor>() { DiscordColor.Red, DiscordColor.Orange, DiscordColor.Yellow, DiscordColor.Green, DiscordColor.Blue, DiscordColor.Purple, DiscordColor.Magenta };

            JObject wordJson = JObject.Parse(File.ReadAllText("../config.json"));

            if (e.Id == "left_arrow_define")
            {
                int currentpage = int.Parse(new Regex(@"\d+(?=\/)").Match(e.Message.Embeds.First().Footer.Text).Value);

                currentpage--;

                DataSet words = JsonConvert.DeserializeObject<DataSet>(await _httpClient.GetStringAsync($"https://api.urbandictionary.com/v0/{((string.IsNullOrEmpty(wordJson["word"].ToString())) ? "random" : $"define?page=1&term={wordJson["word"].ToString()}")}"));

                DataTable dataTable = words.Tables["list"];

                DataRow result = dataTable.Rows[currentpage - 1];

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Title = result["word"].ToString(),
                    Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = $"Page {currentpage}/{dataTable.Rows.Count}  •  {DateTime.Now.ToString("t")}" },
                    Url = result["permalink"].ToString(),
                    Color = Rainbow[(currentpage - 1) % 7]
                };

                try
                {
                    embed.AddField("Definition", Regex.Replace(result["definition"].ToString(), @"\[(.*?)\]", m => $"{m.Value}(<https://www.urbandictionary.com/define.php?term={HttpUtility.UrlEncode(m.Value.Replace("[", string.Empty).Replace("]", string.Empty))}>)"));

                    embed.AddField("Example", Regex.Replace(result["example"].ToString(), @"\[(.*?)\]", m => $"{m.Value}(<https://www.urbandictionary.com/define.php?term={HttpUtility.UrlEncode(m.Value.Replace("[", string.Empty).Replace("]", string.Empty))}>)"));

                    embed.AddField("Author", result["author"].ToString());

                    embed.AddField("Votes", $"Upvotes: {result["thumbs_up"]}\nDownvotes: {result["thumbs_down"]}");
                }
                catch
                {
                    embed.AddField("Error :(", "Too much text");
                }

                var left = new DiscordButtonComponent(ButtonStyle.Primary, "left_arrow_define", "←", (currentpage <= 1));

                var right = new DiscordButtonComponent(ButtonStyle.Primary, "right_arrow_define", "→", (dataTable.Rows.Count <= currentpage));

                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[]
                {
                    left, right
                }));
            }
            else if (e.Id == "right_arrow_define")
            {
                int currentpage = int.Parse(new Regex(@"\d+(?=\/)").Match(e.Message.Embeds.First().Footer.Text).Value);

                currentpage++;

                DataSet words = JsonConvert.DeserializeObject<DataSet>(await _httpClient.GetStringAsync($"https://api.urbandictionary.com/v0/{((string.IsNullOrEmpty(wordJson["word"].ToString())) ? "random" : $"define?page=1&term={wordJson["word"].ToString()}")}"));

                DataTable dataTable = words.Tables["list"];

                DataRow result = dataTable.Rows[currentpage - 1];

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Title = result["word"].ToString(),
                    Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = $"Page {currentpage}/{dataTable.Rows.Count}  •  {DateTime.Now.ToString("t")}" },
                    Url = result["permalink"].ToString(),
                    Color = Rainbow[(currentpage - 1) % 7]
                };

                try
                {
                    embed.AddField("Definition", Regex.Replace(result["definition"].ToString(), @"\[(.*?)\]", m => $"{m.Value}(<https://www.urbandictionary.com/define.php?term={HttpUtility.UrlEncode(m.Value.Replace("[", string.Empty).Replace("]", string.Empty))}>)"));

                    embed.AddField("Example", Regex.Replace(result["example"].ToString(), @"\[(.*?)\]", m => $"{m.Value}(<https://www.urbandictionary.com/define.php?term={HttpUtility.UrlEncode(m.Value.Replace("[", string.Empty).Replace("]", string.Empty))}>)"));

                    embed.AddField("Author", result["author"].ToString());

                    embed.AddField("Votes", $"Upvotes: {result["thumbs_up"]}\nDownvotes: {result["thumbs_down"]}");
                }
                catch
                {
                    embed.AddField("Error :(", "Too much text");
                }

                var left = new DiscordButtonComponent(ButtonStyle.Primary, "left_arrow_define", "←", (currentpage <= 1));

                var right = new DiscordButtonComponent(ButtonStyle.Primary, "right_arrow_define", "→", (dataTable.Rows.Count <= currentpage));

                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[]
                {
                    left, right
                }));
            }
            else if (e.Id == "generate_new_color")
            {
                string color = String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000));

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Title = color,
                    Color = new DiscordColor(color)
                };

                var newColor = new DiscordButtonComponent(ButtonStyle.Secondary, "generate_new_color", "New", false);

                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[] { newColor }));
            }
            else if (e.Id == "make_new_bank_account")
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Title = e.User.Username,
                    Description = $"Created an account!",
                    Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                    Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = e.User.AvatarUrl }
                };

                JObject user = JObject.Parse(File.ReadAllText("../config.json"));

                DateTime currentTime = DateTime.UtcNow;
                long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
                user["servers"][e.Guild.Id.ToString()]["bank"][e.User.Id.ToString()] = JObject.Parse("""{"money": "0.00", "safe": "0.00", "steal_cooldown": "0", "safe_cooldown": "0", "stash": "time"}""".Replace("time", $"{unixTime.ToString()}"));

                File.WriteAllText("../config.json", user.ToString());

                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
        };

        _discordClient.MessageReactionAdded += async (s, e) =>
        {
            try
            {
                if (e.Guild.Id == 1064410879124320286)
                {
                    if (e.Emoji.Id == 1165671822470168607 && e.Message.Reactions.Where(x => x.Emoji.Id == 1165671822470168607).Select(x => x).First().Count <= 1)
                    {
                        Uri uriResult;

                        if (Uri.TryCreate(e.Message.Content, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                        {
                            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                            {
                                Title = e.Message.Author.Username,
                                Description = e.Message.Content,
                                ImageUrl = (e.Message.Attachments.Count > 0) ? e.Message.Attachments[0].Url : e.Message.Content,
                                Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = e.Message.Author.AvatarUrl, Text = $"Sent at {e.Message.Timestamp}" }
                            };

                            if (e.Message.Attachments.Count > 0)
                            {
                                foreach (var attachment in e.Message.Attachments)
                                {
                                    embed.AddField(attachment.FileName, attachment.Url, true);
                                }
                            }

                            await s.SendMessageAsync(await _discordClient.GetChannelAsync(1166583996629667850), embed);
                            return;
                        }
                        else
                        {
                            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                            {
                                Title = e.Message.Author.Username,
                                Description = e.Message.Content,
                                ImageUrl = (e.Message.Attachments.Count > 0) ? e.Message.Attachments[0].Url : string.Empty,
                                Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = e.Message.Author.AvatarUrl, Text = $"Sent at {e.Message.Timestamp}" }
                            };

                            if (e.Message.Attachments.Count > 0)
                            {
                                foreach (var attachment in e.Message.Attachments)
                                {
                                    embed.AddField(attachment.FileName, attachment.Url, true);
                                }
                            }

                            await s.SendMessageAsync(await _discordClient.GetChannelAsync(1166583996629667850), embed);
                            return;
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Nuh Uh (Message not in cache [Fuck you])");
            }
        };

        // connect to discord gateway and initialize node connection
        await _discordClient
            .ConnectAsync()
            .ConfigureAwait(false);

        var readyTaskCompletionSource = new TaskCompletionSource();

        Task SetResult(DiscordClient client, ReadyEventArgs eventArgs)
        {
            readyTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        }

        _discordClient.Ready += SetResult;
        await readyTaskCompletionSource.Task.ConfigureAwait(false);
        _discordClient.Ready -= SetResult;

        DateTime currentTime = DateTime.UtcNow;

        json["startup"] = ((DateTimeOffset)currentTime).ToUnixTimeSeconds().ToString();

        foreach (var guild in _discordClient.Guilds)
        {
            Console.WriteLine($"{guild.Key} | {guild.Value.Name}");

            if (json["servers"][guild.Key.ToString()] == null)
            {
                json["servers"][guild.Key.ToString()] = JObject.Parse("""{"bank": {},"talky": false,"talky_whitelist": [],"current_language": "en-AU", "deleted_message": {"username": "","avatar": "","content": "","urls": [], "filenames": [], "timestamp": ""}}""");
            }
        }

        File.WriteAllText("../config.json", json.ToString());

        await Task.Delay(1000, stoppingToken);

        await UpdateStatusAsync();
        System.Timers.Timer timer = new System.Timers.Timer(TimeSpan.FromMinutes(1));
        timer.Elapsed += async (s, e) => await UpdateStatusAsync();
        timer.Start();

        await Task
            .Delay(Timeout.InfiniteTimeSpan, stoppingToken)
            .ConfigureAwait(false);
    }
}
