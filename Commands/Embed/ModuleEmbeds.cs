using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DiscordBot.Commands.Embed.RandomFiles;

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
            
            var members = await ctx.Guild.GetAllMembersAsync();
            
            var storedData = await RandomCommandSel.RandomFile(ctx.Guild.Id);

            storedData.Today = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (storedData.Date == TimeSpan.Zero)
            {
                var msg = await RandomCommandSel.RandomSelect(ctx.Guild.Id, members, storedData);
                await msg.SendAsync(ctx.Channel);
            }
            else if (storedData.Today.CompareTo(storedData.Date) >= 0)
            {
                var msg = await RandomCommandSel.RandomSelect(ctx.Guild.Id, members, storedData);
                await msg.SendAsync(ctx.Channel);
            }
            else if (storedData.Today.CompareTo(storedData.Date) < 0)
            {
                var user = members.Where(x => x.Id == storedData.UserId).First();

                var random = new Random();

                var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                {
                    Height = 64,
                    Width = 64,
                    Url = user.GetAvatarUrl(DSharpPlus.ImageFormat.Auto, 64)
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
        }
    }
}