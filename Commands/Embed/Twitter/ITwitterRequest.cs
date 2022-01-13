using DSharpPlus.Entities;

namespace DiscordBot.Commands.Embed.Twitter
{
    public interface ITwitterRequest
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