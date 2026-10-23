# CloudPlayoutScheduleEvent

An event on a Cloud Playout schedule channel.


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Id` | *string* | :heavy_minus_sign: | The unique identifier FastPix assigns to the event. Use this identifier when you update or remove it. | b1d6f0a7-4e28-4b93-90c5-7f2a8c1d3e64 |
| `ChannelId` | *string* | :heavy_minus_sign: | The unique identifier of the channel the event is scheduled on. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Type` | [CloudPlayoutScheduleEventType](../../Models/Components/CloudPlayoutScheduleEventType.md) | :heavy_minus_sign: | What the event plays. A `playlist` you scheduled is returned as one `vod` event per item in it. | vod |
| `ReferenceId` | *string* | :heavy_minus_sign: | The unique identifier of the media the event plays. Not returned for a `live` event. | 8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21 |
| `Name` | *string* | :heavy_minus_sign: | The name of the event. For a `vod` event this is the media's `title`, as set with `title` on the create media or update media endpoints; `metadata.name` is not used, and when the media has no title its id is shown instead. For a `live` event it is the name you gave the event. | Morning headlines |
| `StartTime` | [DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0) | :heavy_minus_sign: | When the event starts, defined as a localDateTime (UTC Time). | 2026-09-02T18:00:00Z |
| `EndTime` | [DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0) | :heavy_minus_sign: | When the event ends, defined as a localDateTime (UTC Time). | 2026-09-02T18:10:30Z |
| `Duration` | *string* | :heavy_minus_sign: | How long the event runs, formatted as `HH:MM:SS`. | 00:10:30 |
| `SourceResolution` | *string* | :heavy_minus_sign: | The resolution the event is played out at. | 1080p |
| `Thumbnail` | *string* | :heavy_minus_sign: | A URL to the event's thumbnail image. | https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png |
| `Bumper` | [CloudPlayoutScheduleEventBumper](../../Models/Components/CloudPlayoutScheduleEventBumper.md) | :heavy_minus_sign: | The bumpers attached to the event. Each key is present only when a bumper of that kind is attached; a key is never returned with a null value. |  |
| `Segments` | List<[CloudPlayoutSegment](../../Models/Components/CloudPlayoutSegment.md)> | :heavy_minus_sign: | The segments marked on the event, in the order they occur. |  |
| `CreatedAt` | [DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0) | :heavy_minus_sign: | Time the event was created, defined as a localDateTime (UTC Time). | 2026-08-20T09:02:11.114530Z |
| `UpdatedAt` | [DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0) | :heavy_minus_sign: | Time the event was last updated, defined as a localDateTime (UTC Time). | 2026-08-20T09:02:11.114530Z |
