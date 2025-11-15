use crate::constants::*;
use crate::functions::*;
use skia_safe::{EncodedImageFormat, ISize, Point};
use std::sync::Arc;
use std::time::{SystemTime, UNIX_EPOCH};
use twilight_gateway::EventType;
use twilight_http::Client;
use twilight_model::guild::Member;
use twilight_model::http::attachment::Attachment;
use twilight_model::id::marker::ChannelMarker;
use twilight_model::id::Id;
use twilight_model::user::User;
use twilight_util::snowflake::Snowflake;

pub async fn global_message(
    http: Arc<Client>,
    channel_id: &Id<ChannelMarker>,
    event: EventType,
    member: Option<&Member>,
    user: &User,
) {
    // Fetch avatar
    let mut user_avatar: Vec<u8> = vec![];

    if let Some(avatar_url) = display_avatar_url(&user, 512) {
        if let Ok(res) = reqwest::get(avatar_url).await {
            if let Ok(bytes) = res.bytes().await {
                user_avatar = bytes.to_vec();
            }
        }
        if user_avatar.is_empty() {
            user_avatar = IMG_DEFAULT_AVATAR.to_vec();
        }
    } else {
        user_avatar = IMG_DEFAULT_AVATAR.to_vec();
    }

    let background = match event {
        EventType::MemberAdd => {
            let date = match SystemTime::now().duration_since(UNIX_EPOCH) {
                Ok(duration) => duration.as_millis(),
                Err(_) => 0,
            };
            let join_age = match member.unwrap().joined_at {
                Some(joined_at) => date - (joined_at.as_micros() / 1000) as u128,
                None => 0,
            };
            let account_age = date - user.id.timestamp() as u128;
            const TIME_LIMIT: u128 = 300 * 1000;
            if join_age < TIME_LIMIT {
                CARD_NEW
            } else if account_age < TIME_LIMIT {
                CARD_NEW
            } else {
                CARD_BACK
            }
        }
        EventType::MemberRemove => CARD_LEFT,
        EventType::BanAdd => CARD_MOD,
        _ => CARD_LEFT,
    };

    let mut data = vec![];
    // Scope to fix safe issue with skia
    {
        let mut surface = match skia_safe::surfaces::raster_n32_premul(ISize {
            width: 2800,
            height: 560,
        }) {
            Some(surface) => surface,
            None => {
                error("Failed to create canvas surface.");
                return;
            }
        };
        let canvas = surface.canvas();

        let background_image = match load_image_from_bytes(background) {
            Some(image) => image,
            None => {
                error("Failed to load background image.");
                return;
            }
        };
        canvas.draw_image(&background_image, Point { x: 0.0, y: 0.0 }, None);
        canvas.save();

        // Avatar
        let cdn_avatar = match load_image_from_bytes(&user_avatar) {
            Some(image) => image,
            None => {
                error("Failed to decode user avatar image.");
                return;
            }
        };
        let avatar = match resize_image(cdn_avatar, 400, 400) {
            Some(image) => image,
            None => {
                error("Failed to resize user avatar image.");
                return;
            }
        };
        draw_circle(canvas, avatar, 204.0, 360.0, 200.0);

        if !draw_text_with_font(canvas, &user.name, FONT_FREDOKA, 200.0, 530.0, 140.0) {
            error("Failed to resize user avatar image.");
            return;
        }

        let nickname = match user.global_name.as_ref() {
            Some(nickname) => nickname,
            None => "Undefined",
        };
        if !draw_text_with_font(
            canvas,
            &format!("@{}", nickname),
            FONT_ROBOTO,
            96.0,
            530.0,
            380.0,
        ) {
            error("Failed to resize user avatar image.");
            return;
        }

        let image = surface.image_snapshot();
        let encoded_data = match image.encode(None, EncodedImageFormat::PNG, Some(100)) {
            Some(data) => data,
            None => {
                error("Failed to encode card image.");
                return;
            }
        };

        data = encoded_data.to_vec();
    }

    let mut utc = String::new();
    if event == EventType::MemberAdd {
        let joined_at = match member.unwrap().joined_at {
            Some(timestamp) => timestamp.as_micros() / 1000,
            None => 0,
        };
        utc.push_str(&format!("<t:{}:F>", joined_at));
    }
    let attachment = Attachment::from_bytes("Card.png".to_string(), data, 0);

    let res = http
        .create_message(channel_id.clone())
        .attachments(&[attachment])
        .content(&utc)
        .await;

    if let Err(err) = res {
        error(&format!(
            "Error trying to send card to system channel\nʟ {:?}",
            err
        ));
    }
}
/*
pub async fn global_boost(ctx: &Context, user: &User, guild_id: &GuildId) {

    let color = Colour::new(COLORS.nitro);
    let avatar_url = user.avatar_url().unwrap_or_default();
    let username = user.global_name.clone().unwrap_or(user.name.clone());
    let description = format!(
        "**<a:boost:{}> <@${}> became a <@&${}>**\n\n🚀 Thanks for boosting the server!",
        &EMOJIS.animated.boost, user.id, &GUILD.roles.boosters
    );

    let author = CreateEmbedAuthor::new(username.as_str()).icon_url(&avatar_url);
    let embed = CreateEmbed::new()
        .color(color)
        .author(author)
        .description(description)
        .thumbnail(&avatar_url);

    let channel = match guild_id.channels(ctx.http()).await {
        Ok(channels) => {
            let id = ChannelId::new(GUILD.channels.announcement);
            if let Some(channel) = channels.get(&id).cloned() {
                channel
            } else {
                error(&format!("Guild channel not found!"));
                return;
            }
        }
        Err(err) => {
            error(&format!("Failed to remove member role!\n└ {:?}", err));
            return;
        }
    };

    let payload = CreateMessage::new()
        .content("||@everyone @here||")
        .embed(embed);
    if let Err(err) = channel.send_message(ctx.http(), payload).await {
        error(&format!("Failed to send message!\n└ {:?}", err));
    }

}
*/
