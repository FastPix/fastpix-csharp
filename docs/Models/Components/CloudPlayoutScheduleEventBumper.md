# CloudPlayoutScheduleEventBumper

The bumpers attached to the event. Each key is present only when a bumper of that kind is attached; a key is never returned with a null value.

Each of `Preroll` and `Postroll` is a [CloudPlayoutScheduleEventRollResponse](CloudPlayoutScheduleEventRollResponse.md), where `id` and `bumperId` both refer to the bumper itself.


## Fields

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `Preroll` | [CloudPlayoutScheduleEventRollResponse](../../Models/Components/CloudPlayoutScheduleEventRollResponse.md) | :heavy_minus_sign: | A bumper attached to an event, as returned by the event endpoints. The timeline returned by the get what a channel plays endpoint describes the same bumper with `CloudPlayoutScheduleItemRoll`, where `id` is the media that plays instead. |
| `Postroll` | [CloudPlayoutScheduleEventRollResponse](../../Models/Components/CloudPlayoutScheduleEventRollResponse.md) | :heavy_minus_sign: | A bumper attached to an event, as returned by the event endpoints. The timeline returned by the get what a channel plays endpoint describes the same bumper with `CloudPlayoutScheduleItemRoll`, where `id` is the media that plays instead. |
