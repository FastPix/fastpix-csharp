
# Changelog

All notable changes to this project will be documented in this file.

---
## [1.3.0]

### Added
- Cloud Playout: six new resources with 33 operations for channels that play
  media, playlists and live events as a continuous stream, on a loop or on a schedule.
  - `CloudPlayoutChannels` (7): list, create, get, update, delete, activate, deactivate.
  - `CloudPlayoutLoopPrograms` (11): programs on a loop channel and the media, playlists
    and bumpers inside them.
  - `CloudPlayoutScheduleChannel` (5): list, add, update and delete events on a schedule
    channel, singly or in a batch.
  - `CloudPlayoutChannelOverlays` (4): list, add, update and remove overlay placements.
  - `CloudPlayoutScheduleAndEpg` (3): what a channel plays as JSON, a CSV export
    (`ExportScheduleAsync` returns the body as `Csv`) and an XMLTV guide (`GetEpgAsync`
    returns it as `Xml`); headers such as `Content-Disposition` and `X-Total-Count` are
    on `HttpMeta.Response`.
  - `CloudPlayoutSimulcast` (3): list, add and remove RTMP(S) restream targets.
  - Request times are plain `yyyy-MM-ddTHH:mm:ss` strings (UTC, no `Z`); response times
    are `DateTime?`. Response enums are open enums, request enums are strict.
- Live stream responses (create, get, list, update) now include an optional `MediaQuality`
  (`standard`, `pro`, `premium`), kept raw if the server sends a newer value.

## [1.2.1]

### Changed
- Enum fields the API returns are now open enums (classes) instead of C# `enum`s, so
  a value newer than the SDK no longer fails the whole response — it is kept as a raw
  string. Affects every response-side enum, including `MediaStatus`,
  `MediaSourceResolution`, `MediaMaxResolution`, `MediaMediaQuality`, the
  `GetAllMediaResponse*` / `GetMediaDetailResponse*` / `SourceAccessMedia*` /
  `LiveMediaClips*` / `UpdateMedia*` / `CreateMediaResponse*` variants, `Mp4Support*`,
  playlist and track response types, and the shared `AccessPolicy`, `PolicyAction`
  and `PlaylistOrder`.
  - Backward compatible for the common cases: known values still resolve to the same
    members and `==` still works (`media.Status == MediaStatus.Ready`). The one thing
    to update is a `switch` on these values — you can no longer use a member as a
    `case` label; switch on `.Value` or use `if`/`==`. An unrecognised value has
    `IsKnown == false` with the raw text in `.Value`.
  - Request-side enums are unchanged. The 3 shared enums also drop the throw from
    `Of("unknown")`; request values come from members, so this has no practical effect.

## [1.2.0]

### Breaking
- Media `duration` is now a `double?` (seconds) instead of an `"HH:MM:SS"` string,
  matching the updated API. Affects `ManageVideos.GetByIdAsync`, `ListAsync`,
  `UpdateSourceAccessAsync`, `UpdateMp4SupportAsync`, `ListClipsAsync`,
  `Videos.UpdateAsync`, `ListLiveClipsAsync`, `Playlist.CreateAsync`, and
  `Playlists.GetAsync`, `UpdateAsync`, `AddMediaAsync`, `ReorderMediaAsync`,
  `DeleteMediaAsync` (every model carrying `duration`: `GetMediaDetailResponse`,
  `GetAllMediaResponse`, `UpdateMedia`, `SourceAccessMedia`, `Media`,
  `LiveMediaClips`, `MediaClipResponseData`, `PlaylistByIdResponseMediaListItem`).

### Added
- `InputMediaSettings.EnableRecording` on live stream creation (defaults to `true`;
  set `false` to skip the Live-to-VOD recording).
- `AccessRestrictions` (domain and user-agent allow/deny policies) on
  `PlaybackIdRequest`, `PlaybackIdSuccessResponseData`, `PlaybackSettings` and
  `PlaybackIdResponse`, so live playback IDs can be restricted on create and read
  back on get.
- `LivePlayback.UpdateDomainRestrictionsAsync` for
  `PATCH /live/streams/{streamId}/playback-ids/{playbackId}/domains`.
- `LivePlayback.UpdateUserAgentRestrictionsAsync` for
  `PATCH /live/streams/{streamId}/playback-ids/{playbackId}/user-agents`.
- Offline unit tests: model contracts, a fake-transport test for the new
  endpoints, and a response-contract scan over every resource method.

### Changed
- The validation harness looks for the OpenAPI snapshot as `openapi.yaml` only.

## [1.1.6]

### Changed
- Licence changed from MIT to Apache-2.0.

## [1.1.5]

Synchronises the SDK with the current `fastpix-openai.yaml`. The four new
In-video AI operations (`/ai/{mediaId}/advanced-summary`,
`/ai/{mediaId}/attributed-transcript`) are **not** included in this entry.

