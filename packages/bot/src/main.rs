mod constants;
mod discord;
mod env;
mod functions;
mod models;

#[cfg(target_env = "gnu")]
use crate::functions::configure_malloc;

use crate::discord::*;
use crate::functions::*;
use twilight_model::gateway::Intents;
use std::str::FromStr;

#[tokio::main]
async fn main() {
    #[cfg(target_env = "gnu")]
    configure_malloc();

    let intents = Intents::GUILD_MEMBERS
        | Intents::GUILDS
        | Intents::DIRECT_MESSAGES
        | Intents::MESSAGE_CONTENT
        | Intents::GUILD_MEMBERS
        | Intents::GUILD_MODERATION;
        
    let mut app = App::bootstrap(intents).await;
    app.run().await;
}
