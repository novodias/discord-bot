using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DiscordBot.Commands.Images
{
    public class ModuleImages : BaseCommandModule
    {
        [Command("img")]
        public async Task ImageCommand(CommandContext ctx, [RemainingText] string strLink)
        {
            // strLink example: c/img LETS FUCKING GOOOO#https://image.com/image.jpg

            string[]? msgOutputArr = null;

            await ctx.TriggerTypingAsync();

            string[] input = strLink.Split('#', 2, StringSplitOptions.None);
            string msgOutput = input.First();
            string imageLink = input.Last();

            if (msgOutput.Contains('&'))
            {
                msgOutputArr = msgOutput.Split('&', 2, StringSplitOptions.None);
            }

            using (HttpClient client = new())
            {
                using (var response = await client.GetAsync(imageLink))
                {
                    response.EnsureSuccessStatusCode();

                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    {
                        const long maxbytes = 8 * 1024 * 1024;

                        if (stream.Length > maxbytes)
                        {
                            await ctx.RespondAsync("A imagem tem mais que 8mb");
                            return;
                        }
                        
                        var image = Image.Load(stream);

                        try
                        {
                            image.Mutate( x => x.Resize( new ResizeOptions
                                {
                                    Mode = ResizeMode.Max,
                                    Size = new Size(512, 512),
                                    Compand = true
                                }));

                            var fontf = SixLabors.Fonts.SystemFonts.Find("ubuntu");
                            var fontb = new SixLabors.Fonts.Font(fontf, 6f, SixLabors.Fonts.FontStyle.Bold);

                            var optionstop = new DrawingOptions() 
                            {
                                TextOptions = new TextOptions()
                                {
                                    VerticalAlignment = SixLabors.Fonts.VerticalAlignment.Top,
                                    WrapTextWidth = image.Width,
                                    HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Center,
                                    DpiX = image.Width,
                                    DpiY = image.Height,
                                }

                            };

                            var optionsbot = new DrawingOptions() 
                            {
                                TextOptions = new TextOptions()
                                {
                                    VerticalAlignment = SixLabors.Fonts.VerticalAlignment.Top,
                                    WrapTextWidth = image.Width,
                                    HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Center,
                                    DpiX = image.Width,
                                    DpiY = image.Height,
                                }

                            };
                            
                            var stringSize = SixLabors.Fonts.TextMeasurer.Measure(msgOutput, new SixLabors.Fonts.RendererOptions(fontb));

                            // int HeightFinal = Convert.ToInt32(stringSize.Height + 64f);

                            // var memeImage = new Image<Rgba32>(image.Width, HeightFinal, new Rgba32(255, 255, 255));

                            // var ratioX = (double)stringSize.Width / image.Width;
                            // var ratioY = (double)stringSize.Height / image.Height;
                            // var ratio = Math.Min(ratioX, ratioY);

                            // float sizeFont = 86f;

                            // var scaledFont = new SixLabors.Fonts.Font( fontf, 86f, SixLabors.Fonts.FontStyle.Bold );

                            // image.Mutate( x => x.Transform( new AffineTransformBuilder().AppendTranslation( new PointF(0, HeightFinal) ) ) );

                            // image.Mutate( x => x.DrawImage(memeImage, 1f));

                            // image.Mutate( x => x.DrawText(
                            //     options, 
                            //     msgOutputArr.First(), 
                            //     scaledFont, 
                            //     Color.Black, 
                            //     new PointF( 0, HeightFinal / 2f ) ) );
                            if (msgOutputArr == null)
                            {
                                image.Mutate( x => x.DrawText(
                                optionstop,
                                msgOutput,
                                fontb,
                                Brushes.Solid(Color.White),
                                Pens.Solid(Color.Black, 2f),
                                new PointF( 0, 0 ) ) );
                            }
                            else
                            {
                                image.Mutate( x => x.DrawText(
                                    optionstop,
                                    msgOutputArr.First(),
                                    fontb,
                                    Brushes.Solid(Color.White),
                                    Pens.Solid(Color.Black, 2f),
                                    new PointF( 0, 0 ) ) );

                                image.Mutate( x => x.DrawText(
                                    optionsbot,
                                    msgOutputArr.Last(),
                                    fontb,
                                    Brushes.Solid(Color.White),
                                    Pens.Solid(Color.Black, 2f),
                                    new PointF( 0, image.Height / 1.2f ) ) );
                            }

                            // memeImage.Dispose();

                            await using (Stream imageStream = new MemoryStream())
                            {
                                image.SaveAsPng(imageStream);
                                image.Dispose();
                                imageStream.Position = 0;

                                if (imageStream.Length > maxbytes) { await ctx.RespondAsync("Infelizmente o png final tem mais que 8mb"); }
                                else
                                {
                                    var msg = await new DiscordMessageBuilder()
                                        .WithFiles(new Dictionary<string, Stream>() { {"meme.png", imageStream } })
                                        .SendAsync(ctx.Channel);
                                }

                                await imageStream.DisposeAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            await ctx.RespondAsync(ex.Message);
                        }
                    }
                }
            }
        }
    }
}