# CloudPlayoutScheduleEventBatchItemType

What the event plays. `vod` plays a single media item, and `live` reserves a slot for the channel's live input.

The batch endpoint offers `vod` and `live` only; a single add ([CloudPlayoutScheduleEventCreateType](CloudPlayoutScheduleEventCreateType.md)) also offers `playlist`.


## Values

| Name | Value |
| --- | --- |
| `Vod` | vod |
| `Live` | live |
