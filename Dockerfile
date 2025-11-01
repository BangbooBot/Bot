FROM denoland/deno:debian-2.5.6

WORKDIR /app

COPY deno.json deno.json

RUN deno install
COPY . .

CMD ["deno", "--allow-all", "--env-file=.env", "src/index.ts"]