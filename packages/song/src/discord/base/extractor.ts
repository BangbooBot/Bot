//
import { client } from "#index";
import { BaseExtractor, ExtractorInfo, Track, SearchQueryType, ExtractorSearchContext, GuildQueueHistory, Player, ExtractorStreamable } from "discord-player";
import { env } from "#env";
import { Readable } from "stream";
import z from "zod";

const regex = /(?:https?:\/\/)?(?:www\.)?youtube\.com\/watch\?v=([^&]+)/;

declare module "discord.js" {
    interface Client {
        // Add your properties
        player: Player;
    }
}

const videoSchema = z.object({
    title: z.string(),
    author: z.string(),
    length: z.number(),
    views: z.number(),
    thumbnail_url: z.string(),
});

//type Video = z.infer<typeof videoSchema>;

const querySchema = z.array(z.object({
    id: z.string(),
    title: z.string(),
    uploader: z.string(),
    duration: z.number(),
    views_count: z.number().optional(),
    thumbnail: z.string().nullable(),
    url: z.string()
}));

//type Query = z.infer<typeof querySchema.element>;

export class MubiExtractor extends BaseExtractor {
    static override identifier = "mubi-extractor" as const;

    public override createBridgeQuery = (track: Track) =>
        `${track.title} by ${track.author} official audio`;

    override async activate(): Promise<void> {
        this.protocols = ['mubi'];
    }

    override async deactivate(): Promise<void> {
        this.protocols = [];
    }

    override async validate(query: string, type?: SearchQueryType | null): Promise<boolean> {
        if (type === "youtubeVideo") {
            if (query.startsWith("https://")) {
                return query.match(regex) !== null;
            }
            return !query.startsWith("https://");
        }

        if (type === "youtubeSearch") {
            return !query.startsWith("https://");
        }

        return false;
    }

    override async handle(query: string, context: ExtractorSearchContext): Promise<ExtractorInfo> {
        const { type } = context;
        const is_url = query.startsWith("https://");
        let info: ExtractorInfo = {
            playlist: null,
            tracks: []
        };

        try {
            if (type === "youtubeVideo") {
                let videoId = "";
                if (is_url) {
                    const match = query.match(regex);
                    if (!match || !match[1]) {
                        return info;
                    }
                    videoId = match[1];
                } else {
                    videoId = query;
                }

                const res = await fetch(`${env.MUBILOP_URI}/video/${videoId}/info`);
                if (!res.ok) {
                    console.error(`Failed to fetch video info for ID: ${videoId}, Status: ${res.status}`);
                    return info;
                }

                const videoDataJson = await res.json();
                const videoInfo = videoSchema.safeParse(videoDataJson);
                if (!videoInfo.success) {
                    console.error(`Failed to parse video info for ID: ${videoId}, Errors: ${videoInfo.error}`);
                    return info;
                }

                const { data } = videoInfo;
                const track = new Track(client.player, {
                    title: data.title,
                    author: data.author,
                    duration: data.length.toString(),
                    views: data.views,
                    thumbnail: data.thumbnail_url,
                    url: `https://www.youtube.com/watch?v=${videoId}`
                });
                info.tracks.push(track);
            } else if ((type === "youtubeSearch") && !is_url) {
                const encodedQuery = encodeURIComponent(query);
                const res = await fetch(`${env.MUBILOP_URI}/search?query=${encodedQuery}&max_results=1`);

                if (!res.ok) {
                    console.error(`Failed to fetch search results for query: ${query}, Status: ${res.status}`);
                    return info;
                }

                const queryDataJson = await res.json();
                const queryInfo = querySchema.safeParse(queryDataJson);
                if (!queryInfo.success) {
                    console.error(`Failed to parse search results for query: ${query}, Errors: ${queryInfo.error}`);
                    return info;
                }

                const queryData = queryInfo.data[0];
                if (!queryData) {
                    console.warn(`No search results found for query: ${query}`);
                    return info;
                }

                const infoRes = await fetch(`${env.MUBILOP_URI}/video/${queryData.id}/info`);
                if (!infoRes.ok) {
                    console.error(`Failed to fetch video info for search result ID: ${queryData.id}, Status: ${infoRes.status}`);
                    return info;
                }

                const videoDataJson = await infoRes.json();
                const videoInfo = videoSchema.safeParse(videoDataJson);
                if (!videoInfo.success) {
                    console.error(`Failed to parse video info for search result ID: ${queryData.id}, Errors: ${videoInfo.error}`);
                    return info;
                }

                const { data } = videoInfo;
                const track = new Track(client.player, {
                    title: data.title,
                    author: data.author,
                    duration: data.length.toString(),
                    views: data.views,
                    thumbnail: data.thumbnail_url,
                    url: `https://www.youtube.com/watch?v=${queryData.id}`,

                });
                info.tracks.push(track);
            }
        } catch (e: any) {
            console.error(`MubiExtractor handle method encountered an unexpected error: ${e.message}`);
        }

        return info;
    }

    override async stream(info: Track): Promise<ExtractorStreamable> {
        try {
            const id_match = info.url.match(regex);
            if (!id_match || !id_match[1]) {
                throw new Error("Failed to extract video id from url");
            }
            const url_stream = `${env.MUBILOP_URI}/video/${id_match[1]}?audio_only=true&format_type=mp3`;

            const response = await fetch(url_stream);
            if (!response.ok || !response.body) {
                throw new Error(`Failed to fetch youtube video by id.\nStatus: ${response.status}`);
            }

            const readable = Readable.fromWeb(response.body as any);
            return readable;
        } catch (e: any) {
            console.error(`MubiExtractor error\n${e.message}`);
            return "";
        }
    }

    override async getRelatedTracks(_track: Track, history: GuildQueueHistory): Promise<ExtractorInfo> {
        if (history.isEmpty()) return this.createResponse(null);

        const nextTracks = history.queue.tracks.toArray();

        if (nextTracks.length === 0) return this.createResponse(null);

        return this.createResponse(null, nextTracks);
    }
}