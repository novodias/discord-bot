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
            await ctx.TriggerTypingAsync();

            if (string.IsNullOrEmpty(message)) { await ctx.RespondAsync("O comando não pode ser usado sem uma frase"); return; }
            
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
            await ctx.TriggerTypingAsync();

            if ( string.IsNullOrEmpty( message ) ) 
            {
                await ctx.RespondAsync("O comando não pode ser usado sem uma frase"); 
                return;
            }

            string urlGif, Message;
            if ( message.Contains('#') )
            {
                var arrayStr = message.Split('#');
                
                if ( !arrayStr[1].Contains("https") )
                {
                    await ctx.RespondAsync("A segunda parte não é um link.");
                    return;
                }
                
                Message = arrayStr.First();
                urlGif = arrayStr.Last();
            }
            else
            {
                Message = message;
                urlGif = ctx.Message.Attachments.ElementAt(0).Url;
            }

            Gif Image;
            
            using ( var userUrl = new HttpClient() )
            {
                using var response = await userUrl.GetAsync(urlGif);
                response.EnsureSuccessStatusCode();

                var Stream = await response.Content.ReadAsStreamAsync();
                Image = new( Stream, Message );
            }

            var ImageStream = await Image.GetImageStream();

            try
            {
                ImageStream.Position = 0;

                var msg = await new DiscordMessageBuilder()
                    .WithFiles( new Dictionary<string, Stream>() { { "meme.gif", ImageStream } } )
                    .SendAsync( ctx.Channel );
            }
            catch ( Exception ex )
            {
                await ctx.RespondAsync( ex.Message + ex.StackTrace );
            }
            finally
            {
                await ImageStream.DisposeAsync();
                Image.Dispose();
            }

            // const long bytesToRead = 8 * 1024 * 1024;

            // if (stream.Length > bytesToRead) 
            // { 
            //     await ctx.RespondAsync("O gif tem mais que 8mb."); 
            //     return; 
            // }
            
            // var image = Image.Load(stream);
            // var fontf = SixLabors.Fonts.SystemFonts.Find("ubuntu");
            // var fontb = new SixLabors.Fonts.Font(fontf, 56f, SixLabors.Fonts.FontStyle.Bold);

            // var options = new DrawingOptions() 
            // {
            //     TextOptions = new TextOptions()
            //     {
            //         VerticalAlignment = SixLabors.Fonts.VerticalAlignment.Center,
            //         WrapTextWidth = image.Width,
            //         HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Center,
            //     }

            // };

            // var stringSize = SixLabors.Fonts.TextMeasurer.Measure(message, new SixLabors.Fonts.RendererOptions(fontb));

            // int HeightFinal = Convert.ToInt32(stringSize.Height + 64f);

            // var memeImage = new Image<Rgba32>(image.Width, HeightFinal, new Rgba32(255, 255, 255));

            // var ratioX = (double)stringSize.Width / memeImage.Width;
            // var ratioY = (double)stringSize.Height / memeImage.Height;
            // var ratio = Math.Min(ratioX, ratioY);

            // float sizeFont = 56f;

            // var scaledFont = new SixLabors.Fonts.Font( fontf, Convert.ToSingle(ratio) * sizeFont, SixLabors.Fonts.FontStyle.Bold );
            
            // try
            // {
            //     image.Mutate( x => x.Transform( new AffineTransformBuilder().AppendTranslation( new PointF(0, HeightFinal) ) ) );

            //     image.Mutate( x => x.DrawImage(memeImage, 1f));
            //     image.Mutate( x => x.DrawText(
            //         options, 
            //         message, 
            //         scaledFont, 
            //         Color.Black, 
            //         new PointF( 0, memeImage.Height / 2f ) ) );

            //     if (image.Height > 256 && image.Width > 256)
            //     {
            //         image.Mutate( x => x.Resize( new ResizeOptions
            //         {
            //             Mode = ResizeMode.Max,
            //             Size = new Size(image.Width / 2, image.Width / 2),
            //             Compand = true
            //         })); 
            //     }

            //     memeImage.Dispose();

            //     using ( Stream imageStream = new MemoryStream() )
            //     {
            //         image.SaveAsGif(imageStream);
            //         image.Dispose();
            //         imageStream.Position = 0;

            //         if (imageStream.Length > bytesToRead) { await ctx.RespondAsync("Infelizmente o gif final tem mais que 8mbs..."); }
            //         else
            //         {
            //             var msg = await new DiscordMessageBuilder()
            //                 .WithFiles(new Dictionary<string, Stream>() { {"meme.gif", imageStream } })
            //                 .SendAsync(ctx.Channel);
            //         }

            //         await imageStream.DisposeAsync();
            //     }
            // }
            // catch (Exception ex)
            // {
            //     await ctx.RespondAsync(ex.Message);
            // }
        }
    }
}