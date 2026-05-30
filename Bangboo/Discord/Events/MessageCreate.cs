using Bangboo.Discord.Services;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Bangboo.Discord.Events;

public class MessageCreateHandler : IMessageCreateGatewayHandler
{
    private readonly ILogger<MessageCreateHandler> _logger;
    private readonly GlobalService _globalService;

    public MessageCreateHandler(ILogger<MessageCreateHandler> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        var scope = scopeFactory.CreateScope();
        _globalService = scope.ServiceProvider.GetRequiredService<GlobalService>();
    }

    public async ValueTask HandleAsync(Message arg)
    {
        if (arg.Author.IsBot) return;
        if (arg.GuildId is null) return;
        
        // Mousetrap check
        //if (await _automodService.Mousetrap(arg))
            //return;
/*
        var dbCtx = _automodService.dbContext;
        var gid = arg.Guild.Id;
        var guild = await dbCtx.Guilds.Where(g => g.Id == gid).FirstOrDefaultAsync();
        if (guild is null)
            return;
        var author = arg.Author;
        var mid = author.Id;
        var member = await dbCtx.Members.Where(m => m.FkUserId == mid && m.FkGuildId == gid).FirstOrDefaultAsync();
        if (member is null)
        {
            var uid = await dbCtx.Users.Where(u => u.Id == mid).GroupBy(u => u.Id).FirstOrDefaultAsync();
            if (uid is null)
            {
                var userModel = new UsersModel
                {
                    Id = mid,
                    Username = author.Username,
                    Avatar = author.DefaultAvatarUrl.ToString()
                };
                await dbCtx.Users.AddAsync(userModel);
            }

            var memberModel = new MembersModel
            {
                Xp = guild.XpPerMessage
            };
            await dbCtx.Members.AddAsync(memberModel);
        }
        await dbCtx.Members
            .Where(m => m.FkUserId == mid && m.FkGuildId == gid)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.Xp, m => m.Xp + guild.XpPerMessage));
        
        await dbCtx.SaveChangesAsync();
    */
    }
}