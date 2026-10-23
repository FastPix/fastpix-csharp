# CloudPlayoutChannelUpdateRequest

At least one of `name`, `startTime` and `endTime` must be provided.


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Name` | *string* | :heavy_minus_sign: | A new name for the channel. It must be between 3 and 255 characters and can contain only alphanumeric characters, hyphens, underscores, and spaces. | News 24x7 HD |
| `StartTime` | *string* | :heavy_minus_sign: | A new start time for the channel, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It must be at least 3 minutes in the future. | 2026-11-02T00:00:00 |
| `EndTime` | *string* | :heavy_minus_sign: | A new end time for the channel, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It must be after the channel's start time. | 2026-11-09T00:00:00 |
