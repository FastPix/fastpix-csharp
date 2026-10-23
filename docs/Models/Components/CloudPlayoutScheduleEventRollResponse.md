# CloudPlayoutScheduleEventRollResponse

A bumper attached to an event, as returned by the event endpoints. The timeline returned by the get what a channel plays endpoint describes the same bumper with `CloudPlayoutScheduleItemRoll`, where `id` is the media that plays instead.

On event responses, `id` and `bumperId` both refer to the bumper and carry the same value.


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Id` | *string* | :heavy_minus_sign: | The unique identifier of the bumper (filler). | 9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88 |
| `BumperId` | *string* | :heavy_minus_sign: | The unique identifier of the bumper. It carries the same value as `id`, and is present so the same key exists on the channel schedule view. | 9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88 |
| `Duration` | *long* | :heavy_minus_sign: | The duration of the bumper in seconds. | 5 |
