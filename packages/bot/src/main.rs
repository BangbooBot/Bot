mod constants;
mod discord;
mod env;
mod functions;
mod models;

#[cfg(target_env = "gnu")]
use crate::functions::configure_malloc;

use crate::discord::*;
use crate::env::ENV;
use crate::functions::*;
use serenity::Client;
use serenity::all::{GatewayIntents, Token};
use std::str::FromStr;

#[tokio::main]
async fn main() {
    #[cfg(target_env = "gnu")]
    configure_malloc();

    let intents = GatewayIntents::GUILD_MESSAGES
        | GatewayIntents::GUILDS
        | GatewayIntents::DIRECT_MESSAGES
        | GatewayIntents::MESSAGE_CONTENT
        | GatewayIntents::GUILD_MEMBERS
        | GatewayIntents::GUILD_MODERATION;
        
    let app = App::bootstrap();

    let token = match Token::from_str(&ENV.BOT_TOKEN) {
        Ok(token) => token,
        Err(err) => {
            error(&format!("Invalid token\n{:?}", err));
            return;
        }
    };
    let mut client = match Client::builder(token, intents).event_handler(app).await {
        Ok(client) => client,
        Err(err) => {
            error(&format!(
                "Error when trying to create gateway client.\n{:?}",
                err
            ));
            return;
        }
    };

    if let Err(err) = client.start_autosharded().await {
        error(&format!("Client error\n{:?}", err));
    }
}
