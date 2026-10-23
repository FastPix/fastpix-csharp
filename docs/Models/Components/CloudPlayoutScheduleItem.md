# CloudPlayoutScheduleItem

One thing a Cloud Playout channel plays, at the time it plays.

On a schedule item, `id` is the media that plays and `bumperId` is the bumper (set on `bumper` entries and in the nested `Bumper.Preroll` and `Bumper.Postroll`).


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Id` | *string* | :heavy_minus_sign: | The unique identifier of the media that plays. | 8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21 |
| `Name` | *string* | :heavy_minus_sign: | The name of what plays. For a `vod` entry it is the media's `title`, as set with `title` on the create media or update media endpoints; `metadata.name` is not used. When the media has no title, its id is shown instead. For a `bumper` entry it is the bumper's name, and for a `live` entry the name given to the event. | Morning headlines |
| `Type` | [CloudPlayoutScheduleItemType](../../Models/Components/CloudPlayoutScheduleItemType.md) | :heavy_minus_sign: | What plays. `vod` is a media item, `live` is the channel's live input, and `bumper` is a short clip that plays around an item. | vod |
| `StartTime` | [DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0) | :heavy_minus_sign: | When it starts, defined as a localDateTime (UTC Time). | 2026-09-02T18:00:00Z |
| `EndTime` | [DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0) | :heavy_minus_sign: | When it ends, defined as a localDateTime (UTC Time). | 2026-09-02T18:10:30Z |
| `Duration` | *string* | :heavy_minus_sign: | How long it runs, formatted as `HH:MM:SS`. | 00:10:30 |
| `Thumbnail` | *string* | :heavy_minus_sign: | A URL to its thumbnail image. | https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png |
| `SourceResolution` | *string* | :heavy_minus_sign: | The resolution it is played out at. | 1080p |
| `BumperId` | *string* | :heavy_minus_sign: | The unique identifier of the bumper. Returned on loop channels, for entries whose `type` is `bumper`. | 9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88 |
| `BumperName` | *string* | :heavy_minus_sign: | The name of the bumper. Returned on loop channels, for entries whose `type` is `bumper`. | Ad break intro |
| `Bumper` | [CloudPlayoutScheduleItemBumper](../../Models/Components/CloudPlayoutScheduleItemBumper.md) | :heavy_minus_sign: | The bumpers that play around this entry. Returned on schedule channels, where bumpers belong to the event rather than being entries of their own, nested as `bumper.preroll` and `bumper.postroll`. A bumper plays inside the event's slot, so the preroll runs from this entry's `startTime` for its duration, the postroll ends at this entry's `endTime`, and the main media fills what is left between them. |  |
| `ProgramId` | *string* | :heavy_minus_sign: | The unique identifier of the program this entry belongs to. Returned for loop channels. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `ProgramName` | *string* | :heavy_minus_sign: | The name of the program this entry belongs to. Returned for loop channels. | Morning block |
| `ProgramMediaId` | *string* | :heavy_minus_sign: | The unique identifier of the item within its program. Returned for loop channels. | 4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90 |