### Breaking

- **`Models.Components.GetMediaResponse` renamed to
  `Models.Components.GetMediaDetailResponse`**, following the schema rename on
  `GET /on-demand/{mediaId}`. Its generated companion types were renamed to
  match: `GetMediaResponseMaxResolution`, `…MediaQuality`, `…Mp4Support`,
  `…SourceResolution`, `…Status` and `…Track` are now
  `GetMediaDetailResponse*`. The HTTP wrapper `Models.Requests.GetMediaResponse`
  is unchanged.
- **`UpdateTrackRequest.Url` removed.** The API does not allow a track's file to
  be changed — only its language and title. Callers setting `Url` must drop it.
- **`UpdateMediaMaxResolution.ThreeHundredAndSixtyp` (`360p`) removed**, matching
  the `Update-Media.maxResolution` enum. No other `maxResolution` enum offered
  `360p`, and no request enum accepts it, so the API cannot return it.
- **`Mp4Support` on media responses is now a list, not a scalar enum.** The
  schema has long declared `mp4Support` as an array of rendition objects, but
  the SDK modelled it as a single string enum, so any response with MP4
  renditions present failed to deserialize. On `Media`, `GetAllMediaResponse`,
  `GetMediaDetailResponse`, `LiveMediaClips`, `SourceAccessMedia` and
  `UpdateMedia` the property is now `List<Mp4SupportEntry>?`, and the six
  per-model enums (`MediaMp4Support`, `GetAllMediaResponseMp4Support`,
  `GetMediaDetailResponseMp4Support`, `LiveMediaClipsMp4Support`,
  `SourceAccessMediaMp4Support`, `UpdateMediaMp4Support`) were removed.
  Read the requested setting from the request models — `CreateMediaRequestMp4Support`,
  `DirectUploadVideoMediaMp4Support` and `UpdatedMp4SupportMp4Support` are
  unchanged and remain string enums.

### Added

- **`Title` on ten track models**: `AddTrackRequest`, `AddTrackResponse`,
  `UpdateTrackRequest`, `UpdateTrackResponse`, `GenerateTrackResponse`,
  `TrackSubtitlesGenerateRequest`, `AudioTrack`, `SubtitleTrack`, `VideoTrack`
  and `VideoTrackForGetAll`.
- **`OptimizeAudio`** (`bool?`) on `Media`, `GetAllMediaResponse`,
  `LiveMediaClips`, `SourceAccessMedia`, `GetMediaDetailResponse` and
  `UpdateMedia`.
- **`Mp4SupportEntry`** component, describing one downloadable MP4 rendition
  (`Type`, `Status`, `Height`, `Width`, `Ext`), together with the
  `Mp4SupportType` (`capped_4k`, `audioOnly`), `Mp4SupportStatus` (`preparing`,
  `ready`, `failed`) and `Mp4SupportExt` (`mp4`, `m4a`) enums.
- **`360p` and `360`** added to the `sourceResolution` enums for
  `GetAllMediaResponse`, `GetMediaDetailResponse`, `LiveMediaClips`, `Media` and
  `SourceAccessMedia`. `UpdateMediaSourceResolution` already carried both.

### Fixed

- Documentation links updated for the FastPix docs reorganisation — webhook
  events now under `/docs/webhooks/`, track and playback guides under
  `/docs/video-on-demand/`, AI guides under `/docs/in-video-ai/`, live-stream
  guides under `/docs/live-streaming/`, analytics guides under
  `/docs/video-data/`, and `/docs/error-codes` → `/docs/error-codes/error-codes`.
  Live-stream "Manage streams" links now carry their per-operation anchors.
- Webhook event renamed in prose: `video.media.subtitle.generated.ready` →
  `video.media.subtitle.generated`.
- The `GET /on-demand/{mediaId}` and `PATCH /on-demand/{mediaId}/update-mp4Support`
  response examples now show `mp4Support` as the array of rendition objects the
  schema declares, rather than the scalar `"capped_4k"`.

### Compatibility

- `maxDuration.minimum` moved from `0` to `60` on the live-stream response
  models. The SDK does not emit range validation, so there is no code change.

---

## [1.1.4]

### Changed

- **SDK version bump: `1.1.3` → `1.1.4`.**
  A maintenance release that updates the SDK's internal version identifiers.
  It contains no functional, API, or behavioral changes and is fully
  backward compatible with `1.1.3`.

  Updated identifiers:
  - Package `<Version>` — bumped `1.1.3` → `1.1.4`.
  - `SdkVersion` constant — now reports `1.1.4` (kept in sync with the package
    version).
  - `User-Agent` header — outbound requests now identify as
    `fastpix-sdk/csharp 1.1.4`. This value was previously lagging at `1.1.2`
    and is now aligned with the package version.

