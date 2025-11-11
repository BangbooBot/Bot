use crate::discord::*;
use std::{error::Error, sync::Arc};
use tokio::sync::Mutex;
use twilight_cache_inmemory::InMemoryCache;
use twilight_gateway::{Event, EventType, Shard};
use twilight_http::Client;
pub struct MessageCreate;

#[async_trait]
impl EventHandler for MessageCreate {
    fn event(&self) -> EventType {
        EventType::MessageCreate
    }

    async fn run(
        &self,
        shard: Arc<Mutex<Shard>>,
        http: Arc<Client>,
        cache: Arc<InMemoryCache>,
        event: Event,
    ) -> Result<(), Box<dyn Error + Send + Sync>> {
        let message = match event {
            Event::MessageCreate(e) => e,
            _ => return Ok(()),
        };

        if let Some(member) = &message.member {
            if let Some(user) = &member.user {
                if user.bot {
                    return Ok(());
                }
            }
        }

        if let Some(callback) = HANDLERS
            .prefix_command_handlers
            .get(message.content.as_str())
        {
            callback.run(shard, http, cache, message).await?;
        }

        Ok(())
    }
}
