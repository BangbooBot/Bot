use crate::discord::HANDLERS;
use crate::functions::log;
use crate::{env::ENV, functions::error};
use std::sync::Arc;
use tokio::{sync::Mutex, task::JoinSet};
use twilight_cache_inmemory::{DefaultInMemoryCache, InMemoryCache, ResourceType};
use twilight_gateway::{
    Config, ConfigBuilder, Event, EventTypeFlags, Intents, Shard, StreamExt as _,
};
use twilight_http::Client;
use twilight_model::application::interaction::InteractionData;

pub struct App {
    pub http: Arc<Client>,
    pub cache: Arc<InMemoryCache>,
    pub shards: Vec<Arc<Mutex<Shard>>>,
}

#[derive(Clone)]
pub struct Context {
    pub http: Arc<Client>,
    pub cache: Arc<InMemoryCache>,
    pub shard: Arc<Mutex<Shard>>,
}

impl App {
    pub async fn bootstrap(intents: Intents) -> Self {
        let token = &ENV.BOT_TOKEN;

        let http = Arc::new(Client::new(token.clone()));

        let config = Config::new(token.clone(), intents);
        let config_callback = |_, builder: ConfigBuilder| builder.build();

        let mut shards = match twilight_gateway::create_recommended(
            &http,
            config.clone(),
            config_callback,
        )
        .await
        {
            Ok(shards) => shards,
            Err(err) => {
                error(&format!("Error trying to create shards\n└ {:?}", err));
                panic!();
            }
        }
        .into_iter()
        .map(|shard| Arc::new(Mutex::new(shard)))
        .collect::<Vec<Arc<Mutex<_>>>>();

        let cache = Arc::new(
            DefaultInMemoryCache::builder()
                .resource_types(ResourceType::all())
                .build(),
        );

        Self {
            http,
            cache,
            shards,
        }
    }

    pub async fn run(&mut self) {
        let mut set = JoinSet::new();
        for shard in self.shards.iter() {
            set.spawn(App::shard_handle(
                self.http.clone(),
                self.cache.clone(),
                shard.clone(),
            ));
        }
        while let Some(res) = set.join_next().await {
            match res {
                Ok(_) => log("Shard finished successfully."),
                Err(e) => error(&format!("{:?}", e)),
            }
        }
    }

    pub async fn shard_handle(
        http: Arc<Client>,
        cache: Arc<InMemoryCache>,
        shard: Arc<Mutex<Shard>>,
    ) {
        let ctx = Context {
            http,
            cache,
            shard: shard.clone(),
        };

        loop {
            let mut locked_shard = ctx.shard.lock().await;

            while let Some(item) = locked_shard.next_event(EventTypeFlags::all()).await {
                let Ok(event) = item else {
                    error(&format!("Error receiving event\n└ {:?}", item.unwrap_err()));

                    continue;
                };

                // Update the cache with the event.
                ctx.cache.update(&event);

                tokio::spawn(App::handle_event(ctx.clone(), event));
            }
        }
    }

    async fn handle_event(ctx: Context, event: Event) {
        if let Some(callback) = HANDLERS.event_handlers.get(&event.kind()) {
            if let Err(err) = callback.run(ctx, event).await {
                error(&format!("Event error!\n└ {:?}", err));
            }
            return;
        }

        match event {
            Event::InteractionCreate(interaction) => {
                if let Some(data) = &interaction.data {
                    match data {
                        InteractionData::ApplicationCommand(command) => {
                            if let Some(callback) =
                                HANDLERS.app_command_handlers.get(command.name.as_str())
                            {
                                if let Err(err) = callback.run(ctx, &interaction).await {
                                    error(&format!("Application command error!\n└ {:?}", err));
                                }
                            }
                        }
                        InteractionData::ModalSubmit(modal_data) => {
                            if let Some(callback) =
                                HANDLERS.modal_handlers.get(modal_data.custom_id.as_str())
                            {
                                if let Err(err) = callback.run(ctx, &interaction).await {
                                    error(&format!("Modal submit error!\n└ {:?}", err));
                                }
                            }
                        }
                        _ => {}
                    }
                }
            }
            _ => {}
        }
    }
}
