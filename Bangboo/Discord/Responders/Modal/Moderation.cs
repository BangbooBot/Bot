using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace Bangboo.Discord.Responders.Modal;

public class ModerationResponderModule : ComponentInteractionModule<ModalInteractionContext>
{
    [ComponentInteraction("moderete-timeout")]
    public async Task Timeout()
    {
        // Responder ao modal
        var response = new InteractionMessageProperties()
        {
            Content = "Timeout modal received!"
        };
        
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message(response)
        );
    }
}