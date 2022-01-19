using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DiscordBot.Interactivity.Embeds;

namespace DiscordBot.Commands.Embed.Twitter
{
    public class ModuleTwitter : BaseCommandModule
    {
        private EmbedSend? EmbedSend = null;

        [Command("twitter")]
        public async Task TwitterCommand(CommandContext ctx, [RemainingText] string strUser)
        {
            await ctx.TriggerTypingAsync();

            if ( EmbedSend == null ) { EmbedSend = new(ctx.Client); }

            try
            {
                var embeds = await SearchTwitter.GenerateEmbed(strUser);
                try
                {
                    await EmbedSend.SendEmbedMessageAsync(ctx.Channel, ctx.User, embeds, TimeSpan.FromSeconds(30));
                }
                catch (Exception)
                {
                    await ctx.RespondAsync("Algum erro aconteceu ao enviar a mensagem.");   
                }
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync(ex.Message);
            }
        }
    }
}