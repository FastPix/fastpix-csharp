# CloudPlayoutScheduleEventCreateType

What the event plays. `vod` plays a single media item, `playlist` plays every item in a playlist back to back, and `live` reserves a slot for the channel's live input.

A single add offers `vod`, `playlist` and `live`; the batch endpoint ([CloudPlayoutScheduleEventBatchItemType](CloudPlayoutScheduleEventBatchItemType.md)) offers `vod` and `live` only.


## Values

| Name | Value |
| --- | --- |
| `Vod` | vod |
| `Playlist` | playlist |
| `Live` | live |
