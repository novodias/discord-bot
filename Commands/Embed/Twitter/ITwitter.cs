namespace DiscordBot.Commands.Embed.Twitter
{
    internal interface ITwitter
    {
        Task DoPaginationAsync(ITwitterRequest request);

        void Dispose();
    }
}