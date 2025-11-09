use std::collections::HashMap;
use dotenvy::dotenv;
use once_cell::sync::Lazy;
use std::env;
use serde::Deserialize;
use crate::functions::*;

#[derive(Deserialize)]
pub struct EnvSchema {
    pub BOT_TOKEN: String,
}

pub static ENV: Lazy<EnvSchema> = Lazy::new(|| {
    if let Err(err) = dotenv() {
        error(&format!("Failed to load .env file\n└{}", err));
        panic!();
    }
    
    let env_vars = env::vars().collect::<HashMap<String, String>>();

    let env: EnvSchema = match serde_json::to_string(&env_vars) {
        Ok(env_str) => match serde_json::from_str(&env_str) {
            Ok(schema) => schema,
            Err(err) => {
                error(&format!("Failed to parse environment variables\n└{}", err));
                panic!();
            }
        },
        Err(err) => {
            error(&format!("Failed to parse environment variables\n└{}", err));
            panic!();
        }
    };

    env
});