### Compatibility

- No changes to public types, method signatures, request/response models,
  default server URLs, hooks, or retry logic.
- No action required for existing integrations — update the dependency and
  rebuild.

---

## [1.1.4]

### Changed

- **SDK version bump: `1.1.3` → `1.1.4`.**
  A maintenance release that updates the SDK's internal version identifiers.
  It contains no functional, API, or behavioral changes and is fully
  backward compatible with `1.1.3`.

  Updated identifiers:
  - Package `<Version>` — bumped `1.1.3` → `1.1.4`.
  - `SdkVersion` constant — now reports `1.1.4` (kept in sync with the package
    version).
  - `User-Agent` header — outbound requests now identify as
    `fastpix-sdk/csharp 1.1.4`. This value was previously lagging at `1.1.2`
    and is now aligned with the package version.

### Compatibility

- No changes to public types, method signatures, request/response models,
  default server URLs, hooks, or retry logic.
- No action required for existing integrations — update the dependency and
  rebuild.

---

## [1.1.3]

### ⚠️ Important — FastPix is migrating from `.io` to `.com`

All FastPix hosts and documentation links are moving to the `.com` TLD:

| Old (`.io`) | New (`.com`) |
|---|---|
| `api.fastpix.io` | `api.fastpix.com` |
| `stream.fastpix.io` | `stream.fastpix.com` |
| `images.fastpix.io` | `images.fastpix.com` |
| `dashboard.fastpix.io` | `dashboard.fastpix.com` |
| `www.fastpix.io` | `www.fastpix.com` |
| `docs.fastpix.io/...` | `fastpix.com/docs/...` |

The `.io` hosts continue to serve traffic during the transition, but **they are slated for deprecation soon** — please update any hard-coded references in your application as part of your next deploy. **We strongly recommend upgrading to this SDK release (or later) across every language you use** — every official FastPix SDK is being rolled out with the same migration.

What this means for users of the `Fastpix` C# SDK:

- **If you rely on SDK defaults**, no code change is required. The default server URL is `https://api.fastpix.com/v1/`, so updating the `Fastpix` package to `1.1.3` (e.g. `dotnet add package Fastpix --version 1.1.3`) is enough.
- **If you have an explicit server URL override** (e.g. `new FastpixSDK(serverUrl: "https://api.fastpix.io/v1/")` or `FastpixSDK.Builder().WithServerUrl("https://api.fastpix.io/v1/")`), change it to `https://api.fastpix.com/v1/`.
- **If you reference FastPix asset URLs directly** in your app (HLS playback URLs, image CDN, dashboard deep links), update those to the `.com` equivalents before `.io` is decommissioned.

All README and per-SDK doc links in this package have been updated to point at the new `https://fastpix.com/docs/...` URLs.

### Fixed (SDK ↔ API parity)

- `ManageVideos.ListAsync` (`/on-demand`): tracks now include `frameRate`, which was being silently dropped by the previous SDK build (the field was present in the spec but missing from the generated `VideoTrackForGetAll` model).
- `SigningKeys.DeleteAsync`: response shape now includes the optional `data.message` confirmation string the API has been returning.

### Docs

- All README and per-service documentation pages updated from `docs.fastpix.io/...` and `docs.fastpix.com/...` to the new `https://fastpix.com/docs/...` URL structure.

---

## [1.1.2]

### Fixed
- Fixed data event field remapping in hooks.

## [1.1.1]

- Updated documentation redirection links in README.md.
  
## [1.1.0]

- Fixed missing parameters in multiple API methods.
- Improved overall developer experience through more accurate typings.

---

## [1.0.0]

This release introduces a C#-modified version of the SDK.

- It is a generated-code-based SDK, and direct contributions or pull requests are not accepted.
- Instead of modifying the code directly, users are encouraged to open issues for bug reports or feature suggestions.
- Refer to the `CONTRIBUTING.md` for full contribution guidelines, including how to:
  - Report issues clearly with steps to reproduce
  - Share relevant logs, screenshots, or environment details
- The SDK changes in this version aim to improve compatibility with C# environments and follow platform-specific conventions.
- All fixes or improvements will be included in the next code generation cycle.

---

## [0.1.2]

- Redirection links were corrected for all the methods listed under "Available Resources and Operations" in the `README.md`, ensuring users are routed to the appropriate documentation.

---

## [0.1.1]

- Codebase updated to reflect consistent naming conventions, improving overall clarity and maintainability.

---

## [0.1.0]

Initial release of the SDK.

- **Media API**:
  - Upload media assets
  - List, fetch, update, and delete media
  - Generate and manage playback IDs
- **Live API**:
  - Create, list, update, and delete live streams
  - Generate and manage playback IDs
  - Support simulcasting to multiple platforms
- Designed for secure and efficient communication with the FastPix API

---
