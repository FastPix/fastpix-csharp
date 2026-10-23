# FastPix C# SDK examples

Runnable examples for the FastPix C# SDK. Each one is a short, focused program;
most call the live API, and one (`verify-webhook`) runs entirely offline.

## Setup

You'll need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (or newer).

Grab your credentials from the [FastPix Dashboard](https://dashboard.fastpix.com)
and put them in the environment. Copy `.env.example` to `.env`, fill it in, and
load it into your shell (any tool works — `direnv`, `dotenv`, or a plain
`export`):

| Variable | Where it comes from |
| --- | --- |
| `FASTPIX_USERNAME` | Dashboard → Access Token |
| `FASTPIX_PASSWORD` | Dashboard → Secret Key |
| `FASTPIX_WEBHOOK_SECRET` | Dashboard → Webhooks (only for verifying webhooks) |
| `FASTPIX_READY_MEDIA_ID` | A media that's finished processing (for the playback, AI and Cloud Playout examples) |

```bash
export FASTPIX_USERNAME=your-access-token
export FASTPIX_PASSWORD=your-secret-key
```

## Running

From this folder, run an example by name:

```bash
dotnet run -- create-upload
```

Run it with no arguments to see the full list.

| Example | What it does |
| --- | --- |
| `create-upload` | Mint a signed direct-upload URL for a file |
| `verify-webhook` | Verify a webhook signature (offline, no credentials) |
| `sdk-init` | Three ways to construct the SDK client |
| `video-management` | Create a media from a URL, read it back, list your library |
| `playback` | Create and list playback IDs for a media |
| `live-streaming` | Create a live stream, toggle it, delete it |
| `ai-features` | Generate a summary and chapters for a media |
| `playlist` | Create a playlist, read it back, delete it |
| `cloud-playout` | Create a loop channel, add a program and media, read its schedule, activate and deactivate it, delete it |
| `error-handling` | Catch and inspect an API error |

The `playback`, `ai-features` and `cloud-playout` examples act on a media that has
finished processing, so set `FASTPIX_READY_MEDIA_ID` to a Ready media before running
them. `cloud-playout` also needs a free Cloud Playout channel slot on your account.

## Create an upload, then send the file

`create-upload` (and the `POST /uploads` endpoint in the ASP.NET Core project)
hand you a signed URL. The client uploads the file straight to that URL, so the
bytes never touch your server, and once it finishes FastPix processes the video
and sends the `video.media.ready` webhook.

One PUT is fine for small files. For larger ones you'll usually want a resumable
upload (chunked, with retries and progress); the same signed URL supports that too.

```bash
# 1. Ask FastPix for a signed upload URL
UPLOAD_URL=$(dotnet run -- create-upload | grep '^url:' | awk '{print $2}')

# 2. Upload the file straight to it
curl -X PUT --upload-file video.mp4 \
  -H "Content-Type: video/mp4" \
  "$UPLOAD_URL"
```

Or from the browser, straight off a file input:

```js
// 1. Ask your app for a signed upload URL
const res = await fetch("/uploads", { method: "POST" });
const { url } = await res.json();

// 2. Upload the file straight to it
await fetch(url, {
  method: "PUT",
  headers: { "Content-Type": file.type || "application/octet-stream" },
  body: file,
});
```

We mint uploads with `corsOrigin: "*"` so the browser can PUT from anywhere —
lock that down before you ship. The docs go deeper (resumable included):
https://fastpix.com/docs/upload-videos/upload-videos-from-device

## A web integration

The [`AspNetCore`](AspNetCore) folder is a small ASP.NET Core app that wires the
same ideas into two HTTP endpoints — minting upload URLs and verifying incoming
webhooks. It has its own README.

## Documentation

- [FastPix API docs](https://fastpix.com/docs)
- [Webhook event reference](https://fastpix.com/docs/webhooks/webhook-event-reference)
