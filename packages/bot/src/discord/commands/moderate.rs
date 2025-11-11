use crate::constants::*;
use crate::discord::*;
use crate::functions::*;
use async_trait::async_trait;
use std::{error::Error, sync::Arc};
use tokio::sync::Mutex;
use twilight_cache_inmemory::InMemoryCache;
use twilight_gateway::Shard;
use twilight_http::Client;
use twilight_model::{
    application::{
        command::{Command, CommandOption, CommandType},
        interaction::{
            InteractionContextType,
            application_command::{CommandData, CommandOptionValue},
        },
    },
    channel::message::MessageFlags,
    gateway::payload::incoming::InteractionCreate,
};
use twilight_util::{
    builder::command::{CommandBuilder, UserBuilder},
    snowflake::Snowflake,
};

pub struct Moderate;

#[async_trait]
impl SlashCommandHandler for Moderate {
    fn command(&self) -> Command {
        let user_opt = CommandOption::from(UserBuilder::new(
            "discord",
            "Displays your account creation date.",
        ));
        CommandBuilder::new(
            "age",
            "Displays your or another user's account creation date.",
            CommandType::ChatInput,
        )
        .option(user_opt)
        .contexts([InteractionContextType::Guild])
        .build()
    }

    async fn run(
        &self,
        shard: Arc<Mutex<Shard>>,
        http: Arc<Client>,
        cache: Arc<InMemoryCache>,
        interaction: &Box<InteractionCreate>,
        command_data: &Box<CommandData>,
    ) -> Result<(), Box<dyn Error + Send + Sync>> {
        

        Ok(())
    }
}
