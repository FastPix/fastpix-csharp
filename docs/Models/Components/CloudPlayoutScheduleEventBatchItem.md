# CloudPlayoutScheduleEventBatchItem


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Type` | [CloudPlayoutScheduleEventBatchItemType](../../Models/Components/CloudPlayoutScheduleEventBatchItemType.md) | :heavy_check_mark: | What the event plays. `vod` plays a single media item, and `live` reserves a slot for the channel's live input. | vod |
| `ReferenceId` | *string* | :heavy_minus_sign: | The unique identifier of the media for a `vod` event. Required for `vod`, and must be left out for `live`. | 8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21 |
| `StartTime` | *string* | :heavy_check_mark: | When the event starts, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It must be in the future and must fall inside the channel's own start and end times. | 2026-09-02T18:00:00 |
| `EndTime` | *string* | :heavy_minus_sign: | When the event ends, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. Required for a `live` event, which must be at least 5 minutes long, and must be left out for `vod`, which ends when its media does. | 2026-09-03T09:00:00 |
| `Name` | *string* | :heavy_minus_sign: | A name for a `live` event. It must be between 3 and 255 characters and can contain only alphanumeric characters, hyphens, underscores, and spaces. Required for `live`, and must be left out for `vod`, which takes its name from the media's `title` (`metadata.name` is not used; a media with no title shows its id). | Morning show |
