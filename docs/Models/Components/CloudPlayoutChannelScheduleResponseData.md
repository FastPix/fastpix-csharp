# CloudPlayoutChannelScheduleResponseData


## Fields

| Field | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `Schedule` | List<[CloudPlayoutScheduleItem](../../Models/Components/CloudPlayoutScheduleItem.md)> | :heavy_minus_sign: | What the channel plays, in play order. |  |
| `VideoCount` | *long* | :heavy_minus_sign: | The number of items configured on the channel - events for a schedule channel, program items for a loop channel. It does not depend on the requested window and can differ from the length of `schedule`. | 2 |
