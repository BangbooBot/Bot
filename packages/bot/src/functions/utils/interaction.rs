use crate::functions::*;
use std::sync::Arc;
use twilight_http::Client;
use twilight_model::channel::message::MessageFlags;
use twilight_model::gateway::payload::incoming::InteractionCreate;
use twilight_model::http::interaction::{InteractionResponse, InteractionResponseData, InteractionResponseType};
use twilight_util::builder::embed::EmbedBuilder;
use twilight_util::builder::InteractionResponseDataBuilder;

pub async fn reply(
    http: &Client,
    interaction: &Box<InteractionCreate>,
    payload: InteractionResponseData,
) -> bool {
    let response = InteractionResponse {
        kind: InteractionResponseType::ChannelMessageWithSource,
        data: Some(payload),
    };

    let result = http
        .interaction(interaction.application_id)
        .create_response(interaction.id, &interaction.token, &response)
        .await;

    if let Err(err) = result {
        error(&format!(
            "Error trying to responde command interaction!\n└ {:?}",
            err
        ));
        return false;
    }

    true
}

pub async fn defer_reply(
    http: &Client,
    interaction: &Box<InteractionCreate>,
) -> bool {
    let response = InteractionResponse {
        kind: InteractionResponseType::DeferredChannelMessageWithSource,
        data: None,
    };

    let result = http
        .interaction(interaction.application_id)
        .create_response(interaction.id, &interaction.token, &response)
        .await;

    if let Err(err) = result {
        error(&format!(
            "Error trying to responde command interaction!\n└ {:?}",
            err
        ));
        return false;
    }

    true
}

pub async fn update(
    http: &Client,
    interaction: &Box<InteractionCreate>,
    payload: InteractionResponseData,
) -> bool {
    let response = InteractionResponse {
        kind: InteractionResponseType::UpdateMessage,
        data: Some(payload),
    };

    let result = http
        .interaction(interaction.application_id)
        .create_response(interaction.id, &interaction.token, &response)
        .await;

    if let Err(err) = result {
        error(&format!(
            "Error trying to responde command interaction!\n└ {:?}",
            err
        ));
        return false;
    }

    true
}

pub async fn reply_with_embed(
    http: Arc<Client>,
    interaction: &Box<InteractionCreate>,
    flags: MessageFlags,
    color: u32,
    content: &str,
) -> bool {
    let embed = EmbedBuilder::new()
        .color(color)
        .description(content)
        .build();

    let res_message = InteractionResponseDataBuilder::new()
        .embeds(vec![embed])
        .flags(flags)
        .build();

    let response = InteractionResponse {
        kind: InteractionResponseType::ChannelMessageWithSource,
        data: Some(res_message),
    };

    let result = http
        .interaction(interaction.application_id)
        .create_response(interaction.id, &interaction.token, &response)
        .await;

    if let Err(err) = result {
        error(&format!(
            "Error trying to responde command interaction!\n└ {:?}",
            err
        ));
        return false;
    }

    true
}
