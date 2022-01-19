using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DiscordBot.Commands.Embed.RandomFiles
{
    public class RandomCommandSel
    {
        private static void RandomFolder(ulong guildId)
        {
            string dir = $"files/data/guilds/{guildId}";
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
                    sw.WriteLine(output);
                    sw.Dispose();
                }
            }
        }
        public static async Task<DiscordMessageBuilder> RandomSelect(ulong guildId, IEnumerable<DiscordMember> members, RandomClass jsonFile)
        {
            RandomFolder(guildId);

            string dir = $"files/data/guilds/{guildId}";
            string jsonfile = $"{dir}/random.json";

            jsonFile.Today = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            jsonFile.Date = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour + 2, DateTime.Now.Minute, DateTime.Now.Second);

            var random = new Random();

            var users = members.Where(x => !x.IsBot);

            var user = users.ElementAt(random.Next(0, users.Count()));
            jsonFile.UserId = user.Id;

            string output = JsonConvert.SerializeObject(jsonFile);

            using (StreamWriter sw = new(jsonfile) )
            {
                await sw.WriteLineAsync(output);
                await sw.DisposeAsync();
            }

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
            {
                Height = 64,
                Width = 64,
                Url = user.GetAvatarUrl(DSharpPlus.ImageFormat.Auto, 64),
            };

            string[] linksArr = File.ReadAllLines("files/links.txt");

            int randomGif = random.Next( 0, linksArr.Length );
            string urlGif = linksArr.ElementAt( randomGif );

            var footer = new DiscordEmbedBuilder.EmbedFooter()
            {
                Text = $"-  Tempo restante: 2 horas",
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

            var msg = new DiscordMessageBuilder()
                .WithEmbed(embed);

            return msg;
        }

        public static async Task<RandomClass> RandomFile(ulong guildId)
        {
            RandomFolder(guildId);

            string dir = $"files/data/guilds/{guildId}";
            string jsonfile = $"{dir}/random.json";

            string jsonString = string.Empty;
            
            using (var fs = File.OpenRead(jsonfile))
            using (var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false) ) )
            {
                jsonString = await sr.ReadToEndAsync();
                
                sr.Dispose();
                fs.Dispose();
            }

            return JsonConvert.DeserializeObject<RandomClass>(jsonString);
        }
    }
}