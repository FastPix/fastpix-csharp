# CloudPlayoutScheduleEventRoll

In the request, `id` is the bumper's id. There is no `bumperId` in the request; the event endpoints return it as `id` and `bumperId`, both referring to the bumper.


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Id` | *string* | :heavy_check_mark: | The unique identifier of the bumper, as returned by the create a filler endpoint. It must belong to the same workspace as the channel. | 9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88 |
| `Duration` | *long* | :heavy_check_mark: | The duration of the bumper in seconds. It must match the bumper's own duration. | 5 |
