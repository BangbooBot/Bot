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

pub struct Age;

#[async_trait]
impl SlashCommandHandler for Age {
    fn command(&self) -> Command {
        let user_opt = CommandOption::from(UserBuilder::new(
            "user",
            "Select an user",
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
        let Some(guild_id) = &interaction.guild_id else {
            error("Guild id not found.");
            return Ok(());
        };

        let options = &command_data.options;

        let mut name = String::new();
        let mut timestamp: i64 = 0;

        let mut user_id = None;
        if let Some(opt) = options.get(0) {
            if let CommandOptionValue::User(uid) = opt.value {
                user_id = Some(uid.clone());
            }
        }

        if let Some(uid) = user_id {
            if let Some(user) = cache.user(uid.clone()) {
                name = user.global_name.as_ref().unwrap_or(&user.name).clone();
                timestamp = uid.timestamp() / 1000;
            }
        }

        if name.is_empty() {
            if let Some(member) = &interaction.member {
                if let Some(user) = &member.user {
                    name = user.global_name.as_ref().unwrap_or(&user.name).clone();
                    timestamp = user.id.timestamp() / 1000;
                }
            }
        }

        if name.is_empty() {
            error("Error trying to access unknown user.");
            return Ok(());
        }

        let mut age = String::new();
        let locale = interaction.locale.clone().unwrap_or("en-US".to_string());
        if locale == "pt-BR" {
            age = format!(
                "**{}** criou a conta <t:{}:R> em um(a) <t:{}:F> ",
                name, timestamp, timestamp
            );
        } else {
            age = format!(
                "**{}** account was created <t:{}:R> on <t:{}:F> ",
                name, timestamp, timestamp
            );
        }

        reply_with_embed(http, interaction, MessageFlags::empty(), COLORS.green, &age).await;

        Ok(())
    }
}
