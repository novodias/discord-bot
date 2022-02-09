using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using ConcurrentCollections;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands.Game
{
    public class Trivia
    {
        readonly DiscordClient _client;
        readonly ConcurrentHashSet<TriviaRequest> _requests;
        public Trivia(DiscordClient client)
        {
            this._client = client;
            this._requests = new();

            client.ComponentInteractionCreated += HandleButtonInteraction;
        }

        private Task HandleButtonInteraction(DiscordClient client, ComponentInteractionCreateEventArgs e)
        {
            if ( this._requests.Count == 0 )
                return Task.CompletedTask;

            _ = Task.Run(async () => 
            {
                foreach (var req in this._requests)
                {
                    var msg = await req.GetMessageAsync().ConfigureAwait(false);
                    if ( msg is null ) { return; }

                    var answer = await req.GetTrueAnswer().ConfigureAwait(false);

                    if (e.Message.Id == msg.Id)
                    {
                        var component = e.Message.Components.Single().Components.Single(x => x.CustomId == answer);
                        if ( e.Id == component.CustomId )
                        {
                            var interac = new DiscordInteractionResponseBuilder().WithContent($"{e.User.Mention} acertou!");
                            interac.ClearComponents();

                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, interac);
                        }
                        else
                        {
                            var interac = new DiscordInteractionResponseBuilder().WithContent($"{e.User.Mention} Kaka errou");
                            interac.ClearComponents();

                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, interac);
                        }
                        
                        this._requests.TryRemove(req);
                    }
                    
                }

            });

            return Task.CompletedTask;
        }

        public async Task InitializeTask(DiscordChannel chn)
        {
            if (this._client is null) { throw new Exception("Trivia -> client cannot be null at Trivia.cs"); }

            var request = new TriviaRequest(chn);
            this._requests.Add(request);
            
            try
            {
                await this._requests.Single(x => x == request).Setup();
            }
            catch (System.Exception ex)
            {
                this._client.Logger.LogError(LoggerEvents.Misc, ex, "TriviaRequest got a error while setting up");
            }
        }
    }

    public class TriviaRequest
    {
        private readonly DiscordChannel _chn;
        private DiscordMessage? _botmsg;
        private string? _trueanswer;
        public TriviaRequest(DiscordChannel chn)
        {
            this._chn = chn;
        }

        public async Task Setup()
        {
            var json = string.Empty;
            using (var fsd = File.OpenRead("files/trivia.json"))
            using (var sr = new StreamReader(fsd, new System.Text.UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            var trivia = JsonConvert.DeserializeObject<TriviaRoot>(json);
            var rnd = new Random();

            var selected = trivia.Trivia.ElementAt(rnd.Next(0, trivia.Trivia.Count));
            int[] array = Enumerable.Range(0, 4).OrderBy(x => rnd.Next()).Take(4).ToArray();
            

            var buttonlst = new List<DiscordButtonComponent>() 
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "anwser_01", selected.Answers.ElementAt(array[0]), false ),
                new DiscordButtonComponent(ButtonStyle.Secondary, "anwser_02", selected.Answers.ElementAt(array[1]), false ),
                new DiscordButtonComponent(ButtonStyle.Secondary, "anwser_03", selected.Answers.ElementAt(array[2]), false ),
                new DiscordButtonComponent(ButtonStyle.Secondary, "anwser_04", selected.Answers.ElementAt(array[3]), false )
            };

            this._trueanswer = buttonlst.Single(x => x.Label == selected.Answer).CustomId;
            
            var builder = new DiscordMessageBuilder()
                .WithContent(selected.Question)
                .AddComponents(buttonlst);

            this._botmsg = await builder.SendAsync(_chn);
        }

        public async Task<DiscordMessage?> GetMessageAsync()
        {
            await Task.Yield();

            return this._botmsg;
        }

        public async Task<string?> GetTrueAnswer()
        {
            await Task.Yield();

            return this._trueanswer;
        }
    }

    public struct TriviaRoot
    {
        [JsonProperty("trivia")]
        public List<TriviaJson> Trivia;
    }
    
    public struct TriviaJson
    {
        [JsonProperty("question")]
        public string Question;

        [JsonProperty("answers")]
        public List<string> Answers;

        [JsonProperty("trueanswer")]
        public string Answer;
    }
}