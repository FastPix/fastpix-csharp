# GetCloudPlayoutScheduleChannelEventsRequest


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Id` | *string* | :heavy_check_mark: | The unique identifier of the schedule channel whose events are retrieved. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `StartTime` | *string* | :heavy_minus_sign: | Only return events that are still running at or after this time (an event that started earlier but ends after this time is included), in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It can be sent on its own to return everything from that point on. | 2026-09-02T00:00:00 |
| `EndTime` | *string* | :heavy_minus_sign: | Only return events that start at or before this time, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It can be sent on its own to return everything up to that point. | 2026-09-03T00:00:00 |
