using DiscordBot.Commands.Embed.Twitch;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DiscordBot.MonitorTwitch
{
    public class TwitchChannels
    {
        [JsonProperty("channels")]
        public List<string> Channels {get; set;}
        [JsonProperty("roles")]
        public HashSet<string> Roles {get; set;}

        public TwitchChannels()
        {
            Channels = new();
            Roles = new();
        }

        private static FileStream GetFileStream(string guildId)
        {
            return File.Open($"files/data/guilds/{guildId}/twitchchannels.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public async static Task<TwitchChannels> GetContent(string guildId)
        {
            var str = string.Empty;

            if(File.Exists($"files/data/guilds/{guildId}/twitchchannels.json"))
            {
                using ( var sr = new StreamReader(GetFileStream(guildId)) )
                {
                    str = await sr.ReadToEndAsync();

                    // sr.Dispose();
                }

                return JsonConvert.DeserializeObject<TwitchChannels>(str) ?? new TwitchChannels();
            }
            else
            {
                return new TwitchChannels();
            }
        }

        public async static Task SaveContent(TwitchChannels input, string guildId)
        {
            var str = JsonConvert.SerializeObject(input);

            using ( var sw = new StreamWriter(GetFileStream(guildId)) )
            {
                await sw.WriteLineAsync(str);

                await sw.DisposeAsync();
            }
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

        public static Task SaveProfile(TwitchChannels input, string guildId)
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

        private static async Task SaveInitialChannels(TwitchChannels input, string guildId)
        {
            // CheckDir(guildId);

            // const string file = "twitchchannels.json";
            // string dir = "files/data/guilds/" + guildId;

            // var str = JsonConvert.SerializeObject(input);

            // using (var fs = File.Open($"{dir}/{file}", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            // {
            //     using ( var sw = new StreamWriter( fs, new System.Text.UTF8Encoding(false) ) )
            //     {
            //         await sw.WriteLineAsync(str);

            //         sw.Close();
            //         fs.Close();
            //     }
            // }

            await SaveContent(input, guildId);
        }

        private static async Task SaveChannels(TwitchChannels input, string guildId)
        {
            // const string file = "twitchchannels.json";
            // string dir = "files/data/guilds/" + guildId;
            // var str = string.Empty;

            // using (var fs = File.Open($"{dir}/{file}", FileMode.Open, FileAccess.Read))
            // {
            //     using ( var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false)) )
            //     {
            //         str = await sr.ReadToEndAsync();

            //         sr.Dispose();
            //         await fs.DisposeAsync();
            //     }
            // }

            var filechannels = await GetContent(guildId);

            // var filechannels = JsonConvert.DeserializeObject<TwitchChannels>(str);

            if (filechannels is null)
            {
                // str = JsonConvert.SerializeObject(input);

                // using (var fsw = File.Open($"{dir}/{file}", FileMode.Open, FileAccess.Write))
                // {
                //     using ( var sw = new StreamWriter(fsw, new System.Text.UTF8Encoding(false)) )
                //     {
                //         await sw.WriteLineAsync(str);

                //         await sw.DisposeAsync();
                //         await fsw.DisposeAsync();
                //     }
                // }

                await SaveContent(input, guildId);
            }
            else
            {
                var query = from chn in input.Channels
                            where !filechannels.Channels.Contains(chn)
                            select chn;
                
                if (query == null) { return; }

                filechannels.Channels.AddRange(query);

                // str = JsonConvert.SerializeObject(filechannels);

                // using (var fsw = File.Open($"{dir}/{file}", FileMode.Open, FileAccess.Write))
                // {
                //     using ( var sw = new StreamWriter(fsw, new System.Text.UTF8Encoding(false)) )
                //     {
                //         await sw.WriteLineAsync(str);

                //         await sw.DisposeAsync();
                //         await fsw.DisposeAsync();
                //     }
                // }

                await SaveContent(filechannels, guildId);
            }
        }

        public static Task SaveMainFile(TwitchChannels input)
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
                    var filechannels = JsonConvert.DeserializeObject<TwitchChannels>(str) ?? 
                        throw new Exception("filechannels cannot be null, delete it so it can be created again");

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