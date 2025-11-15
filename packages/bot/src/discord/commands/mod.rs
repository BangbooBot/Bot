mod age;
mod moderate;

use crate::discord::*;

pub fn prefix_commands() -> Vec<Box<dyn PrefixCommandHandler + Send + Sync>> {
    let commands: Vec<Box<dyn PrefixCommandHandler + Send + Sync>> = vec![
        //Box::new(prefix::canvas::Canvas),
    ];

    commands
}

pub fn slash_commands() -> Vec<Box<dyn SlashCommandHandler + Send + Sync>> {
    let commands: Vec<Box<dyn SlashCommandHandler + Send + Sync>> =
        vec![Box::new(age::Age), Box::new(moderate::Moderate)];

    commands
}
