using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Newtonsoft.Json;

namespace DiscordBot.Commands.Embed
{
    public class ModuleEmbeds : BaseCommandModule
    {
        [Command("randomaddlink")]
        public async Task RandomAddCommand(CommandContext ctx, [RemainingText] string link)
        {

            if (!link.EndsWith(".gif") && !link.EndsWith(".png"))
            {
                using (var stream = new FileStream("files/images/example-random.png", FileMode.Open, FileAccess.Read))
                {
                    var msg = await new DiscordMessageBuilder()
                        .WithReply(ctx.Message.Id, true)
                        .WithContent("O link precisa terminar com .gif ou .png")
                        .WithFile("example-random.png", stream)
                        .SendAsync(ctx.Channel);
                }

            }
            else
            {
                string[] linksArr = File.ReadAllLines("files/links.txt");

                List<string> linksList = linksArr.ToList();

                if ( linksList.Contains(link) )
                {
                    var msg = await new DiscordMessageBuilder()
                        .WithReply(ctx.Message.Id, true)
                        .WithContent("O link já existe")
                        .SendAsync(ctx.Channel);
                        
                    return;
                }

                linksList.Add(link);

                try
                {
                    await using (var write = new StreamWriter("files/links.txt"))
                    {
                        foreach (var item in linksList)
                        {
                            write.WriteLine(item);
                        }

                        await write.DisposeAsync();
                    }
                    
                    var msg = await new DiscordMessageBuilder()
                        .WithReply(ctx.Message.Id, true)
                        .WithContent("Link adicionado com sucesso!")
                        .SendAsync(ctx.Channel);
                }
                catch (Exception)
                {
                    var msg = await new DiscordMessageBuilder()
                        .WithReply(ctx.Message.Id, true)
                        .WithContent("Falha ao salvar o link.")
                        .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("random")]
        public async Task RandomCommand(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            
            string dir = $"files/data/guilds/{ctx.Guild.Id}";
            string jsonfile = $"{dir}/random.json";

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (!File.Exists(jsonfile))
            {
                var classDate = new RandomClass(TimeSpan.Zero, TimeSpan.Zero);

                string output = JsonConvert.SerializeObject(classDate);

                using (StreamWriter sw = new(jsonfile) )
                {
                    await sw.WriteLineAsync(output);
                }

            }

            async void RandomSelect(RandomClass jsonRandom)
            {
                jsonRandom.Today = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                jsonRandom.Date = new TimeSpan(DateTime.Now.Day + 1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                var allUsers = await ctx.Guild.GetAllMembersAsync();
                
                var random = new Random();

                var users = allUsers.Where(x => !x.IsBot);

                var user = users.ElementAt(random.Next(0, users.Count()));
                jsonRandom.UserId = user.Id;

                string output = JsonConvert.SerializeObject(jsonRandom);

                using (StreamWriter sw = new(jsonfile) )
                {
                    await sw.WriteLineAsync(output);
                }

                var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                {
                    Height = 64,
                    Width = 64,
                    Url = user.AvatarUrl
                };

                string[] linksArr = File.ReadAllLines("files/links.txt");

                int randomGif = random.Next( 0, linksArr.Length );
                string urlGif = linksArr.ElementAt( randomGif );

                var footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"-  Tempo restante: 1 dia",
                    IconUrl = "https://i.imgur.com/82HZ341.png"
                };

                var embed = new DiscordEmbedBuilder()
                {
                    Title = "Sus amogus",
                    Description = $"{user.DisplayName} é muito sus, tenha muito cuidado com essa pessoa.",
                    Thumbnail = thumbnail,
                    ImageUrl = urlGif,
                    Color = DiscordColor.Red,
                    Footer = footer,
                };

                var msg = await new DiscordMessageBuilder()
                    .WithEmbed(embed)
                    .SendAsync(ctx.Channel);
            }

            string jsonString = string.Empty;
            
            using (var fs = File.OpenRead(jsonfile))
            using (var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false) ) )
            {
                jsonString = await sr.ReadToEndAsync();
            }
            
            var storedData = JsonConvert.DeserializeObject<RandomClass>(jsonString);

            storedData.Today = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (storedData.Date == TimeSpan.Zero)
            {
                RandomSelect(storedData);
            }
            else if (storedData.Today.CompareTo(storedData.Date) >= 0)
            {
                RandomSelect(storedData);
            }
            else if (storedData.Today.CompareTo(storedData.Date) < 0)
            {
                var AllUsers = await ctx.Guild.GetAllMembersAsync();

                var user = AllUsers.Where(x => x.Id == storedData.UserId).First();

                var random = new Random();

                var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                {
                    Height = 64,
                    Width = 64,
                    Url = user.AvatarUrl
                };

                var footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"-  Tempo restante: {storedData.Date.Subtract(storedData.Today)}",
                    IconUrl = "https://i.imgur.com/82HZ341.png"
                };

                string[] linksArr = File.ReadAllLines("files/links.txt");

                int randomGif = random.Next( 0, linksArr.Length );
                string urlGif = linksArr.ElementAt( randomGif );

                var embed = new DiscordEmbedBuilder()
                {
                    Title = "Sus amogus",
                    Description = $"{user.DisplayName} é muito sus, tenha muito cuidado com essa pessoa.",
                    Thumbnail = thumbnail,
                    ImageUrl = urlGif,
                    Color = DiscordColor.Red,
                    Footer = footer
                };

                var msg = await new DiscordMessageBuilder()
                    .WithEmbed(embed)
                    .SendAsync(ctx.Channel);
            }
        }

        [Command("members")]
        public async Task MembersCommand(CommandContext ctx)
        {
            var getMembers = await ctx.Guild.GetAllMembersAsync();
            
            string message = string.Empty;

            foreach (var user in getMembers)
            {
                if (message == string.Empty)
                {
                    message = user.DisplayName;
                }
                else
                {
                    message += $", {user.DisplayName}";
                }
            }   
            
            var embed = new DiscordEmbedBuilder()
            {
                Title = "All users",
                Color = DiscordColor.Green,
                Description = $"Todos os users no servidor: {message}"

            };

            var msg = await new DiscordMessageBuilder()
                .WithEmbed(embed)
                .WithReply(ctx.Message.Id, true)
                .SendAsync(ctx.Channel);

            // await ctx.RespondAsync($"All members: {message}");
            
        }
    }
}