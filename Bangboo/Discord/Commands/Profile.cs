using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Bangboo.Utils;
using EmbedBuilder = Bangboo.Utils.EmbedBuilder;

public class ProfileModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly Constants _constants;
    
    public ProfileModule(Constants constants)
    {
        _constants = constants;
    }
    
    [SlashCommand("profile", "Displays your or another member's info from guild", Contexts = [InteractionContextType.Guild])]
    public async Task Profile(
        [SlashCommandParameter(Name = "user", Description = "Selected user")] User? user = null
        )
    {
        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
        
        user ??= Context.User;

        long unixTimestamp = user.CreatedAt.ToUnixTimeSeconds();

        string ageMessage;
        if (Context.Interaction.UserLocale== "pt-BR")
        {
            ageMessage = $"**{user.Username}** criou a conta <t:{unixTimestamp}:R> em um(a) <t:{unixTimestamp}:F>";
        }
        else
        {
            ageMessage = $"**{user.Username}**'s account was created <t:{unixTimestamp}:R> on <t:{unixTimestamp}:F>";
        }
        
        var embed = EmbedBuilder.Res(ageMessage, _constants.Colors.Green);
        
        await Context.Interaction.ModifyResponseAsync(message =>
            message.AddEmbeds([embed])
        );
    }
}