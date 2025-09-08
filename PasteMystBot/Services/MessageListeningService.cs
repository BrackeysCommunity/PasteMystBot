using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Hosting;
using PasteMystBot.Configuration;

namespace PasteMystBot.Services;

/// <summary>
///     Represents a service which listens for codeblocks in messages to auto-paste.
/// </summary>
internal sealed class MessageListeningService : BackgroundService
{
    private readonly DiscordClient _discordClient;
    private readonly ConfigurationService _configurationService;
    private readonly MessagePastingService _messagePastingService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageListeningService" /> class.
    /// </summary>
    /// <param name="discordClient">The Discord client.</param>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="messagePastingService">The message pasting service.</param>
    public MessageListeningService(DiscordClient discordClient,
        ConfigurationService configurationService,
        MessagePastingService messagePastingService)
    {
        _discordClient = discordClient;
        _configurationService = configurationService;
        _messagePastingService = messagePastingService;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discordClient.MessageCreated += DiscordClientOnMessageCreated;
        return Task.CompletedTask;
    }

    private async Task DiscordClientOnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        GuildConfiguration configuration = _configurationService.GetGuildConfiguration(e.Guild) ?? new GuildConfiguration();
        if (e.Author.IsBot && configuration.IgnoreBots)
        {
            return;
        }

        if (_messagePastingService.IsChannelExempt(e.Channel))
        {
            return;
        }

        await _messagePastingService.PasteMessageAsync(e.Message);
    }
}
