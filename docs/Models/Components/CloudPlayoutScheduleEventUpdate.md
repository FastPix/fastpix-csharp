# CloudPlayoutScheduleEventUpdate


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `StartTime` | *string* | :heavy_check_mark: | A new start time for the event, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It must be in the future; a start time that has already passed is rejected with a 422 naming the field, `event.startTime`, with the message `The 'startTime' must be a future date.` Fractional seconds (`yyyy-MM-dd'T'HH:mm:ss.SSS`) are accepted and stored as sent. | 2026-09-02T18:15:00 |
| `EndTime` | *string* | :heavy_check_mark: | A new end time for the event, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It must be later than `startTime`. For a `vod` event the span from `startTime` to `endTime` must equal the media's duration to the whole second, so moving a `vod` event means shifting both times by the same amount; any other span is rejected with 400 `Duration mismatch`. Fractional seconds (`yyyy-MM-dd'T'HH:mm:ss.SSS`) are accepted and stored as sent. | 2026-09-02T19:15:00 |
| `Name` | *string* | :heavy_minus_sign: | The name of a `live` event. It is required on every update of a `live` event, because the stored name is not carried over; an update without it is rejected with 400 `Invalid name`. It must be between 3 and 255 characters and can contain only alphanumeric characters, hyphens, underscores, and spaces. Leave it out for a `vod` event, which takes its name from the media; sending one is rejected with 400 `Invalid name`. | Evening press conference |
| `ReferenceId` | *string* | :heavy_minus_sign: | What the event plays cannot be changed. Either leave this out, in which case the stored value is used, or send the value the event already has. | 8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21 |
| `Bumper` | [CloudPlayoutScheduleEventBumperRequest](../../Models/Components/CloudPlayoutScheduleEventBumperRequest.md) | :heavy_minus_sign: | The bumpers to attach to the event. Sending this with neither a preroll nor a postroll removes the bumpers already attached. |  |
