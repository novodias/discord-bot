using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using ConcurrentCollections;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Interactivity.Games
{
    public class Trivia : IGame
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

        public async Task InitializeTask(params object[] args)
        {
            if (this._client is null) { throw new Exception("Trivia -> client cannot be null at Trivia.cs"); }

            var chn = (DiscordChannel)args[0];

            var request = new TriviaRequest(chn, TimeSpan.FromSeconds(30));
            this._requests.Add(request);
            
            try
            {
                var tcs = await request.GetTaskCompletionSourceAsync().ConfigureAwait(false);
                await this._requests.Single(x => x == request).Setup();
                
                await tcs.Task.ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                this._client.Logger.LogError(LoggerEvents.Misc, ex, "TriviaRequest got a error while setting up");
            }
            finally
            {
                // If the trivia got an interaction, it should be removed,
                // otherwise will be removed here.
                this._requests.TryRemove(request);
                this._client.Logger.LogDebug(LoggerEvents.Misc, "Trivia Request removed");
            }
        }
    }

    public class TriviaRequest
    {
        private TaskCompletionSource<bool> _tcs;
        private readonly CancellationTokenSource _ct;
        private readonly DiscordChannel _chn;
        private DiscordMessage? _botmsg;
        private readonly TimeSpan _timeout;
        private string? _trueanswer;

        public TriviaRequest(DiscordChannel chn, TimeSpan timeout)
        {
            this._tcs = new();
            this._ct = new(timeout);
            this._ct.Token.Register( () => this._tcs.TrySetResult(true));
            this._timeout = timeout;

            this._chn = chn;
        }

        public async Task Setup()
        {
            string result;

            using (var http = new HttpClient())
            {
                using (var response = await http.GetAsync("https://opentdb.com/api.php?amount=1&type=multiple"))
                {
                    response.EnsureSuccessStatusCode();

                    var contentresponse = await response.Content.ReadAsStreamAsync();

                    result = await new StreamReader(contentresponse).ReadToEndAsync();
                }
            }

            var trivia = JsonConvert.DeserializeObject<OpenTDB>(result);
            var content = trivia.Results.Single();
            content = Decode(trivia);
            var rnd = new Random();

            int[] array = Enumerable.Range(0, 4).OrderBy(x => rnd.Next()).Take(4).ToArray();
            var questions = new List<string>();

            questions.AddRange(content.IncorrectAnswers);
            questions.Add(content.CorrectAnswer);

            var buttonlst = new List<DiscordButtonComponent>() 
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "anwser_01", "A", false ),
                new DiscordButtonComponent(ButtonStyle.Secondary, "anwser_02", "B", false ),
                new DiscordButtonComponent(ButtonStyle.Secondary, "anwser_03", "C", false ),
                new DiscordButtonComponent(ButtonStyle.Secondary, "anwser_04", "D", false )
            };

            DiscordColor color;
            string difficulty;

            switch (content.Difficulty.ToLower())
            {
                case "easy":
                    color = DiscordColor.Green;
                    difficulty = "Easy";
                break;
                case "medium":
                    color = DiscordColor.Orange;
                    difficulty = "Medium";
                break;
                case "hard":
                    color = DiscordColor.Red;
                    difficulty = "Hard";
                break;

                default:
                    color = DiscordColor.White;
                    difficulty = "Unknown";
                break;
            }

            var author = new DiscordEmbedBuilder.EmbedAuthor()
            {
                Name = "OpenTDB",
                Url = "https://opentdb.com/"
            };

            var footer = new DiscordEmbedBuilder.EmbedFooter()
            {
                Text = "30 seconds to answer the question!"
            };

            var embed = new DiscordEmbedBuilder()
            {
                Author = author,
                Title = content.Question,
                Color = color,
                Description = $"Difficulty: `{difficulty}` | Category: `{content.Category}`",
                Footer = footer
            };

            embed.AddField("A.", questions.ElementAt(array[0]), false);
            embed.AddField("B.", questions.ElementAt(array[1]), false);
            embed.AddField("C.", questions.ElementAt(array[2]), false);
            embed.AddField("D.", questions.ElementAt(array[3]), false);

            this._trueanswer = buttonlst.Single(x => x.Label == embed.Fields.Single(x => x.Value == content.CorrectAnswer).Name.First().ToString()).CustomId;
            
            var builder = new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(buttonlst);

            this._botmsg = await builder.SendAsync(_chn);
        }

        private static TriviaJson Decode(OpenTDB content)
        {
            var trivia = content.Results.Single();

            trivia.CorrectAnswer = System.Web.HttpUtility.HtmlDecode(trivia.CorrectAnswer);
            List<string> temp = new();
            foreach (var i in trivia.IncorrectAnswers)
            {
                temp.Add(System.Web.HttpUtility.HtmlDecode(i));
            }
            trivia.IncorrectAnswers = temp;
            trivia.Question = System.Web.HttpUtility.HtmlDecode(trivia.Question);
            
            return trivia;
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

        public async Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync()
        {
            await Task.Yield();

            return this._tcs;
        }

        ~TriviaRequest()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this._ct.Dispose();
            // this._tcs = null;
            this._botmsg = null;
            this._trueanswer = null;
        }
    }

    public struct OpenTDB
    {
        [JsonProperty("response_code")]
        public int ResponseCode;

        [JsonProperty("results")]
        public IEnumerable<TriviaJson> Results;
    }
    
    public struct TriviaJson
    {
        [JsonProperty("category")]
        public string Category;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("difficulty")]
        public string Difficulty;

        [JsonProperty("question")]
        public string Question;

        [JsonProperty("correct_answer")]
        public string CorrectAnswer;

        [JsonProperty("incorrect_answers")]
        public IEnumerable<string> IncorrectAnswers;
    }
}