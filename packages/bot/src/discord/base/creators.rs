pub use async_trait::async_trait;
use std::{error::Error, sync::Arc};
use tokio::sync::Mutex;
use twilight_cache_inmemory::InMemoryCache;
use twilight_gateway::{Event, EventType, Shard};
use twilight_http::Client;
use twilight_model::application::{command::*, interaction::application_command::CommandData};
use twilight_model::gateway::payload::incoming::{InteractionCreate, MessageCreate};

#[async_trait]
pub trait EventHandler: Send + Sync {
    fn event(&self) -> EventType;
    async fn run(
        &self,
        shard: Arc<Mutex<Shard>>,
        http: Arc<Client>,
        cache: Arc<InMemoryCache>,
        event: Event,
    ) -> Result<(), Box<dyn Error + Send + Sync>>;
}

#[async_trait]
pub trait PrefixCommandHandler: Send + Sync {
    fn name(&self) -> String;
    async fn run(
        &self,
        shard: Arc<Mutex<Shard>>,
        http: Arc<Client>,
        cache: Arc<InMemoryCache>,
        message: Box<MessageCreate>,
    ) -> Result<(), Box<dyn Error + Send + Sync>>;
}

#[async_trait]
pub trait SlashCommandHandler: Send + Sync {
    fn command(&self) -> Command;
    async fn run(
        &self,
        shard: Arc<Mutex<Shard>>,
        http: Arc<Client>,
        cache: Arc<InMemoryCache>,
        interaction: &Box<InteractionCreate>,
        command_data: &Box<CommandData>,
    ) -> Result<(), Box<dyn Error + Send + Sync>>;
}

#[async_trait]
pub trait ResponderHandler: Send + Sync {
    fn custom_id(&self) -> String;
    async fn run(
        &self,
        shard: Arc<Mutex<Shard>>,
        http: Arc<Client>,
        cache: Arc<InMemoryCache>,
        event: Event,
    );
}
