# GetAllCloudPlayoutChannelOverlaysRequest


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel whose overlays are retrieved. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `StartTime` | *string* | :heavy_minus_sign: | Only return overlays that are on air at or after this time, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. Applies to schedule channels only. | 2026-09-02T18:00:00 |
| `EndTime` | *string* | :heavy_minus_sign: | Only return overlays that are on air at or before this time, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. Applies to schedule channels only. | 2026-09-02T19:00:00 |
