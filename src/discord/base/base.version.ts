import { env } from "#env";
import ck from "chalk";

env.BASE_VERSION = "1.4.8" as const; // DO NOT CHANGE THIS VAR
const RUNTIME_VERSION = Deno.version.deno; 

const engineName = `${ck.hex("#54A044")(" Deno")}`;

export const runtimeDisplay = `${engineName} ${ck.reset.dim(RUNTIME_VERSION)}`