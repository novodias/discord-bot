namespace DiscordBot.Interactivity.Embeds
{
    internal interface IEmbed
    {
        Task DoPaginationAsync(IEmbedRequest request);

        void Dispose();
    }
}