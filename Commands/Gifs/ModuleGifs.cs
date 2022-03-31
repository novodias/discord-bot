using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SixLabors.ImageSharp.Formats.Gif;

namespace DiscordBot.Commands.Gifs
{
    public class ModuleGifs : BaseCommandModule
    {
        // [Command("cesar")]
        // [Cooldown(1, 20, CooldownBucketType.Channel)]
        // public async Task CesarCommand(CommandContext ctx, [RemainingText] string message)
        // {
        //     await ctx.TriggerTypingAsync();

        //     if (string.IsNullOrEmpty(message)) { await ctx.RespondAsync("O comando não pode ser usado sem uma frase"); return; }
            
        //     using (var image = Image.Load("files/gifs/cesarNegocios.gif"))
        //     {
        //         image.Mutate( x => x.Resize(512, 512) );
        //         image.Mutate( x => x.Transform( new AffineTransformBuilder().AppendTranslation( new PointF(0, 86) ) ) );
        //         var memeImage = new Image<Rgba32>(512, 86, new Rgba32(255, 255, 255));
        //         image.Mutate( x => x.DrawImage(memeImage, 1f));
        //         memeImage.Dispose();


        //         var fontf = SixLabors.Fonts.SystemFonts.Get("ubuntu");
        //         var fontb = new SixLabors.Fonts.Font(fontf, 24f, SixLabors.Fonts.FontStyle.Bold);
        //         //var fontw = new SixLabors.Fonts.Font(fontf, 28f, SixLabors.Fonts.FontStyle.Bold);
                
        //         var options = new DrawingOptions() 
        //         {
        //             TextOptions = new TextOptions()
        //             {
        //                 VerticalAlignment = SixLabors.Fonts.VerticalAlignment.Center,
        //                 WrapTextWidth = 450,
        //                 HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Center
        //             }
        //         };

        //         image.Mutate( x => x.DrawText(options, message, fontb, Color.Black, new PointF( 32f, 42f )) );

        //         await using (Stream imageStream = new MemoryStream())
        //         {
        //             image.SaveAsGif(imageStream);
        //             image.Dispose();
        //             imageStream.Position = 0;

        //             var msg = await new DiscordMessageBuilder()
        //                 .WithFiles(new Dictionary<string, Stream>() { {"cesarNegocios.gif", imageStream } })
        //                 .SendAsync(ctx.Channel);

        //             await imageStream.DisposeAsync();
        //         }
        //     }
        // }

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

                try
                {
                    var Stream = await response.Content.ReadAsStreamAsync();
                    Image = new( Stream, Message );
                    await Stream.DisposeAsync();
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync( ex.Message );
                    return;
                }

                response.Dispose();
            }

