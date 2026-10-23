# CloudPlayoutChannel

A Cloud Playout channel.


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Id` | *string* | :heavy_minus_sign: | The unique identifier FastPix assigns to the channel. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Name` | *string* | :heavy_minus_sign: | The name given to the channel. | News 24x7 |
| `Logo` | *string* | :heavy_minus_sign: | The URL of the channel's logo image. Present only when one was given. | https://static.example.com/channels/news-logo.png |
| `Description` | *string* | :heavy_minus_sign: | The description given to the channel. Present only when one was given. | Round the clock news coverage |
| `IngestionPoint` | *string* | :heavy_minus_sign: | The RTMPS URL to send a live source to when the channel plays a live slot. Present for `schedule` channels and for `loop` channels with live support. | rtmps://live.fastpix.com:443/live/493e43e13ac83bb49672403f0fba52b8k17eed9dd1c9c18fe8177d5b547007ef4 |
| `Destination` | *string* | :heavy_minus_sign: | Where the channel is delivered. Present only when it was given. | fastpixlive |
| `StartTime` | [DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0) | :heavy_minus_sign: | The time the channel starts, defined as a localDateTime (UTC Time). | 2026-11-01T00:00:00Z |
| `EndTime` | [DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0) | :heavy_minus_sign: | The time the channel ends, defined as a localDateTime (UTC Time). | 2026-11-08T00:00:00Z |
| `LiveSupport` | *bool* | :heavy_minus_sign: | Whether the channel can switch to a live source. Present for `loop` channels. | true |
| `Status` | [CloudPlayoutChannelStatus](../../Models/Components/CloudPlayoutChannelStatus.md) | :heavy_minus_sign: | The current status of the channel. A newly created channel is in `draft`. Activating it moves it to `scheduled`; at its start time it passes through `starting` to `running`, and it moves to `completed` when it reaches its end time. Deactivating a channel moves it through `stopping` to `idle`, from which it can be activated again. `deleting` is the transient state while a delete request is processed, and `error` is reported when playout could not be started or was interrupted. | draft |
| `WorkspaceId` | *string* | :heavy_minus_sign: | The unique identifier of the workspace the channel was created in. | 37d62550-e2a5-4827-b16a-3826e66d77ad |
| `CreatedAt` | [DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0) | :heavy_minus_sign: | Time the channel was created, defined as a localDateTime (UTC Time). | 2026-10-12T06:17:26.403017Z |
| `UpdatedAt` | [DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0) | :heavy_minus_sign: | Time the channel was last updated, defined as a localDateTime (UTC Time). | 2026-10-12T06:17:26.403017Z |
| `SlateId` | *string* | :heavy_minus_sign: | The slate shown when no other content is available, as the ID of a `slate` filler or `default` for the FastPix slate. | default |
| `Type` | [CloudPlayoutChannelType](../../Models/Components/CloudPlayoutChannelType.md) | :heavy_minus_sign: | How the channel is programmed. | loop |
| `Captions` | *bool* | :heavy_minus_sign: | Whether captions are generated for the channel's output. Present for `loop` channels. | false |
| `Genres` | List<*string*> | :heavy_minus_sign: | The genres that describe the channel's content. Present for `schedule` channels when genres were given. | [<br/>"news"<br/>] |
| `EncodingTier` | *string* | :heavy_minus_sign: | The encoding tier the channel is played out with. It is assigned by FastPix. | standard |
| `PlaybackUrl` | *string* | :heavy_minus_sign: | The HLS playback URL for the channel's output. It is available as soon as the channel is created and serves content once the channel is `running`. | https://stream.fastpix.com/d6bf9101-7d5f-4247-b5a4-0ffc91896ba3.m3u8 |
| `InputPlaybackUrl` | *string* | :heavy_minus_sign: | The HLS playback URL of the channel's live input, for previewing a live source before it goes to air. Present when the channel has a live input. | https://stream.fastpix.com/396ce915-0978-4d49-a662-06c7f975dd6b.m3u8 |
| `DestinationUrl` | *string* | :heavy_minus_sign: | The RTMPS URL FastPix delivers the channel's output to. | rtmps://live.fastpix.com:443/live/9150bf73432f52483b4b819237eab3afk41b0002bba7a1f971fbada1cc746a04f |
| `DestinationLiveId` | *string* | :heavy_minus_sign: | The identifier of the live stream that carries the channel's output. Use it with the live stream APIs, for example to add a private playback ID or restrict playback. | 41b0002bba7a1f971fbada1cc746a04f |
| `InputLiveId` | *string* | :heavy_minus_sign: | The identifier of the live stream that receives the channel's live input. Present when the channel has a live input. | 17eed9dd1c9c18fe8177d5b547007ef4 |
