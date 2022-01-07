using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordBot.Commands
{
    public class Module : BaseCommandModule
    {
        [Command("amogus")]
        public async Task AmogusCommand(CommandContext ctx, DiscordMember member)
        {
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

                        // System.Numerics.Vector2 origin = new System.Numerics.Vector2(image.Width, image.Height);
                        // avatar.Mutate(x => x.Transform(
                        //     new AffineTransformBuilder().AppendTranslation(origin) ));

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

                /* 
                There's still room to improve,
                Send the file without saving it.
                */
                // image.Save("files/images/meme/amogusUser.png");

                using (Stream imageStream = new MemoryStream())
                {
                    image.SaveAsPng(imageStream);
                    image.Dispose();
                    imageStream.Position = 0;

                    var msg = await new DiscordMessageBuilder()
                        .WithContent($"vc e sus {member.Mention}")
                        .WithFiles(new Dictionary<string, Stream>() { {"amogusUser.png", imageStream } })
                        .SendAsync(ctx.Channel);

                    /*
                    Bug: Probably a leak?
                    Using the command increases the RAM
                    and then doesn't decrease even though
                    is disposed.
                    */
                    imageStream.Dispose();
                }
                
                
            }

            // using (var fs = new FileStream("files/images/meme/amogusUser.png", FileMode.Open, FileAccess.Read))
            // {
            //     var msg = await new DiscordMessageBuilder()
            //         .WithContent($"vc e sus {member.Value.Mention}")
            //         .WithFiles(new Dictionary<string, Stream>() { {"files/images/meme/amogusUser.png", fs } })
            //         .SendAsync(ctx.Channel);
            // }

        }

        

    }
}