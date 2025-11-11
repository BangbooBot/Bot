use crate::{
    discord::*,
    functions::{error, global_message},
};
use std::{error::Error, sync::Arc};
use tokio::sync::Mutex;
use twilight_cache_inmemory::InMemoryCache;
use twilight_gateway::{Event, EventType, Shard};
use twilight_http::Client;

pub struct MemberAdded;

#[async_trait]
impl EventHandler for MemberAdded {
    fn event(&self) -> EventType {
        EventType::MemberAdd
    }

    async fn run(
        &self,
        shard: Arc<Mutex<Shard>>,
        http: Arc<Client>,
        cache: Arc<InMemoryCache>,
        event: Event,
    ) -> Result<(), Box<dyn Error + Send + Sync>> {
        let member_add = match event {
            Event::MemberAdd(e) => e,
            _ => return Ok(()),
        };

        if member_add.member.user.bot {
            return Ok(())
        }

        let guild_id = &member_add.guild_id;

        let mut system_channel_id = None;

        if let Some(cached_guild) = cache.guild(guild_id.clone()) {
            if let Some(sys_channel_id) = cached_guild.system_channel_id() {
                system_channel_id = Some(sys_channel_id);
            }
        }

        if system_channel_id.is_none() {
            if let Ok(guild) = http.guild(guild_id.clone()).await?.model().await {
                if let Some(sys_channel_id) = guild.system_channel_id {
                    system_channel_id = Some(sys_channel_id);
                }
            }
        }

        if system_channel_id.is_none() {
            error("System channel not set for guild.");
            return Ok(());
        }

        let system_channel_id = system_channel_id.unwrap();

        let user = &member_add.user;

        global_message(http, &system_channel_id, EventType::MemberAdd, Some(&member_add.member), user).await;

        Ok(())
    }
}
