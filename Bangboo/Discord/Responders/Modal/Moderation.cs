using Bangboo.Utils;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace Bangboo.Discord.Responders.Modal;

public class ModerationResponderModule : ComponentInteractionModule<ModalInteractionContext>
{
    private readonly Constants _constants;

    public ModerationResponderModule(Constants constants)
    {
        _constants = constants;
    }
    
    [ComponentInteraction("modal/moderate/delete-message")]
    public async Task DeleteMessage()
    {
        var selectedUsers = Context.Components
            .OfType<Label>()
            .Select(l => l.Component)
            .OfType<UserMenu>()
            .First(c => c.CustomId == "delete-message-users")
            .SelectedValues;

        var limit = Context.Components
            .OfType<Label>()
            .Select(l => l.Component)
            .OfType<TextInput>()
            .First(c => c.CustomId == "delete-message-limit")
            .Value;
        
        if (selectedUsers == null)
        {
            await Context.Interaction.SendResponseAsync(
                InteractionCallback.Message(new() { Embeds = [EmbedBuilder.Res("Failed to get selected users input field values", _constants.Colors.Danger)] })
            );
            return;
        }

        if (!int.TryParse(limit, out int limitPaginaton) && limitPaginaton < 1)
        {
            InteractionCallback.Message(new()
                { Embeds = [EmbedBuilder.Res("Failed to parse limit input field", _constants.Colors.Danger)] });
            return;
        }

        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        var messages = await Context.Interaction.Channel
            .GetMessagesAsync(new() { BatchSize = limitPaginaton })
            .ToListAsync();
        
        var filteredMessages = messages.Where(m => selectedUsers.Any(u => u.Id == m.Author.Id));
        
        var successList = new List<string>();
        var failList = new List<string>();
        try
        {
            await Context.Interaction.Channel.DeleteMessagesAsync(filteredMessages.Select(m => m.Id));
            foreach (var selectedUser in selectedUsers)
            {
                successList.Add($"<@{selectedUser.Id}>");
            }
            
        }
        catch (RestException ex)
        {
            foreach (var selectedUser in selectedUsers)
            {
                failList.Add($"<@{selectedUser.Id}>");
            }
        }

        var embed = EmbedBuilder.OfficerCuiAction(Context.User, "Delete message", successList, failList);

        await Context.Interaction.ModifyResponseAsync(message =>
        {
            message.AddEmbeds([embed]);
            message.WithFlags(MessageFlags.Ephemeral);
        });
    }

    [ComponentInteraction("modal/moderate/timeout")]
    public async Task Timeout()
    {
        var selectedUsers = Context.Components
                                .OfType<Label>()
                                .Select(l => l.Component)
                                .OfType<UserMenu>()
                                .First(c => c.CustomId == "timeout-users")
                                .SelectedValues;

        var timeout = Context.Components
                            .OfType<Label>()
                            .Select(l => l.Component)
                            .OfType<StringMenu>()
                            .First(c => c.CustomId == "timeout-duration")
                            .SelectedValues
                            .First();

        var reason = Context.Components
                            .OfType<Label>()
                            .Select(l => l.Component)
                            .OfType<TextInput>()
                            .First(c => c.CustomId == "timeout-reason")
                            .Value;
        
        if (selectedUsers == null || timeout == null)
        {
            await Context.Interaction.SendResponseAsync(
                InteractionCallback.Message(new() { Embeds = [EmbedBuilder.Res("Failed to get input field values", _constants.Colors.Danger)] })
            );
            return;
        }

        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        var filteredUsers = selectedUsers.Where(u => u.Id != Context.Guild.OwnerId);
        if (!double.TryParse(timeout, out double timeoutSeconds))
        {
            await Context.Interaction.SendResponseAsync(
                InteractionCallback.Message(new() { Embeds = [EmbedBuilder.Res("Failed to convert timeout value to seconds", _constants.Colors.Danger)] })
            );
            return;
        }
        var timeoutUntil = DateTimeOffset.Now.AddSeconds(timeoutSeconds);

        var successList = new List<string>();
        var failList = new List<string>();
        foreach (var u in filteredUsers)
        {
            try
            {
                await Context.Guild.ModifyUserAsync(u.Id, properties =>
                {
                    properties.WithTimeOutUntil(timeoutUntil);
                },
                new RestRequestProperties
                {
                    AuditLogReason = string.IsNullOrWhiteSpace(reason) ? $"${Context.User.GlobalName ?? Context.User.Username} apply timeout via Bangboo" : reason
                });
                successList.Add($"<@{u.Id}>");
            }
            catch (RestException ex)
            {
                failList.Add($"<@{u.Id}>");
            }
        }

        var embed = EmbedBuilder.OfficerCuiAction(Context.User, "Timeout", successList, failList, reason);

        await Context.Interaction.ModifyResponseAsync(message =>
        {
            message.AddEmbeds([embed]);
            message.WithFlags(MessageFlags.Ephemeral);
        });
    }
    