            try
            {
                using var ImageStream = await Image.GetImageStream();
                ImageStream.Position = 0;

                var msg = await new DiscordMessageBuilder()
                    .WithFiles(new Dictionary<string, Stream>() { { "meme.gif", ImageStream } })
                    .SendAsync(ctx.Channel);
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync( ex.Message );
            }
            finally
            {
                Image.Dispose();
            }
            
        }

        [Command("pawg")]
        public async Task PawgCommand(CommandContext ctx, [RemainingText] string linkURL)
        {
            await ctx.TriggerTypingAsync();

            if ( string.IsNullOrEmpty( linkURL ) && !ctx.Message.Attachments.Any() ) 
            {
                await ctx.RespondAsync("O comando não pode ser usado sem um link ou anexo"); 
                return;
            }

            string? attachmentLink = ctx.Message.Attachments[0].Url;
            string link = ctx.Message.Attachments.Any() ? attachmentLink : linkURL;

            using var Client = new HttpClient();
            using var response = await Client.GetAsync( link );
            response.EnsureSuccessStatusCode();

            var Stream = await response.Content.ReadAsStreamAsync();
            using Image Picture = Image.Load( Stream );
            await Stream.DisposeAsync();
            
            response.Dispose();
            Client.Dispose();

            try
            {

                Picture.Mutate(img => img
                    .Resize(
                        new ResizeOptions()
                        {
                            Size = new Size(100, 100),
                            Mode = ResizeMode.Max,
                        }
                    )
                );

                DrawingOptions options = new()
                {
                    GraphicsOptions = new GraphicsOptions()
                    {
                        AlphaCompositionMode = PixelAlphaCompositionMode.SrcIn,
                        ColorBlendingMode = PixelColorBlendingMode.Darken,
                    }
                };

                var Red = Picture.Clone(red => red
                    .Fill( options, Color.Red )
                    .DrawImage( Picture, PixelColorBlendingMode.Darken, 0.8f )
                );

                var Blue = Picture.Clone(blue => blue
                    .Fill( options, new Rgba32(0, 128, 255) )
                    .DrawImage( Picture, PixelColorBlendingMode.Darken, 0.8f )
                    .Resize(Convert.ToInt32(Red.Width * 0.8), Convert.ToInt32(Red.Height * 0.8))
                );

                Picture.Dispose();

                const int frameDelay = 2;
                const GifDisposalMethod disposalMethod = GifDisposalMethod.RestoreToBackground;

                using Image Gif = new Image<Rgba32>(150, 150, Color.Transparent);
                
                var gifMetaData = Gif.Metadata.GetGifMetadata();
                gifMetaData.RepeatCount = 0;

                GifFrameMetadata metadata = Gif.Frames.RootFrame.Metadata.GetGifMetadata();
                metadata.FrameDelay = frameDelay;

                for (int i = 0; i < 8; i++)
                {
                    using (var frame = new Image<Rgba32>(150, 150, Color.Transparent))
                    {
                        metadata = frame.Frames.RootFrame.Metadata.GetGifMetadata();
                        metadata.FrameDelay = frameDelay;
                        metadata.DisposalMethod = disposalMethod;

                        if ( i == 0 )
                        {
                            frame.Mutate( img => img
                                .DrawImage( Red, new Point(10, 10), 1f )
                                .DrawImage( Blue, new Point(60, 50), 1f ) 
                            );
                        }

                        if ( i == 1 )
                        {
                            frame.Mutate( img => img
                                .DrawImage( Red, new Point(14, 14), 1f )
                                .DrawImage( Blue, new Point(56, 48), 1f ) 
                            );
                        }

                        if ( i == 2 )
                        {
                            frame.Mutate( img => img
                                .DrawImage( Red, new Point(16, 16), 1f )
                                .DrawImage( Blue, new Point(54, 47), 1f ) 
                            );
                        }
                        
                        if ( i == 3 )
                        {
                            frame.Mutate( img => img
                                .DrawImage( Red, new Point(20, 20), 1f )
                                .DrawImage( Blue, new Point(50, 45), 1f ) 
                            );
                        }

                        if ( i == 4 )
                        {
                            frame.Mutate( img => img
                                .DrawImage( Red, new Point(17, 17), 1f )
                                .DrawImage( Blue, new Point(53, 47), 1f ) 
                            );
                        }

                        if ( i == 5 )
                        {
                            frame.Mutate( img => img
                                .DrawImage( Red, new Point(14, 14), 1f )
                                .DrawImage( Blue, new Point(55, 48), 1f ) 
                            );
                        }

                        if ( i == 6 )
                        {
                            frame.Mutate( img => img
                                .DrawImage( Red, new Point(12, 12), 1f )
                                .DrawImage( Blue, new Point(57, 49), 1f ) 
                            );
                        }

                        if ( i == 7 )
                        {
                            frame.Mutate( img => img
                                .DrawImage( Red, new Point(10, 10), 1f )
                                .DrawImage( Blue, new Point(60, 50), 1f ) 
                            );
                        }

                        Gif.Frames.AddFrame(frame.Frames.RootFrame);
                    }
                }

                Gif.Frames.RemoveFrame(0);

                GifEncoder Encoder = new()
                {
                    ColorTableMode = GifColorTableMode.Local,
                };

                using Stream ImageStream = new MemoryStream();
                await Gif.SaveAsGifAsync(ImageStream, Encoder);
                ImageStream.Position = 0;

                var msg = new DiscordMessageBuilder()
                    .WithFile("pawg.gif", ImageStream)
                    .WithReply(ctx.Message.Id);

                await msg.SendAsync(ctx.Channel);
                await ImageStream.DisposeAsync();
                Gif.Dispose();
                Red.Dispose();
                Blue.Dispose();

                return;
            }
            catch (System.Exception ex)
            {
                await ctx.RespondAsync( ex.Message + ex.StackTrace + "\n" + link );
                return;
            }

        }
    }
}