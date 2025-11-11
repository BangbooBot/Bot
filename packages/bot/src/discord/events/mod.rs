mod ban_add;
mod member_add;
mod member_removed;
mod member_update;
mod message_create;
mod ready;

use super::base::*;

pub fn events() -> Vec<Box<dyn EventHandler + Send + Sync>> {
    let events: Vec<Box<dyn EventHandler + Send + Sync>> = vec![
        Box::new(ban_add::BanAdd),
        Box::new(ready::Ready),
        Box::new(message_create::MessageCreate),
        Box::new(member_removed::MemberRemove),
    ];

    events
}