    [ComponentInteraction("modal/moderate/kick")]
    public async Task Kick()
    {
        var selectedUsers = Context.Components
                                .OfType<Label>()
                                .Select(l => l.Component)
                                .OfType<UserMenu>()
                                .First(c => c.CustomId == "kick-users")
                                .SelectedValues;

        var reason = Context.Components
                            .OfType<Label>()
                            .Select(l => l.Component)
                            .OfType<TextInput>()
                            .First(c => c.CustomId == "kick-reason")
                            .Value;
        
        if (selectedUsers == null)
        {
            await Context.Interaction.SendResponseAsync(
                InteractionCallback.Message(new() { Embeds = [EmbedBuilder.Res("Failed to get selected users input field values", _constants.Colors.Danger)] })
            );
            return;
        }

        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        var filteredUsers = selectedUsers.Where(u => u.Id != Context.Guild.OwnerId);

        var successList = new List<string>();
        var failList = new List<string>();
        foreach (var u in filteredUsers)
        {
            try
            {
                await Context.Guild.KickUserAsync(u.Id, new()
                {
                    AuditLogReason = string.IsNullOrWhiteSpace(reason)
                        ? $"${Context.User.GlobalName ?? Context.User.Username} apply kick via Bangboo"
                        : reason
                });
                successList.Add($"<@{u.Id}>");
            }
            catch (RestException ex)
            {
                failList.Add($"<@{u.Id}>");
            }
        }

        var embed = EmbedBuilder.OfficerCuiAction(Context.User, "Kick", successList, failList, reason);

        await Context.Interaction.ModifyResponseAsync(message =>
        {
            message.AddEmbeds([embed]);
            message.WithFlags(MessageFlags.Ephemeral);
        });
    }
    
    [ComponentInteraction("modal/moderate/ban")]
    public async Task Ban()
    {
        var selectedUsers = Context.Components
                                .OfType<Label>()
                                .Select(l => l.Component)
                                .OfType<UserMenu>()
                                .First(c => c.CustomId == "ban-users")
                                .SelectedValues;

        var reason = Context.Components
                            .OfType<Label>()
                            .Select(l => l.Component)
                            .OfType<TextInput>()
                            .First(c => c.CustomId == "ban-reason")
                            .Value;
        
        if (selectedUsers == null)
        {
            await Context.Interaction.SendResponseAsync(
                InteractionCallback.Message(new() { Embeds = [EmbedBuilder.Res("Failed to get selected users input field values", _constants.Colors.Danger)] })
            );
            return;
        }

        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        var filteredUsers = selectedUsers.Where(u => u.Id != Context.Guild.OwnerId);

        var successList = new List<string>();
        var failList = new List<string>();
        try
        {
            await Context.Guild.BanUsersAsync(filteredUsers.Select(u => u.Id), 604800, new()
            {
                AuditLogReason = string.IsNullOrWhiteSpace(reason)
                    ? $"${Context.User.GlobalName ?? Context.User.Username} apply ban via Bangboo"
                    : reason
            });
            foreach (var filteredUser in filteredUsers)
            {
                successList.Add($"<@{filteredUser.Id}>");
            }
        }
        catch (RestException ex)
        {
            foreach (var filteredUser in filteredUsers)
            {
                failList.Add($"<@{filteredUser.Id}>");
            }
        }

        var embed = EmbedBuilder.OfficerCuiAction(Context.User, "Ban", successList, failList, reason);

        await Context.Interaction.ModifyResponseAsync(message =>
        {
            message.AddEmbeds([embed]);
            message.WithFlags(MessageFlags.Ephemeral);
        });
    }
    
    [ComponentInteraction("modal/moderate/unban")]
    public async Task Unban()
    {
        var selectedUsers = Context.Components
                                .OfType<Label>()
                                .Select(l => l.Component)
                                .OfType<UserMenu>()
                                .First(c => c.CustomId == "unban-users")
                                .SelectedValues;
        
        if (selectedUsers == null)
        {
            await Context.Interaction.SendResponseAsync(
                InteractionCallback.Message(new() { Embeds = [EmbedBuilder.Res("Failed to get selected users input field values", _constants.Colors.Danger)] })
            );
            return;
        }

        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        var successList = new List<string>();
        var failList = new List<string>();
        foreach (var u in selectedUsers)
        {
            try
            {
                await Context.Guild.UnbanUserAsync(u.Id);
                successList.Add($"<@{u.Id}>");
            }
            catch (RestException ex)
            {
                failList.Add($"<@{u.Id}>");
            }
        }

        var embed = EmbedBuilder.OfficerCuiAction(Context.User, "Unban", successList, failList);

        await Context.Interaction.ModifyResponseAsync(message =>
        {
            message.AddEmbeds([embed]);
            message.WithFlags(MessageFlags.Ephemeral);
        });
    }
}