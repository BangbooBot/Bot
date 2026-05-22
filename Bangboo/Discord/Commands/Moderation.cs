using Bangboo.Menus.Modals;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Bangboo.Utils;
using EmbedBuilder = Bangboo.Utils.EmbedBuilder;

public enum ModAction
{
    [SlashCommandChoice(Name = "delete dessage")]
    DeleteMessage,
    [SlashCommandChoice(Name = "timeout")]
    Timeout,
    [SlashCommandChoice(Name = "kick")]
    Kick,
    [SlashCommandChoice(Name = "ban")]
    Ban,
    [SlashCommandChoice(Name = "unban")]
    Unban
}

public class ModerationModule: ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly Constants _constants;
    
    public ModerationModule(Constants constants)
    {
        _constants = constants;
    }
    
    [SlashCommand("moderate", "Equality before the law is the cornerstone of justice ⚖.", Contexts = [InteractionContextType.Guild])]
    public async Task Moderate(
        ModAction action
    )
    {
        //var guildUser = Context.User as GuildUser;
        if (Context.Guild.OwnerId !=  Context.User.Id)
        {
            var embed = EmbedBuilder.Res("You are not a mod or the owner of the guild.", _constants.Colors.Danger);
            await Context.Interaction.SendResponseAsync(
                InteractionCallback.Message(new(){ Embeds = [embed] })
                );
            return;
        }
        
        switch (action)
        {
            case ModAction.DeleteMessage: {
                await Context.Interaction.SendResponseAsync(
                    InteractionCallback.Modal(ModerationMenus.DeleteMessageMenu())
                );
            }
                break;
            case ModAction.Timeout:
                await Context.Interaction.SendResponseAsync(
                    InteractionCallback.Modal(ModerationMenus.TimeoutMenu())
                );
                break;
            case ModAction.Kick: {
                await Context.Interaction.SendResponseAsync(
                    InteractionCallback.Modal(ModerationMenus.KickMenu())
                );
            }
                break;
            case ModAction.Ban: {
                await Context.Interaction.SendResponseAsync(
                    InteractionCallback.Modal(ModerationMenus.BanMenu())
                );
            }
                break;
            case ModAction.Unban: {
                var bannedMembers = await Context.Guild.GetBansAsync(new() { BatchSize = 25 }).ToListAsync();
                if (bannedMembers.Count == 0)
                {
                    var embed = EmbedBuilder.Res("There is no banned members in this guild", _constants.Colors.Danger);
                    await Context.Interaction.SendResponseAsync(
                        InteractionCallback.Message(new(){ Embeds = [embed] })
                    );
                }
                await Context.Interaction.SendResponseAsync(
                    InteractionCallback.Modal(ModerationMenus.UnbanMenu(bannedMembers))
                );
            }
                break;
            default:
                break;
        }
    }
}