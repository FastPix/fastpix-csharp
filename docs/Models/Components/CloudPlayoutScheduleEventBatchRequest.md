# CloudPlayoutScheduleEventBatchRequest


## Fields

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `Events` | List<[CloudPlayoutScheduleEventBatchItem](../../Models/Components/CloudPlayoutScheduleEventBatchItem.md)> | :heavy_check_mark: | The events to place on the channel. Up to 500 per request, and all of them must fall within a single 24 hour window. |
