use crate::discord::*;
use std::error::Error;
use twilight_gateway::{Event, EventType};

pub struct MemberUpdate;

#[async_trait]
impl EventHandler for MemberUpdate {
    fn event(&self) -> EventType {
        EventType::MemberUpdate
    }

    async fn run(&self, ctx: Context, event: Event) -> Result<(), Box<dyn Error + Send + Sync>> {
        let member_update = match event {
            Event::MemberUpdate(e) => e,
            _ => return Ok(()),
        };

        Ok(())
    }
}
