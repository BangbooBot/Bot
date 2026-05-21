using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Bangboo.Utils;
using Embed = Bangboo.Utils.Embed;

public class AgeModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly Constants _constants;
    
    public AgeModule(Constants constants)
    {
        _constants = constants;
    }
    
    [SlashCommand("age", "Displays your or another user's account creation date", Contexts = [InteractionContextType.Guild])]
    public async Task Age(
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
        
        var embed = Embed.Res(ageMessage, _constants.Colors.Green);
        
        await Context.Interaction.ModifyResponseAsync(message =>
            message.AddEmbeds([embed])
        );
    }
}