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
        public async Task MonitorRoleCommand(CommandContext ctx, string? operation = null, DiscordRole? role = null)
        {
            await ctx.TriggerTypingAsync();

            if (!ctx.Member.Permissions.HasPermission(Permissions.Administrator))
            { await ctx.RespondAsync("Você não tem permissão para executar esse comando."); return; }
            
            if ( MonitorRoles == null ) { MonitorRoles = new(ctx.Client); }

            MonitorEnum option = MonitorEnum.Help;

            if (operation == "add") { option = MonitorEnum.Add; }
            if (operation == "remove") { option = MonitorEnum.Remove; }
            if (operation == "roles") { option = MonitorEnum.Roles; }
            if (operation == "help") { option = MonitorEnum.Help; }
            
            await MonitorRoles.SubCommand(ctx.Channel, option, ctx.Guild.Id.ToString(), role);
        }

        [Command("monitor")]
        public async Task MonitorCommand(CommandContext ctx, [RemainingText] string streamers)
        {
            // Transformar em funções, muita linha de código
            await ctx.TriggerTypingAsync();

            if (File.Exists($"files/data/guilds/{ctx.Guild.Id}/twitchchannels.json"))
            {
                var file = await TwitchChannels.GetContent(ctx.Guild.Id.ToString());
                var msg = new DiscordMessageBuilder().WithReply(ctx.Message.Id, true);

                if ( file.Roles is not null )
                {
                    var query = from role in file.Roles
                                where ctx.Member.Roles.Contains(ctx.Guild.GetRole(Convert.ToUInt64(role)))
                                select role;

                    if (!query.Any())
                    {
                        if (!ctx.Member.Permissions.HasPermission(Permissions.Administrator))
                        {
                            // await ctx.RespondAsync("Você não tem role ou admin para usar esse comando"); 
                            msg.Content = "Você não tem role ou admin para usar esse comando";
                            await msg.SendAsync(ctx.Channel);
                            return; 
                        }
                    }
                }
                else
                {
                    if (!ctx.Member.Permissions.HasPermission(Permissions.Administrator))
                    { 
                        // await ctx.RespondAsync("Você não tem admin para usar esse comando"); 
                        msg.Content = "Você não tem admin para usar esse comando";
                        await msg.SendAsync(ctx.Channel);
                        return; 
                    }
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