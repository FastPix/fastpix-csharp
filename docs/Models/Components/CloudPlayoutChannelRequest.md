# CloudPlayoutChannelRequest


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Name` | *string* | :heavy_check_mark: | A name for the channel. It must be between 3 and 255 characters and can contain only alphanumeric characters, hyphens, underscores, and spaces. | News 24x7 |
| `Type` | [CloudPlayoutChannelRequestType](../../Models/Components/CloudPlayoutChannelRequestType.md) | :heavy_check_mark: | How the channel is programmed. A `loop` channel repeats a sequence of programs, and a `schedule` channel plays items at specific times. | loop |
| `StartTime` | *string* | :heavy_check_mark: | The time the channel starts, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It must be at least 3 minutes in the future. | 2026-11-01T00:00:00 |
| `EndTime` | *string* | :heavy_check_mark: | The time the channel ends, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It must be after `startTime`. | 2026-11-08T00:00:00 |
| `LiveSupport` | *bool* | :heavy_minus_sign: | Whether the channel can switch to a live source. Required when `type` is `loop`, and not applicable when `type` is `schedule`. | true |
| `SlateId` | *string* | :heavy_minus_sign: | The slate to show when no other content is available, given either as the ID of a `slate` filler or as `default` to use the FastPix slate. Required when `liveSupport` is `true`, and not applicable when `type` is `schedule`. | default |
| `Logo` | *string* | :heavy_minus_sign: | A publicly reachable URL for the channel's logo image. Only applicable when `type` is `loop`. | https://static.example.com/channels/news-logo.png |
| `Description` | *string* | :heavy_minus_sign: | A description of the channel, up to 255 characters. Only applicable when `type` is `loop`. | Round the clock news coverage |
| `Destination` | [CloudPlayoutChannelRequestDestination](../../Models/Components/CloudPlayoutChannelRequestDestination.md) | :heavy_minus_sign: | Where the channel is delivered. Only applicable when `type` is `loop`. | fastpixlive |
| `Captions` | *bool* | :heavy_minus_sign: | Whether captions are generated for the channel's output. Only applicable when `type` is `loop`. | false |
| `Genres` | List<[CloudPlayoutChannelRequestGenresItem](../../Models/Components/CloudPlayoutChannelRequestGenresItem.md)> | :heavy_minus_sign: | The genres that describe the channel's content, used in the channel's programme guide. Only applicable when `type` is `schedule`. | [<br/>"news"<br/>] |
