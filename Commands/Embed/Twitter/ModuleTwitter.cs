using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordBot.Commands.Embed.Twitter
{
    public class ModuleTwitter : BaseCommandModule
    {
        private TwitterSend? TwitterSend = null;

        [Command("twitter")]
        public async Task TwitterCommand(CommandContext ctx, [RemainingText] string strUser)
        {
            await ctx.TriggerTypingAsync();

            if ( TwitterSend == null ) { TwitterSend = new(ctx.Client); }

            try
            {
                var embeds = await SearchTwitter.GenerateEmbed(strUser);
                await TwitterSend.SendEmbedMessageAsync(ctx.Channel, ctx.User, embeds, TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync("Algum erro aconteceu ao criar embeds do usuário: " + ex.Message);
            }
        }
    }
}