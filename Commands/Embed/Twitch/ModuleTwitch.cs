using DiscordBot.MonitorTwitch;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DiscordBot.Commands.Embed.Twitch
{
    public class ModuleTwitch : BaseCommandModule
    {
        private Twitch? Twitch;
        private MonitorRoles? MonitorRoles;

        [Command("twitch")]
        public async Task TwitchCommand(CommandContext ctx, [RemainingText] string input)
        {
            await ctx.TriggerTypingAsync();

            if (string.IsNullOrEmpty(input)) { await ctx.RespondAsync("O comando não pode ser usado sem o canal twitch"); return; }

            if (Twitch == null) { Twitch = new(); }
            
            try
            {
                var msg = await Twitch.GetStreamEmbed(input);
                msg.WithReply(ctx.Message.Id, true);
                await msg.SendAsync(ctx.Channel);
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync(ex.Message);
            }
        }

        [Command("monitorroles")]
        public async Task MonitorRoleCommand(CommandContext ctx, string operation, DiscordRole? role = null)
        {
            await ctx.TriggerTypingAsync();


            if (!ctx.Member.Permissions.HasPermission(Permissions.Administrator))
            { await ctx.RespondAsync("Você não tem permissão para executar esse comando."); return; }
            
            if ( MonitorRoles == null ) { MonitorRoles = new(ctx.Client); }
            
            await MonitorRoles.SubCommand(ctx.Channel, operation, ctx.Guild.Id.ToString(), role);

            // var str = string.Empty;
            
            // if (File.Exists($"files/data/guilds/{ctx.Guild.Id}/twitchchannels.json"))
            // {
            //     if (operation == "add" && role is not null)
            //     {
            //         using ( var fs = File.Open($"files/data/guilds/{ctx.Guild.Id}/twitchchannels.json", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            //         {
            //             try
            //             {
            //                 using ( var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false)) )
            //                 {
            //                     str = await sr.ReadToEndAsync();

            //                     sr.Dispose();
            //                 }

            //                 var file = JsonConvert.DeserializeObject<TwitchChannels>(str);
            //                 if (file.Roles is null) { file.Roles = new(); }
            //                 if (file.Roles.Contains(role)) { await ctx.RespondAsync("O role mencionado já foi adicionado préviamente"); await fs.DisposeAsync(); return; }
                            
            //                 file.Roles.Add(role);

            //                 str = JsonConvert.SerializeObject(file);

            //                 using ( var sw = new StreamWriter(fs, new System.Text.UTF8Encoding(false)) )
            //                 {
            //                     await sw.WriteLineAsync(str);

            //                     await sw.DisposeAsync();
            //                 }
                            
            //                 await ctx.RespondAsync("Role adicionado");
            //             }
            //             catch (Exception ex)
            //             {
            //                 await ctx.RespondAsync(ex.Message + ex.StackTrace);
            //             }
            //         }

            //     }
            //     else if (operation == "remove" && role is not null)
            //     {
            //         using ( var fs = File.Open($"files/data/guilds/{ctx.Guild.Id}/twitchchannels.json", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            //         {
            //             using ( var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false)) )
            //             {
            //                 str = await sr.ReadToEndAsync();

            //                 sr.Dispose();
            //             }

            //             var file = JsonConvert.DeserializeObject<TwitchChannels>(str);
            //             if (file.Roles is null) { file.Roles = new(); }
            //             if (!file.Roles.Contains(role)) { await ctx.RespondAsync("O role mencionado não existe ou já foi removido"); await fs.DisposeAsync(); return; }

            //             file.Roles.Remove(role);

            //             str = JsonConvert.SerializeObject(file);

            //             using ( var sw = new StreamWriter(fs, new System.Text.UTF8Encoding(false)) )
            //             {
            //                 await sw.WriteLineAsync(str);

            //                 await sw.DisposeAsync();
            //             }
            //         }
            //     }
            //     else
            //     {
            //         await ctx.RespondAsync("???");
            //     }
            // }
            // else
            // {
            //     if (operation == "add" && role is not null)
            //     {
            //         using ( var fs = File.Open($"files/data/guilds/{ctx.Guild.Id}/twitchchannels.json", FileMode.OpenOrCreate, FileAccess.ReadWrite) )
            //         {
            //             var file = new TwitchChannels();
                        
            //             file.Roles.Add(role);

            //             str = JsonConvert.SerializeObject(file);

            //             using ( var sw = new StreamWriter(fs, new System.Text.UTF8Encoding(false)) )
            //             {
            //                 await sw.WriteLineAsync(str);

            //                 await sw.DisposeAsync();
            //             }
            //         }
            //     }
            //     else if (operation == "remove" && role is not null)
            //     {
            //         await ctx.RespondAsync("Não é possível usar o sub-comando `remove` por que tem nada para remover!");
            //     }
            // }
        }

        // [Command("monitorroles")]
        // public async Task MonitorRoleCommand(CommandContext ctx, string operation)
        // {
        //     await ctx.TriggerTypingAsync();

        //     var str = string.Empty;

        //     if (!ctx.Member.Permissions.HasPermission(Permissions.Administrator)) { await ctx.RespondAsync("Você não tem permissão para executar esse comando."); return; }
            
        //     if (File.Exists($"files/data/guilds/{ctx.Guild.Id}/twitchchannels.json"))
        //     {
        //         if (operation == "help" || string.IsNullOrEmpty(operation))
        //         {
        //             await ctx.RespondAsync("Sub-comandos disponíveis: `add`, `remove`, `roles` e `help`.");
        //         }
        //         else if (operation == "roles")
        //         {
        //             using (var fs = File.OpenRead($"files/data/guilds/{ctx.Guild.Id}/twitchchannels.json"))
        //             {
        //                 using ( var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false)) )
        //                 {
        //                     str = await sr.ReadToEndAsync();
        //                 }
        //             }

        //             var file = JsonConvert.DeserializeObject<TwitchChannels>(str);

        //             string msgContent = string.Empty;

        //             foreach (var item in file.Roles)
        //             {
        //                 if (msgContent == string.Empty)
        //                 {
        //                     msgContent += $"`{item.Name}`";
        //                 }
        //                 else
        //                 {
        //                     msgContent += $", `{item.Name}`";
        //                 }
        //             }

        //             if (string.IsNullOrEmpty(msgContent))
        //             {
        //                 await ctx.RespondAsync("Não tem roles adicionados.");
        //             }
        //             else
        //             {
        //                 await ctx.RespondAsync($"Roles adicionados: {msgContent}");
        //             }
        //         }
        //         else if (operation == "add" || operation == "remove")
        //         {
        //             await ctx.RespondAsync("Role não mencionado!");
        //         }
        //         else
        //         {
        //             await ctx.RespondAsync("???");
        //         }
        //     }
        //     else
        //     {
        //         if (operation == "help" || string.IsNullOrEmpty(operation))
        //         {
        //             await ctx.RespondAsync("Sub-comandos disponíveis: `add` e `help`.");
        //         }
        //     }
        // }

        [Command("monitor")]
        public async Task MonitorCommand(CommandContext ctx, [RemainingText] string streamers)
        {
            // Transformar em funções, muita linha de código
            await ctx.TriggerTypingAsync();

            if (File.Exists($"files/data/guilds/{ctx.Guild.Id}/twitchchannels.json"))
            {
                var file = await TwitchChannels.GetContent(ctx.Guild.Id.ToString());
                if ( file.Roles is not null )
                {
                    var query = from role in file.Roles
                                where ctx.Member.Roles.Contains(ctx.Guild.GetRole(Convert.ToUInt64(role)))
                                select role;

                    if (!query.Any())
                    {
                        if (!ctx.Member.Permissions.HasPermission(Permissions.Administrator))
                        {
                            await ctx.RespondAsync("Você não tem role ou admin para usar esse comando"); 
                            return; 
                        }
                    }
                }
                else
                {
                    if (!ctx.Member.Permissions.HasPermission(Permissions.Administrator))
                    { await ctx.RespondAsync("Você não tem admin para usar esse comando"); return; }
                }
            }

            if (string.IsNullOrEmpty(streamers)) { await ctx.RespondAsync("O comando não pode ser usado sem especificar os canais twitch"); return; }
            if (Twitch == null) { Twitch = new(); }

            TwitchChannels final = new();
            
            try
            {
                if (streamers.Contains('#'))
                {
                    var list = await TwitchChannels.GetSplitChannels(streamers, Twitch);
                    final.Channels.AddRange(list);
                }
                else
                {
                    var streamer = await Twitch.GetChannelAsync(streamers);
                    if (streamer is not null)
                    {
                        final.Channels.Add(streamers.ToLower());
                    }
                }
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync(ex.Message + ex.StackTrace);   
            }

            string strList = string.Empty;

            try
            {
                var chn = ctx.Guild.Channels.Values.SingleOrDefault(x => x.Name == "twitch"); 
                
                if (chn is null)
                {
                    var channel = await ctx.Guild.CreateChannelAsync("twitch", ChannelType.Text, null);
                    await channel.SendMessageAsync("Canal criado com sucesso!");
                }

                await TwitchChannels.SaveProfile(final, ctx.Guild.Id.ToString());
                await TwitchChannels.SaveMainFile(final);
                
                var msg = new DiscordMessageBuilder()
                    .WithContent("Streamer adicionado com sucesso!")
                    .WithReply(ctx.Message.Id, true);

                await msg.SendAsync(ctx.Channel);
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync(ex.Message + ex.StackTrace);   
            }

        }
    }
}