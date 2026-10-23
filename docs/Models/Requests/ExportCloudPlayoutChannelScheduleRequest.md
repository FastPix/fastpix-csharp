# ExportCloudPlayoutChannelScheduleRequest


## Fields

| Field | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel whose timeline is exported. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Type` | [ExportCloudPlayoutChannelScheduleType](../../Models/Requests/ExportCloudPlayoutChannelScheduleType.md) | :heavy_check_mark: | The type of the channel. It must match the channel's own type. | loop |
| `StartTime` | *string* | :heavy_check_mark: | The start of the period to export, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. | 2026-09-02T00:00:00 |
| `EndTime` | *string* | :heavy_check_mark: | The end of the period to export, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It must be later than `startTime`. | 2026-09-09T00:00:00 |
