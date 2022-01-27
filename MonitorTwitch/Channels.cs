using DiscordBot.Commands.Embed.Twitch;
using Newtonsoft.Json;

namespace DiscordBot.MonitorTwitch
{
    public class JsonChannels
    {
        [JsonProperty("channels")]
        public List<string> Channels {get; set;}

        public JsonChannels()
        {
            Channels = new();
        }

        public static async Task<List<string>> GetSplitChannels(string input, Twitch twitchapi)
        {
            var lst = input.Split('#').ToList();
            var list = new List<string>(lst.Count);

            foreach (var item in lst)
            {
                var chn = await twitchapi.GetChannelAsync(item);
                if (chn is not null)
                {
                    list.Add(item.ToLower());
                }
            }

            return list;
        }

        // private static Task? t1;
        private static Task? t2;

        public static Task WaitTasks()
        {
            if (t2 != null)
            {
                t2.Wait();
            }

            return Task.CompletedTask;
        }

        public static Task SaveProfile(JsonChannels input, string guildId)
        {
            string dir = "files/data/guilds/" + guildId;
            const string file = "twitchchannels.json";

            Task.Run ( async () => 
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string filedir = $"{dir}/{file}";
                
                if (!File.Exists($"{dir}/{file}"))
                {
                    await SaveInitialChannels(input, guildId);
                }
                else
                {
                    string content = await File.ReadAllTextAsync(filedir);
                    if (content is null)
                    {
                        await SaveInitialChannels(input, guildId);
                    }
                    else
                    {
                        await SaveChannels(input, guildId);
                    }
                }
            });

            return Task.CompletedTask;
        }

        private static async Task SaveInitialChannels(JsonChannels input, string guildId)
        {
            // CheckDir(guildId);

            const string file = "twitchchannels.json";
            string dir = "files/data/guilds/" + guildId;

            var str = JsonConvert.SerializeObject(input);

            using (var fs = File.Open($"{dir}/{file}", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using ( var sw = new StreamWriter( fs, new System.Text.UTF8Encoding(false) ) )
                {
                    await sw.WriteLineAsync(str);

                    sw.Close();
                    fs.Close();
                }
            }
        }

        private static async Task SaveChannels(JsonChannels input, string guildId)
        {
            const string file = "twitchchannels.json";
            string dir = "files/data/guilds/" + guildId;
            var str = string.Empty;

            using (var fs = File.Open($"{dir}/{file}", FileMode.Open, FileAccess.Read))
            {
                using ( var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false)) )
                {
                    str = await sr.ReadToEndAsync();

                    sr.Dispose();
                    await fs.DisposeAsync();
                }
            }

            var filechannels = JsonConvert.DeserializeObject<JsonChannels>(str);

            if (filechannels is null)
            {
                str = JsonConvert.SerializeObject(input);

                using (var fsw = File.Open($"{dir}/{file}", FileMode.Open, FileAccess.Write))
                {
                    using ( var sw = new StreamWriter(fsw, new System.Text.UTF8Encoding(false)) )
                    {
                        await sw.WriteLineAsync(str);

                        await sw.DisposeAsync();
                        await fsw.DisposeAsync();
                    }
                }
            }
            else
            {
                // Why not AddRange? Because of lists that has only one object
                var query = from chn in input.Channels
                            where !filechannels.Channels.Contains(chn)
                            select chn;
                
                if (query == null) { return; }

                filechannels.Channels.AddRange(query);

                str = JsonConvert.SerializeObject(filechannels);

                using (var fsw = File.Open($"{dir}/{file}", FileMode.Open, FileAccess.Write))
                {
                    using ( var sw = new StreamWriter(fsw, new System.Text.UTF8Encoding(false)) )
                    {
                        await sw.WriteLineAsync(str);

                        await sw.DisposeAsync();
                        await fsw.DisposeAsync();
                    }
                }
            }
        }

        public static Task SaveMainFile(JsonChannels input)
        {
            const string file = "files/channels.json";
            
            t2 = Task.Run( async () => 
            {
                if (!File.Exists(file))
                {
                    string strconf = JsonConvert.SerializeObject(input);
                    
                    using (var fs = File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        using ( var sw = new StreamWriter(fs, new System.Text.UTF8Encoding(false)) )
                        {
                            await sw.WriteLineAsync(strconf);

                            await sw.DisposeAsync();
                            await fs.DisposeAsync();
                        }
                    }
                }
                else
                {
                    var str = string.Empty;

                    using ( var fsr = File.Open(file, FileMode.Open, FileAccess.Read))
                    {
                        using ( var sr = new StreamReader(fsr, new System.Text.UTF8Encoding(false)) )
                        {
                            str = await sr.ReadToEndAsync();

                            sr.Dispose();
                            await fsr.DisposeAsync();
                            fsr.Close();
                        }
                    }


                    // Will be throwned if the file exists and there's nothing inside.
                    var filechannels = JsonConvert.DeserializeObject<JsonChannels>(str) ?? 
                        throw new Exception("filechannels cannot be null");

                    var query = from chn in input.Channels
                                where !filechannels.Channels.Contains(chn)
                                select chn;
                    
                    if (query == null) { return; }

                    filechannels.Channels.AddRange(query);

                    str = JsonConvert.SerializeObject(filechannels);

                    using (var fsw = File.Open(file, FileMode.Open, FileAccess.Write))
                    {
                        using ( var sw = new StreamWriter(fsw, new System.Text.UTF8Encoding(false)) )
                        {
                            await sw.WriteLineAsync(str);

                            await sw.DisposeAsync();
                            await fsw.DisposeAsync();
                            fsw.Close();
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }

    }
}