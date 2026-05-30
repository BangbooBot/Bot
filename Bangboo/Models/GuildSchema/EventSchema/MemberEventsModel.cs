using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bangboo.Models;

[Table("member_events")]
public class MemberEventsModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("on_join")]
    [DefaultValue(false)]
    public bool OnJoin { get; set; } = false;
    
    [Column("on_leave")]
    [DefaultValue(false)]
    public bool OnLeave { get; set; } = false;
    
    [Column("on_ban")]
    [DefaultValue(false)]
    public bool OnBan { get; set; } = false;
    
    [Column("on_boost")]
    [DefaultValue(false)]
    public bool OnBoost { get; set; } = false;
    
    [Column("on_mute")]
    [DefaultValue(false)]
    public bool OnMute { get; set; } = false;
    
    [Column("system_channel_id")]
    public ulong? SystemChannelId { get; set; }
    
    [Column("boost_channel_id")]
    public ulong? BoostChannelId { get; set; }
    
    [Column("mod_channel_id")]
    public ulong? ModChannelId { get; set; }
    
    [Column("fk_guild_id")]
    public ulong FkGuildId { get; set; }

    public GuildsModel GuildModel = null!;
}