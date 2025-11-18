import { createCommand } from "#base";
import { createQueueMetadata, res } from "#functions";
import { brBuilder, createEmbed } from "@magicyan/discord";
import { QueryType, SearchQueryType } from "discord-player";
import { ApplicationCommandOptionType, ApplicationCommandType, InteractionContextType } from "discord.js";

createCommand({
    name: "player",
    description: "Commands to control your podcasts and OST queue!",
    type: ApplicationCommandType.ChatInput,
    contexts: [InteractionContextType.Guild],
    options: [
        {
            name: "add",
            description: "Add your favorite podcast or OST to queue!",
            type: ApplicationCommandOptionType.Subcommand,
            options: [
                {
                    name: "search",
                    description: "Enter a name or url.",
                    type: ApplicationCommandOptionType.String,

                },
                {
                    name: "engine",
                    description: "Search engine(optional).",
                    type: ApplicationCommandOptionType.String,
                    choices: [
                        { name: "Youtube Video", value: "youtubeVideo" },
                        { name: "Youtube Search", value: "youtubeSearch" }
                    ],
                },
            ],
        },
        {
            name: "pause",
            description: "Pause the current track!",
            type: ApplicationCommandOptionType.Subcommand,
        },
        {
            name: "resume",
            description: "Resume the current track!",
            type: ApplicationCommandOptionType.Subcommand,
        },
        {
            name: "skip",
            description: "Skip a certain numbers of track!",
            type: ApplicationCommandOptionType.Subcommand,
            options: [
                {
                    name: "amount",
                    description: "Amount of tracks to skip.",
                    type: ApplicationCommandOptionType.Integer,
                },
            ],
        },
        {
            name: "stop",
            description: "Stop and clean the queue!",
            type: ApplicationCommandOptionType.Subcommand,
        },
        {
            name: "queue",
            description: "Show the tracks on the queue.",
            type: ApplicationCommandOptionType.Subcommand,
        },
    ],
    async run(interaction) {
        const { options, member, guild, channel, client } = interaction;
        if (!member.voice.channel) {
            interaction.reply(
                res.danger(`<:icons_x:${emojis.static.icons_x}> You are not connected to a voice channel!`)
            );
            return;
        }
        if (!channel) {
            interaction.reply(
                res.danger(`<:icons_x:${emojis.static.icons_x}> It is not possible to use this command on this channel.`)
            );
            return;
        }

        await interaction.deferReply();
        const queue = client.player.queues.cache.get(guild.id);
        if (options.getSubcommand(true) !== "add" && !queue) {
            interaction.editReply(
                res.danger(`<:icons_x:${emojis.static.icons_x}> There is no track on the queue!`)
            );
            return;
        }

        const voiceChannel = member.voice.channel;
        const queueMetadata = createQueueMetadata({ channel, client, guild, voiceChannel });

        switch (options.getSubcommand(true)) {
            case "add":
                try {
                    const query = options.getString("search", true);
                    const searchEngine = options.getString("engine") ?? QueryType.YOUTUBE_VIDEO;

                    const { track, searchResult } = await client.player.play(
                        voiceChannel as never,
                        query,
                        {
                            searchEngine: searchEngine as SearchQueryType,
                            nodeOptions: { metadata: queueMetadata },
                        }
                    );

                    const display: string[] = [];

                    if (searchResult.playlist) {
                        const { tracks, title, url } = searchResult.playlist;
                        display.push(
                            `Added ${tracks.length} tracks from playlist **[${title}](${url})**.`,
                            ...tracks.map((track) => `${track.title}`).slice(0, 8),
                            "..."
                        );
                    } else {
                        display.push(
                            `${queue?.size ? "Added to queue. " : "Playing now! "} **[${track.title}](${track.url})**`
                        );
                    }
                    
                    interaction.editReply(
                        res.success(`<:icons_verified:${emojis.static.icons_verified}> ${brBuilder(display)}`)
                    );
                } catch (e) {
                    interaction.editReply(
                        res.danger(`<:icons_x:${emojis.static.icons_x}> Error when trying to play the track.\n${e}`)
                    );
                }
                break;
            case "pause":
                if (!queue) {
                    interaction.editReply(
                        res.danger(`<:icons_x:${emojis.static.icons_x}> There is no queue!`)
                    );
                    return;
                }
                if (queue.node.isPaused()){
                    interaction.editReply(
                        res.danger(`<:icons_x:${emojis.static.icons_x}> The current track is already paused!`)
                    );
                    return;
                }
                queue?.node.pause();
                interaction.editReply(
                    res.success(`<:icons_verified:${emojis.static.icons_verified}> Current track has been paused!`)
                );
                break;
            case "resume":
                if (!queue?.node.isPaused()) {
                    interaction.editReply(
                        res.danger(`<:icons_x:${emojis.static.icons_x}> The current track is not paused!`)
                    );
                    return;
                } 
                queue.node.resume();
                interaction.editReply(
                    res.success(`<:icons_verified:${emojis.static.icons_verified}> Current track has been resumed!`)
                );
                break;
            case "stop":
                queue?.node.stop();
                interaction.editReply(
                    res.success(
                        `<:icons_verified:${emojis.static.icons_verified}> Current track has been stopped and track list has been cleaned!`
                    )
                );
                break;
            case "skip":
                const amount = options.getInteger("amount") ?? 1;
                const skipAmount = Math.min(queue!.size, amount);
                for (let i = 0; i < skipAmount; i++) {
                    queue?.node.skip();
                }
                interaction.editReply(
                    res.success(
                        `<:icons_verified:${emojis.static.icons_verified}> ${skipAmount} ${
                            skipAmount > 1 ? "tracks have been skipped!" : "track has been skipped!"
                        } `
                    )
                );
                break;
            case "queue":
                /*
                multimenu({
                    embed: createEmbed({
                        color: constants.colors.fuchsia,
                        description: brBuilder(
                            "# Current queue",
                            `Amount: ${queue!.tracks.size}`,
                            "",
                            `Current track: ${queue!.currentTrack?.title ?? "Nothing"}`
                        ),
                    }),
                    items: queue!.tracks.map((track) => ({
                        color: constants.colors.green,
                        description: brBuilder(
                            `**Title**: [${track.title}](${track.url})`,
                            `**Autor**: ${track.author}`,
                            `**Duration**: ${track.duration}`
                        ),
                        thumbnail: track.thumbnail,
                    })),
                    render: (embeds, components) => interaction.editReply({ embeds, components }),
                });
                */
        }
        return;
    }
});