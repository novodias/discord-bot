using DSharpPlus.Entities;

namespace DiscordBot.Interactivity.Embeds
{
    public interface IEmbedRequest
    {
        int EmbedCount { get; }

        Task<DiscordEmbed> GetEmbedAsync();

        Task NextEmbedAsync();

        Task PreviousEmbedAsync();

        Task<PaginationEmojis> GetEmojisAsync();

        Task<DiscordMessage> GetMessageAsync();

        Task<DiscordUser> GetUserAsync();

        Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync();
    }
}