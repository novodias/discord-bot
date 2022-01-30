using DSharpPlus;
using DiscordBot.MonitorTwitch;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DiscordBot.Commands.Embed.Twitch
{
    public class MonitorRoles
    {
        // private MonitorRolesEnum Operation;
        private readonly DiscordClient client;
        public MonitorRoles(DiscordClient client)
        {
            this.client = client;
        }
        
        public async Task SubCommand(DiscordChannel channel, string operation, string guildid, DiscordRole? role = null)
        {
            int option = 3;
            operation = operation.ToLower();

            if (operation == "add") { option = (int) MonitorEnum.Add; }
            if (operation == "remove") { option = (int) MonitorEnum.Remove; }
            if (operation == "roles") { option = (int) MonitorEnum.Roles; }
            if (operation == "help") { option = (int) MonitorEnum.Help; }

            switch (option)
            {
                case (int)MonitorEnum.Add:
                    if ( role is null ) { await this.client.SendMessageAsync(channel, "??? cade o role caraio"); return; }
                    await this.AddRole(channel, guildid, role);
                break;

                case (int)MonitorEnum.Remove:
                    if ( role is null ) { await this.client.SendMessageAsync(channel, "??? cade o role caraio"); return; }
                    await this.RemoveRole(channel, guildid, role);
                break;

                case (int)MonitorEnum.Roles:
                    await this.ShowRoles(channel, guildid);
                break;

                case (int)MonitorEnum.Help:
                    await this.ShowHelp(channel, guildid);
                break;

                default:
                    await this.ShowHelp(channel, guildid);
                break;
            }
        }

        private async Task AddRole(DiscordChannel channel, string guildid, DiscordRole role)
        {
            var file = await TwitchChannels.GetContent(guildid);

            var msg = new DiscordMessageBuilder();

            if (file.Roles.Contains(role.Id.ToString()))
            {
                msg.Content = "O role mencionado já foi adicionado préviamente";
                await this.client.SendMessageAsync(channel, msg);
            }
            else
            {
                file.Roles.Add(role.Id.ToString());
                msg.Content = "Role adicionado com sucesso";
                await this.client.SendMessageAsync(channel, msg);
            }

            try
            {
                await TwitchChannels.SaveContent(file, guildid);
            }
            catch (Exception)
            {
                await this.client.SendMessageAsync(channel, "Não foi possível salvar");   
            }
        }

        private async Task RemoveRole(DiscordChannel channel, string guildid, DiscordRole role)
        {
            var file = await TwitchChannels.GetContent(guildid);

            var msg = new DiscordMessageBuilder();

            if (file.Roles.Contains(role.Id.ToString()))
            {
                file.Roles.Remove(role.Id.ToString());
                msg.Content = "Role removido com sucesso";
                await this.client.SendMessageAsync(channel, msg);
            }
            else
            {
                msg.Content = "O role mencionado já foi removido préviamente ou não existe";
                await this.client.SendMessageAsync(channel, msg);
            }

            try
            {
                // For some reason getting the FileStream
                // doesn't replace/remove the content of the file.
                // await TwitchChannels.SaveContent(file, guildid);

                var str = JsonConvert.SerializeObject(file);

                using ( var sw = new StreamWriter($"files/data/guilds/{guildid}/twitchchannels.json") )
                {
                    await sw.WriteLineAsync(str);

                    await sw.DisposeAsync();
                }
            }
            catch (Exception)
            {
                await this.client.SendMessageAsync(channel, "Não foi possível salvar");   
            }
        }

        private async Task ShowRoles(DiscordChannel channel, string guildid)
        {
            var file = await TwitchChannels.GetContent(guildid);
            var guild = await this.client.GetGuildAsync(Convert.ToUInt64(guildid));

            string msgContent = string.Empty;

            foreach (var item in file.Roles)
            {
                if (msgContent == string.Empty)
                {
                    msgContent += $"`{guild.GetRole(Convert.ToUInt64(item)).Name}`";
                }
                else
                {
                    msgContent += $", `{guild.GetRole(Convert.ToUInt64(item)).Name}`";
                }
            }

            var embed = new DiscordEmbedBuilder()
            {
                Title = "Roles",
                Color = DiscordColor.Green,
                Description = $"Todos os roles adicionados: {msgContent}"

            };

            if ( msgContent == string.Empty ) { embed.Description = "Vazio!"; }

            await this.client.SendMessageAsync(channel, embed);
        }

        private async Task ShowHelp(DiscordChannel channel, string guildid)
        {
            if (File.Exists($"files/data/guilds/{guildid}/twitchchannels.json"))
            {
                await this.client.SendMessageAsync(channel, "Sub-comandos disponíveis: `add`, `remove`, `roles` e `help`.");
            }
            else
            {
                await this.client.SendMessageAsync(channel, "Sub-comandos disponíveis: `add` e `help`.");
            }
        }
    }

    internal enum MonitorEnum
    {
        Add,
        Remove,
        Roles,
        Help
    }
}