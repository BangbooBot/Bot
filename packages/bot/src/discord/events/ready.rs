use crate::{discord::*, functions::*};
use colored::Colorize;
use std::error::Error;
use twilight_gateway::{Event, EventType};
use twilight_model::gateway::payload::outgoing::UpdatePresence;
use twilight_model::gateway::presence::{Activity, ActivityType, Status};

pub struct Ready;

#[async_trait]
impl EventHandler for Ready {
    fn event(&self) -> EventType {
        EventType::Ready
    }

    async fn run(&self, ctx: Context, event: Event) -> Result<(), Box<dyn Error + Send + Sync>> {
        let ready = match event {
            Event::Ready(e) => e,
            _ => return Ok(()),
        };

        success(&format!("● {} online ✓", ready.user.name.underline()).bright_green());

        let app_id = ready.application.id;
        let commands = HANDLERS
            .app_command_handlers
            .values()
            .map(|val| val.command())
            .collect::<Vec<_>>();

        let res = ctx
            .http
            .interaction(app_id)
            .set_global_commands(&commands)
            .await;

        match res {
            Ok(_) => {
                success(
                    &format!(
                        "└ {} command(s) successfully registered globally!",
                        commands.len()
                    )
                    .bright_green(),
                );
                for command in commands {
                    success(
                        &format!(
                            "{{/}} Slash command > {} ✓",
                            command.name.as_str().bright_blue()
                        )
                        .bright_green(),
                    );
                }
            }
            Err(err) => {
                error(&format!(
                    "└ {} command(s) successfully registered globally!",
                    commands.len()
                ));
                error(&format!("{:?}", err));
                return Err(Box::new(err));
            }
        }

        let activity = Activity {
            application_id: None,
            assets: None,
            buttons: vec![],
            created_at: None,
            details: None,
            emoji: None,
            flags: None,
            id: None,
            instance: None,
            kind: ActivityType::Custom,
            name: "".to_string(),
            party: None,
            secrets: None,
            state: Some("Rust-powered bot.\nHosted by discloud.".to_string()),
            timestamps: None,
            url: None,
        };

        if let Ok(presence) = UpdatePresence::new(vec![activity], false, None, Status::Online) {
            ctx.shard.lock().await.command(&presence);
        }

        Ok(())
    }
}
