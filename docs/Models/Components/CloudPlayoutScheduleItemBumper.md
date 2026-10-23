# CloudPlayoutScheduleItemBumper

The bumpers that play around an entry on a schedule channel. Each key is present only when a bumper of that kind is attached to the event; a key is never returned with a null value.

Each of `Preroll` and `Postroll` is a [CloudPlayoutScheduleItemRoll](CloudPlayoutScheduleItemRoll.md). On the schedule timeline, `id` is the media that plays and `bumperId` is the bumper.


## Fields

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `Preroll` | [CloudPlayoutScheduleItemRoll](../../Models/Components/CloudPlayoutScheduleItemRoll.md) | :heavy_minus_sign: | One bumper on a schedule channel's timeline. Unlike the event endpoints, `id` here is the media that plays, and `bumperId` is the bumper itself. |
| `Postroll` | [CloudPlayoutScheduleItemRoll](../../Models/Components/CloudPlayoutScheduleItemRoll.md) | :heavy_minus_sign: | One bumper on a schedule channel's timeline. Unlike the event endpoints, `id` here is the media that plays, and `bumperId` is the bumper itself. |
