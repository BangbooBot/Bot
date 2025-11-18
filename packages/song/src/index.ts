import { bootstrap } from "#base";
import { Client } from "discord.js";

let bootstrapClient!: Client<boolean>;
bootstrap({
    meta: import.meta, 
    async beforeLoad(client) {
        bootstrapClient = client;
    },
});

export const client = bootstrapClient;


