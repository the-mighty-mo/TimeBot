using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using static TimeBot.DatabaseManager;

namespace TimeBot.Modules
{
    public class SetChannel : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("set-channel", "Sets the time channel")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetChannelAsync(SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                await SetChannelPrivAsync();
            }
            else
            {
                await SetChannelPrivAsync(channel);
            }
        }

        public async Task SetChannelPrivAsync()
        {
            if (await channelsDatabase.Channels.GetTimeChannelAsync(Context.Guild) == null)
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription("You already do not have a channel set.");

                await Context.Interaction.RespondAsync(embed: emb.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription("\"Time\" messages will no longer be sent.");

            await Task.WhenAll
            (
                channelsDatabase.Channels.RemoveTimeChannelAsync(Context.Guild),
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }

        public async Task SetChannelPrivAsync(SocketTextChannel channel)
        {
            if (await channelsDatabase.Channels.GetTimeChannelAsync(Context.Guild) == channel)
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{channel.Mention} is already configured for \"Time\" messages.");

                await Context.Interaction.RespondAsync(embed: emb.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription($"\"Time\" messages will now be sent to {channel.Mention}.");

            await Task.WhenAll
            (
                channelsDatabase.Channels.SetTimeChannelAsync(channel),
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }
    }
}