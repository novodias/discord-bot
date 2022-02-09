using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus;
using Newtonsoft.Json;

namespace DiscordBot.Commands.Game
{
    public class ModuleTrivia : BaseCommandModule
    {
        [Command("trivia")]
        public async Task TriviaCommand(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var interac = ctx.Client.GetInteractivity();

            try
            {
                var json = string.Empty;
                using (var fsd = File.OpenRead("files/trivia.json"))
                using (var sr = new StreamReader(fsd, new System.Text.UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync();

                var trivia = JsonConvert.DeserializeObject<TriviaJson>(json);
                var rnd = new Random();

                var selected = trivia.Trivia.ElementAt(rnd.Next(0, trivia.Trivia.Count));
                int[] array = Enumerable.Range(0, 4).OrderBy(x => rnd.Next()).Take(4).ToArray();
                
                var button1 = new DiscordButtonComponent(ButtonStyle.Secondary, "anwser_01", selected.Answers.ElementAt(array[0]), false );
                var button2 = new DiscordButtonComponent(ButtonStyle.Secondary, "anwser_02", selected.Answers.ElementAt(array[1]), false );
                var button3 = new DiscordButtonComponent(ButtonStyle.Secondary, "anwser_03", selected.Answers.ElementAt(array[2]), false );
                var button4 = new DiscordButtonComponent(ButtonStyle.Secondary, "anwser_04", selected.Answers.ElementAt(array[3]), false );

                var msgbuilder = new DiscordMessageBuilder()
                    .WithContent(selected.Question)
                    .AddComponents(button1, button2, button3, button4);

                List<DiscordButtonComponent> buttons = new() { button1, button2, button3, button4 };

                var msg = await msgbuilder.SendAsync(ctx.Channel);
                
                var buttonwait = await interac.WaitForButtonAsync(msg, buttons, TimeSpan.FromSeconds(10));
                bool on = true;

                while (on)
                {
                    if (buttonwait.Result != null)
                    {
                        on = false;
                    }
                    else if (buttonwait.TimedOut)
                    {
                        on = false;
                    }
                }

                if (buttonwait.Result == null) { await msg.ModifyAsync(msg.Content + " Timer ended!"); }
                else if (buttonwait.Result != null)
                {
                    var buttonanswer = buttons.Single(x => x.Label == selected.Answer);
                    if (buttonanswer.CustomId == buttonwait.Result.Id)
                    {
                        await ctx.RespondAsync("certa resposta");
                    }
                    else
                    {
                        await ctx.RespondAsync("kaka errou");
                    }
                }
            }
            catch (System.Exception ex)
            {
                await ctx.RespondAsync("bruh " + ex.Message + ex.StackTrace);   
            }
        }
    }

    public struct TriviaJson
    {
        [JsonProperty("trivia")]
        public List<Trivia> Trivia;
    }
    public struct Trivia
    {
        [JsonProperty("question")]
        public string Question;

        [JsonProperty("answers")]
        public List<string> Answers;

        [JsonProperty("trueanswer")]
        public string Answer;
    }
}