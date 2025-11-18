import { bootstrap, MubiExtractor } from "#base";
import { Player } from "discord-player";
import { Client } from "discord.js";

let bootstrapClient!: Client<boolean>;
bootstrap({
    meta: import.meta, 
    async beforeLoad(client) {
        client.player = new Player(client as never);
        client.player.extractors.register(MubiExtractor, {});
        bootstrapClient = client;
    },
});

export const client = bootstrapClient;


