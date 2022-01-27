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

        [Command("twitch")]
        public async Task TwitchCommand(CommandContext ctx, [RemainingText] string input)
        {
            await ctx.TriggerTypingAsync();

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

        [Command("monitor")]
        public async Task MonitorCommand(CommandContext ctx, [RemainingText] string streamers)
        {
            // Transformar em funções, muita linha de código
            await ctx.TriggerTypingAsync();

            if (Twitch == null) { Twitch = new(); }

            JsonChannels final = new();
            
            try
            {
                if (streamers.Contains('#'))
                {
                    var list = await JsonChannels.GetSplitChannels(streamers, Twitch);
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
                // string dir = $"files/data/guilds/{ctx.Guild.Id}";
                // string jsonfile = $"{dir}/twitchchannels.json";

                var chn = ctx.Guild.Channels.Values.SingleOrDefault(x => x.Name == "twitch"); 
                
                if (chn is null)
                {
                    var channel = await ctx.Guild.CreateChannelAsync("twitch", ChannelType.Text, null);
                    await channel.SendMessageAsync("Canal criado com sucesso!");
                }

                await JsonChannels.SaveProfile(final, ctx.Guild.Id.ToString());
                await JsonChannels.SaveMainFile(final);
                
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