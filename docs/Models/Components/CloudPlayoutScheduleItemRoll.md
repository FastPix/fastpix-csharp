# CloudPlayoutScheduleItemRoll

One bumper on a schedule channel's timeline. Unlike the event endpoints, `id` here is the media that plays, and `bumperId` is the bumper itself.

On the schedule timeline, `id` is the media that plays and `bumperId` is the bumper. On the event endpoints, a bumper is returned as [CloudPlayoutScheduleEventRollResponse](CloudPlayoutScheduleEventRollResponse.md), where both `id` and `bumperId` refer to the bumper.


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Id` | *string* | :heavy_minus_sign: | The unique identifier of the media that plays for the bumper. | 1f7a3c92-5b6d-4e08-9a41-c2d8e5f7b310 |
| `BumperId` | *string* | :heavy_minus_sign: | The unique identifier of the bumper. | 9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88 |
| `Name` | *string* | :heavy_minus_sign: | The name of the bumper. | Ad break intro |
| `Duration` | *long* | :heavy_minus_sign: | The duration of the bumper in seconds. | 5 |
