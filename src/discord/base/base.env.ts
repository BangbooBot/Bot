import ck from "chalk";
import { z, ZodObject, ZodRawShape } from "zod";
import { brBuilder } from "@magicyan/discord";
import chalk from "chalk";

const x = chalk.red("✖︎");
const w = chalk.yellow("▲");

export function validateEnv<T extends ZodRawShape>(schema: ZodObject<T>){
    const result = schema.loose().safeParse(Deno.env.toObject());
    if (!result.success){
        const u = ck.underline;
        for(const error of result.error.issues){
            const { path, message } = error;
            console.error(`${x} ENV VAR → ${u.bold(path)} ${message}`);
            if (error.code == "invalid_type")
                console.log(ck.dim(
                    `└ "Expected: ${u.green(error.expected)} | Received: ${u.red(error.input)}`
                ));
        }
        Deno.exit(1);
    }
    console.log(ck.green(`${ck.magenta("☰ Environment variables")} loaded ✓`));

    return result.data as Record<string, string> & z.infer<typeof schema>;
}

declare global {
    namespace Deno {
        interface ProcessEnv {
            "Use import { env } from \"#settings\"": never
        }
    }
}