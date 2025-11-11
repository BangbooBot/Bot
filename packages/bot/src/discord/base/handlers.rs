use crate::discord::*;
use once_cell::sync::Lazy;
use std::collections::HashMap;
use twilight_gateway::EventType;

pub struct Handler {
    pub event_handlers: HashMap<EventType, Box<dyn EventHandler + Send + Sync>>,
    pub prefix_command_handlers: HashMap<String, Box<dyn PrefixCommandHandler + Send + Sync>>,
    pub slash_command_handlers: HashMap<String, Box<dyn SlashCommandHandler + Send + Sync>>,
    pub responder_handlers: HashMap<String, Box<dyn ResponderHandler + Send + Sync>>,
}

pub static HANDLERS: Lazy<Handler> = Lazy::new(|| {
    let events = events();
    let mut event_handlers = HashMap::new();
    for event in events {
        let kind = event.event();
        event_handlers.insert(kind, event);
    }

    let slash_commands = slash_commands();
    //let mut commands = Vec::new();
    let mut slash_command_handlers = HashMap::new();
    for slash_command in slash_commands {
        let cmd = slash_command.command();
        let name = slash_command.command().name.clone();
        //commands.push(cmd);
        slash_command_handlers.insert(name, slash_command);
    }

    let prefix_commands = prefix_commands();
    let mut prefix_command_handlers = HashMap::new();
    for command in prefix_commands {
        let name = format!("!{}", command.name());
        prefix_command_handlers.insert(name, command);
    }

    let responders = responders();
    let mut responder_handlers = HashMap::new();
    for responder in responders {
        let custom_id = responder.custom_id();
        responder_handlers.insert(custom_id, responder);
    }

    Handler {
        prefix_command_handlers,
        event_handlers,
        slash_command_handlers,
        responder_handlers,
    }
});
