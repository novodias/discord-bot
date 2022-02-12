using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus;
using DiscordBot.Interactivity.Games;

namespace DiscordBot.Commands
{
    public class Module : BaseCommandModule
    {
        [Command("amogus")]
        public async Task AmogusCommand(CommandContext ctx, DiscordMember member)
        {
            await ctx.TriggerTypingAsync();
            using (var fs = new FileStream("files/images/amogus.jpeg", FileMode.Open, FileAccess.Read))
            {
                var msg = await new DiscordMessageBuilder()
                    .WithContent($"vc e sus {member.Mention}")
                    .WithFiles(new Dictionary<string, Stream>() { {"files/images/amogus.jpeg", fs } })
                    .SendAsync(ctx.Channel);
            }
        }

        [Command("amogus")]
        public async Task AmogusCommand(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var AllMembers = await ctx.Guild.GetAllMembersAsync();

            var random = new Random();
            var Members = AllMembers.Where(x => !x.IsBot);
            int selectedMember = random.Next(0, Members.Count());
            var member = Members.ElementAt(selectedMember);

            using (var image = Image.Load("files/images/amogus.jpeg"))
            {
                image.Mutate( x => x.Resize(512, 512) );

                using (var userUrl = new HttpClient())
                {
                    using (Stream stream = await userUrl.GetStreamAsync(member.GetAvatarUrl(DSharpPlus.ImageFormat.Png, 64)))
                    {
                        var avatar = Image.Load(stream);

                        var fontf = SixLabors.Fonts.SystemFonts.Find("ubuntu");
                        var font = new SixLabors.Fonts.Font(fontf, 32f, SixLabors.Fonts.FontStyle.Bold);

                        image.Mutate( x => x.DrawImage( avatar, new Point(240, 164), 1f ) );
                        avatar.Dispose();

                        var options = new DrawingOptions() 
                        {
                            TextOptions = new TextOptions()
                            {
                                VerticalAlignment = SixLabors.Fonts.VerticalAlignment.Bottom,
                                WrapTextWidth = image.Width,
                                HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Center,
                            }

                        };

                        image.Mutate( y => y.DrawText(
                            options,
                            String.Format("{0} é sus", member.DisplayName).ToUpper(), 
                            font, 
                            Color.White, 
                            new PointF(0, 384f) ));

                    }
                } 

                using (Stream imageStream = new MemoryStream())
                {
                    image.SaveAsPng(imageStream);
                    image.Dispose();
                    imageStream.Position = 0;

                    var msg = await new DiscordMessageBuilder()
                        .WithContent($"vc e sus {member.Mention}")
                        .WithFiles(new Dictionary<string, Stream>() { {"amogusUser.png", imageStream } })
                        .SendAsync(ctx.Channel);

                    imageStream.Dispose();
                }
            }
        }

        private Tictactoe? Tictactoe;

        [Command("tictactoe")]
        public async Task TictactoeCommand(CommandContext ctx, DiscordUser user)
        {
            await ctx.TriggerTypingAsync();

            if ( Tictactoe is null ) { Tictactoe = new(ctx.Client); }

            var users = new List<DiscordUser>() { ctx.User, user };

            try
            {
                await Tictactoe.InitializeTask(users, ctx.Channel);
            }
            catch (System.Exception ex)
            {
                await ctx.RespondAsync(ex.Message + ex.StackTrace);
            }
        }

        // [Command("nice")]
        // public async Task NiceCommand(CommandContext ctx, DiscordMember user)
        // {
        //     var date = new TimeSpan(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).GetHashCode();
        //     var id = user.Id.GetHashCode();

        //     date = Math.Abs(date) / 10000;
        //     id = Math.Abs(id) / 10000;
            
        //     var rnd = new Random(date + id);

        //     int value = rnd.Next(10, 101);
        //     value = Convert.ToByte(Math.Round(Convert.ToDouble(value /= 10)));

        //     var m = new DiscordMessageBuilder()
        //         .WithContent($"date(hashcode) e id dividos por 10000\ndate: {date} / id: {id} / value: {value}");

        //     await m.SendAsync(ctx.Channel);
        // }

        // [Command("buttons")]
        // public async Task ButtonsCommand(CommandContext ctx)
        // {
        //     var buttonlst = new List<DiscordButtonComponent>() 
        //     {
        //         new DiscordButtonComponent(ButtonStyle.Primary, "01", "Primeiro", false ),
        //         new DiscordButtonComponent(ButtonStyle.Secondary, "02", "Segundo", false ),
        //         new DiscordButtonComponent(ButtonStyle.Success, "03", "Terceiro", false ),
        //         new DiscordButtonComponent(ButtonStyle.Danger, "04", "Quarto", false )
        //     };

        //     var builder = new DiscordMessageBuilder()
        //         .WithContent("Botões")
        //         .AddComponents(buttonlst);

        //     await builder.SendAsync(ctx.Channel);
        // }
    }
}