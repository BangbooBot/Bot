use crate::{
    discord::*,
    functions::{error, global_message},
};
use std::{error::Error, sync::Arc};
use tokio::sync::Mutex;
use twilight_cache_inmemory::InMemoryCache;
use twilight_gateway::{Event, EventType, Shard};
use twilight_http::Client;

pub struct MemberUpdate;

#[async_trait]
impl EventHandler for MemberUpdate {
    fn event(&self) -> EventType {
        EventType::MemberUpdate
    }

    async fn run(
        &self,
        shard: Arc<Mutex<Shard>>,
        http: Arc<Client>,
        cache: Arc<InMemoryCache>,
        event: Event,
    ) -> Result<(), Box<dyn Error + Send + Sync>> {
        let member_update = match event {
            Event::MemberUpdate(e) => e,
            _ => return Ok(()),
        };

        Ok(())
    }
}
