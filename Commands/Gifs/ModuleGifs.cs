using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordBot.Commands.Gifs
{
    public class ModuleGifs : BaseCommandModule
    {
        [Command("cesar")]
        [Cooldown(1, 20, CooldownBucketType.Channel)]
        public async Task CesarCommand(CommandContext ctx, [RemainingText] string message)
        {
            using (var image = Image.Load("files/gifs/cesarNegocios.gif"))
            {
                image.Mutate( x => x.Resize(512, 512) );
                image.Mutate( x => x.Transform( new AffineTransformBuilder().AppendTranslation( new PointF(0, 86) ) ) );
                var memeImage = new Image<Rgba32>(512, 86, new Rgba32(255, 255, 255));
                image.Mutate( x => x.DrawImage(memeImage, 1f));
                memeImage.Dispose();


                var fontf = SixLabors.Fonts.SystemFonts.Find("ubuntu");
                var fontb = new SixLabors.Fonts.Font(fontf, 24f, SixLabors.Fonts.FontStyle.Bold);
                //var fontw = new SixLabors.Fonts.Font(fontf, 28f, SixLabors.Fonts.FontStyle.Bold);
                
                var options = new DrawingOptions() 
                {
                    TextOptions = new TextOptions()
                    {
                        VerticalAlignment = SixLabors.Fonts.VerticalAlignment.Center,
                        WrapTextWidth = 450,
                        HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Center
                    }
                };

                image.Mutate( x => x.DrawText(options, message, fontb, Color.Black, new PointF( 32f, 42f )) );

                await using (Stream imageStream = new MemoryStream())
                {
                    image.SaveAsGif(imageStream);
                    image.Dispose();
                    imageStream.Position = 0;

                    var msg = await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { {"cesarNegocios.gif", imageStream } })
                        .SendAsync(ctx.Channel);

                    await imageStream.DisposeAsync();
                }
            }
        }

        [Command("gif")]
        [Cooldown(1, 20, CooldownBucketType.Channel)]
        public async Task GifCommand(CommandContext ctx, [RemainingText] string message)
        {
            var urlGif = ctx.Message.Attachments.ElementAt(0).Url;
            using ( var userUrl = new HttpClient() )
            {
                using (var response = await userUrl.GetAsync(urlGif))
                {
                    response.EnsureSuccessStatusCode();

                    var stream = await response.Content.ReadAsStreamAsync();
                    const long bytesToRead = 8 * 1024 * 1024;

                    if (stream.Length > bytesToRead) { await ctx.RespondAsync("O gif tem mais que 8mb."); }
                    else
                    {
                        
                        // Lower resolutions the text doesn't draw.
                        
                        var image = Image.Load(stream);

                        int Width = Convert.ToInt32(image.Width / 1.2f);
                        int Height = Convert.ToInt32(image.Height / 1.2f);


                        image.Mutate( x => x.Resize( Width, Height ) );

                        // float x = image.Width / 1f;
                        // float y = image.Height / 1f;
                        // var ratioX = (double)x / image.Width;
                        // var ratioY = (double)y / image.Height;
                        // var ratio = Math.Min(ratioX, ratioY);
                        
                        // int Width = Decimal.ToInt32( Math.Round( Convert.ToDecimal( x ) ) );
                        // int Height = Decimal.ToInt32( Math.Round( Convert.ToDecimal( y ) ) );
                        
                        // float dpiX = Convert.ToSingle(image.Metadata.HorizontalResolution);
                        // float dpiY = Convert.ToSingle(image.Metadata.VerticalResolution);

                        var fontf = SixLabors.Fonts.SystemFonts.Find("ubuntu");
                        var fontb = new SixLabors.Fonts.Font(fontf, 32f, SixLabors.Fonts.FontStyle.Bold);

                        var options = new DrawingOptions() 
                        {
                            TextOptions = new TextOptions()
                            {
                                VerticalAlignment = SixLabors.Fonts.VerticalAlignment.Center,
                                WrapTextWidth = image.Width,
                                HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Center,
                            }

                        };
                        
                        var stringSize = SixLabors.Fonts.TextMeasurer.Measure(message, new SixLabors.Fonts.RendererOptions(fontb));

                        int HeightFinal = Convert.ToInt32(stringSize.Height + 64f);
                        // if (stringSize.Width > image.Width) { HeightFinal = ( image.Height / 6 ) * 2; }
                        // else { HeightFinal = image.Height / 6; }

                        var memeImage = new Image<Rgba32>(image.Width, HeightFinal, new Rgba32(255, 255, 255));

                        var ratioX = (double)stringSize.Width / memeImage.Width;
                        var ratioY = (double)stringSize.Height / memeImage.Height;
                        var ratio = Math.Min(ratioX, ratioY);

                        const long bytesToReadgif = 4 * 1024 * 1024;
                        float sizeFont = 64f;
                        if (stream.Length > bytesToReadgif) { sizeFont = 128f; }

                        var scaledFont = new SixLabors.Fonts.Font( fontf, Convert.ToSingle(ratio) * sizeFont, SixLabors.Fonts.FontStyle.Bold );

                        //image.Mutate( x => x.Resize( Width, Height ) );
                        image.Mutate( x => x.Transform( new AffineTransformBuilder().AppendTranslation( new PointF(0, HeightFinal) ) ) );

                        //float scalingFactor = memeImage.Height / stringSize.Height;
                        //var scaledFont = new SixLabors.Fonts.Font(fontf, scalingFactor * fontb.Size, SixLabors.Fonts.FontStyle.Bold);

                        image.Mutate( x => x.DrawImage(memeImage, 1f));
                        image.Mutate( x => x.DrawText(
                            options, 
                            message, 
                            scaledFont, 
                            Color.Black, 
                            new PointF( 0, memeImage.Height / 2f ) ) );
                        
                        memeImage.Dispose();

                        await using (Stream imageStream = new MemoryStream())
                        {
                            image.SaveAsGif(imageStream);
                            image.Dispose();
                            imageStream.Position = 0;

                            var msg = await new DiscordMessageBuilder()
                                .WithFiles(new Dictionary<string, Stream>() { {"meme.gif", imageStream } })
                                .SendAsync(ctx.Channel);

                            await imageStream.DisposeAsync();
                        }
                    }

                }
            }
        }
    }
}