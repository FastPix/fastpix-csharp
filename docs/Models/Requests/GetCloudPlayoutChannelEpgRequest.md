# GetCloudPlayoutChannelEpgRequest


## Fields

| Field | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel whose program guide is retrieved. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Type` | [GetCloudPlayoutChannelEpgType](../../Models/Requests/GetCloudPlayoutChannelEpgType.md) | :heavy_check_mark: | The type of the channel. It must match the channel's own type. | loop |
| `StartTime` | *string* | :heavy_minus_sign: | The start of the guide window, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. | 2026-09-02T00:00:00 |
| `EndTime` | *string* | :heavy_minus_sign: | The end of the guide window, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. When both times are given, the window cannot be longer than 14 days. | 2026-09-03T00:00:00 |
