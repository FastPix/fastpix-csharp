# CloudPlayoutScheduleEventBumperRequest

The bumpers to attach to the event. Only a `vod` event can carry bumpers; sending this for a `live` event is rejected. Sending it with neither a preroll nor a postroll removes the bumpers already attached.

In the request, each roll is a [CloudPlayoutScheduleEventRoll](CloudPlayoutScheduleEventRoll.md) whose `id` is the bumper's id. There is no `bumperId` in the request.


## Fields

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `Preroll` | [CloudPlayoutScheduleEventRoll](../../Models/Components/CloudPlayoutScheduleEventRoll.md) | :heavy_minus_sign: | N/A |
| `Postroll` | [CloudPlayoutScheduleEventRoll](../../Models/Components/CloudPlayoutScheduleEventRoll.md) | :heavy_minus_sign: | N/A |
