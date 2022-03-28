using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace DiscordBot.Commands.Gifs
{
    public class Gif
    {
        private readonly FontFamily UbuntuFont = SystemFonts.Find("ubuntu");
        private const long MaxBytes = 8 * 1024 * 1024;
        private readonly Image Image;
        private readonly Font Font;
        private readonly DrawingOptions Options;
        private readonly string Message;

        public Gif( Stream ImageStream, string Message ) 
        {
            Image = Image.Load( ImageStream );
            this.Message = Message;

            Font = new( UbuntuFont, 18f, FontStyle.Bold );

            Options = new DrawingOptions()
            {
                TextOptions = new TextOptions()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    WrapTextWidth = Image.Width,
                    DpiX = (float)Image.Metadata.HorizontalResolution,
                    DpiY = (float)Image.Metadata.VerticalResolution
                }
            };

            Image.Metadata.IccProfile = null;
        }

        public async Task<Stream> GetImageStream()
        {
            var StringSize = TextMeasurer.Measure( Message, new RendererOptions( Font, 
                Options.TextOptions.DpiX, Options.TextOptions.DpiY ) );

            int HeightFinal = Convert.ToInt32( StringSize.Height + 64f );
            
            Image.Mutate( x => x.Transform( new AffineTransformBuilder().AppendTranslation( new PointF( 0, HeightFinal ) ) ) );
            using ( var WhiteBar = new Image<Rgba32>( Image.Width, HeightFinal, new Rgba32(255, 255, 255) ) )
            {
                WhiteBar.Metadata.HorizontalResolution = Image.Metadata.HorizontalResolution;
                WhiteBar.Metadata.VerticalResolution = Image.Metadata.VerticalResolution;

                float scalingFactor = WhiteBar.Height / StringSize.Height;
                float fontSize = (Font.Size * scalingFactor) / Font.Size;
                var scaledFont = new Font( UbuntuFont, Font.Size - fontSize, FontStyle.Bold );

                WhiteBar.Mutate( img => img.DrawText( Options, Message, scaledFont, Color.Black, new PointF( 0, HeightFinal / 2 ) ) ); 

                Image.Mutate( x => x.DrawImage( WhiteBar, 1f ) );
            }
            
            // Image.Mutate( x => x.DrawText(
            //     Options,
            //     Message,
            //     Font,
            //     Color.Black,
            //     new PointF( 0, HeightFinal / 2 ) )
            // );

            GifEncoder encoder = new()
            {
                ColorTableMode = GifColorTableMode.Global,
                Quantizer = new OctreeQuantizer( new QuantizerOptions() {
                    MaxColors = 32,
                }),
            };

            int Width, Height;
            Width = Image.Width >= 512 ?
                Convert.ToInt32( Image.Width / 1.5 ) : Image.Width;
            Height = Image.Height >= 512 ?
                Convert.ToInt32( Image.Height / 1.5 ) : Image.Height;

            Image.Mutate( img => img.Resize(Width, Height) );
            
            var ImageStream = new MemoryStream();
            await Image.SaveAsGifAsync( ImageStream, encoder );
            
            if ( ImageStream.Length > MaxBytes )
            {
                await ImageStream.FlushAsync();
                
                for (int i = 0; i < Image.Frames.Count; i++)
                {
                    if ( i % 2 == 0 )
                        Image.Frames.RemoveFrame(i);
                }

                await Image.SaveAsGifAsync( ImageStream, encoder );
            }

            return ImageStream;
        }

        ~Gif()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            Image.Dispose();
        }
    }
}