use super::base::*;
use serenity::all::{async_trait, Context, EventHandler, FullEvent};

#[async_trait]
impl EventHandler for App {
    async fn dispatch(&self, _context: &Context, _event: &FullEvent) {
        match _event {
            FullEvent::Ready { data_about_bot, .. } => {
                
            }

            _ => {}
        }
    }
}