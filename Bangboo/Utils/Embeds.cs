using NetCord;
using NetCord.Rest;

namespace Bangboo.Utils;

public class Embed
{
    public static EmbedProperties Res(string message, int color)
    {
        return new EmbedProperties()
        {
            Description =  message,
            Color = new Color(color)
        };
    }
}