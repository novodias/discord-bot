using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace DiscordBot.Commands.Game
{
    public class ModuleJokenpo : BaseCommandModule
    {
        private Jokenpo? Jokenpo;
        [Command("jokenpo")]
        public async Task JokenpoCommand(CommandContext ctx, DiscordUser opponent)
        {
            await ctx.TriggerTypingAsync();

            if ( Jokenpo is null ) { Jokenpo = new(ctx.Client); }

            // var interactivity = ctx.Client.GetInteractivity();
            var user_one = ctx.Member;
            var user_two = await ctx.Guild.GetMemberAsync(opponent.Id);

            await Jokenpo.InitializeTask(user_one, user_two, ctx.Channel);

            // if ( user_one is null || user_two is null ) { return; }
            // if ( user_one == user_two ) { return; }

            // var botmsg = await ctx.RespondAsync("Esperando o resultado...");

            // var scissors = DiscordEmoji.FromUnicode("✂️");
            // var rock = DiscordEmoji.FromUnicode("🪨");
            // var paper = DiscordEmoji.FromUnicode("📰");

            // var m = new DiscordMessageBuilder()
            //     .WithContent("Escolha entre papel, pedra ou tesoura");

            // var msgone = await user_one.SendMessageAsync(m);
            // var msgtwo = await user_two.SendMessageAsync(m);

            // await msgone.CreateReactionAsync(scissors);
            // await msgone.CreateReactionAsync(rock);
            // await msgone.CreateReactionAsync(paper);

            // await msgtwo.CreateReactionAsync(scissors);
            // await msgtwo.CreateReactionAsync(rock);
            // await msgtwo.CreateReactionAsync(paper);

            // var emojione = await interactivity.WaitForReactionAsync(msgone, user_one, TimeSpan.FromSeconds(10));
            // var emojitwo = await interactivity.WaitForReactionAsync(msgtwo, user_two, TimeSpan.FromSeconds(10));

            // if (emojione.TimedOut && emojione.Result.Emoji is null && emojitwo.Result.Emoji is not null)
            //     await botmsg.ModifyAsync($"{user_two.Mention} venceu!");

            // else if (emojitwo.TimedOut && emojitwo.Result.Emoji is null && emojitwo.Result.Emoji is not null)
            //     await botmsg.ModifyAsync($"{user_one.Mention} venceu!");
                
            // else if (emojione.TimedOut && emojione.Result.Emoji is null &&
            //         emojitwo.TimedOut && emojitwo.Result.Emoji is null ) { await botmsg.ModifyAsync($"Empatou por que ninguém reagiu a mensagem"); }
            
            // if (emojione.TimedOut && emojione.Result.Emoji == scissors && emojitwo.Result.Emoji == paper)
            // {
            //     await botmsg.ModifyAsync($"{user_one.Mention} venceu!");
            // }
            // else if (emojione.TimedOut && emojione.Result.Emoji == rock && emojitwo.Result.Emoji == scissors)
            // {
            //     await botmsg.ModifyAsync($"{user_one.Mention} venceu!");
            // }
            // else if (emojione.TimedOut && emojione.Result.Emoji == paper && emojitwo.Result.Emoji == rock)
            // {
            //     await botmsg.ModifyAsync($"{user_one.Mention} venceu!");
            // }
            // else if (emojione.Result.Emoji == emojitwo.Result.Emoji) { await botmsg.ModifyAsync("Empate!"); }
            // else
            // {
            //     await botmsg.ModifyAsync($"{user_two.Mention} venceu!");
            // }
        }
    }
}