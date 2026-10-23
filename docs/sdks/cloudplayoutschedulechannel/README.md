# CloudPlayoutScheduleChannel

## Overview

Schedule a Cloud Playout channel by placing events on its timeline. The path `/cloud-playout/channels/{id}/schedule-event` is shared by all four single-event operations: it takes the channel id for ListEvents and AddEvent, and the event id for UpdateEvent and DeleteEvent.

### Available Operations

* [ListEvents](#listevents) - Get the events on a schedule channel
* [AddEvent](#addevent) - Add an event to a schedule channel
* [UpdateEvent](#updateevent) - Update an event on a schedule channel
* [DeleteEvent](#deleteevent) - Remove an event from a schedule channel
* [AddEvents](#addevents) - Add several events to a schedule channel

## ListEvents

This endpoint retrieves the events scheduled on a Cloud Playout schedule channel, in the order they play. The `id` in the path is the channel's ID.

#### How it works

1. Make a GET request to this endpoint with the `channelId`.

2. The response returns each event with its type, what it plays, when it runs, and any bumpers attached to it.

3. Narrow the response to a period with `startTime` and `endTime`. They select events that overlap the window, so an event that started before `startTime` but is still running at it is included. Either bound can be sent on its own, and leaving both out returns the whole timeline.

Events are returned in full, without pagination. The `id` must belong to a schedule channel; a loop channel's id is refused with 400 `Channel type mismatch`.

#### Example

A playout dashboard loads tomorrow's line-up for a channel by asking for the events between midnight and midnight.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="get-cloud-playout-schedule-channel-events" method="get" path="/cloud-playout/channels/{id}/schedule-event" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var start = DateTime.UtcNow.AddMinutes(5);
var end = start.AddDays(1);

var res = await sdk.CloudPlayoutScheduleChannel.ListEventsAsync(
    channelId: "<channelId>",
    startTime: start.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
    endTime: end.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutScheduleEventListResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the schedule channel whose events are retrieved. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `StartTime` | *string* | :heavy_minus_sign: | Only return events that are still running at or after this time (an event that started earlier but ends after this time is included), in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It can be sent on its own to return everything from that point on. | 2026-09-02T00:00:00 |
| `EndTime` | *string* | :heavy_minus_sign: | Only return events that start at or before this time, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It can be sent on its own to return everything up to that point. | 2026-09-03T00:00:00 |

### Response

**[GetCloudPlayoutScheduleChannelEventsResponse](../../Models/Requests/GetCloudPlayoutScheduleChannelEventsResponse.md)**

### Errors

| Error Type | Status Code | Content Type |
| --- | --- | --- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX | \*/\* |

## AddEvent

This endpoint schedules content on a Cloud Playout schedule channel. Unlike a loop channel, which repeats its programs, a schedule channel plays what you place on its timeline at the time you place it. The `id` in the path is the channel's ID.

#### How it works

1. Make a POST request to this endpoint with an `event` object describing what to play and when it starts.

2. What else you send depends on the `type`:

   * `vod` plays one media item. Send its `referenceId` and leave `endTime` and `name` out, because the event ends when the media does.
   * `playlist` plays every item in a playlist back to back. Send the playlist's `referenceId` and leave `endTime` and `name` out.
   * `live` reserves a slot for the channel's live input. Send `endTime` and a `name`, and leave `referenceId` out. The slot must be at least 5 minutes long.

3. The response returns the events that were created. A `playlist` becomes one event per item in it, laid end to end from `startTime`.

The `startTime` must be in the future and the event must sit inside the channel's own start and end times, and it cannot overlap an event that is already scheduled. A start time that has already passed is refused with a 422 naming `event.startTime`. Media used by a `vod` event must have reached the `ready` status, and the event takes its name from the media's `title` (`metadata.name` is not used; a media with no title shows its id). On a channel that is on air, an event cannot be added if it would start or end in the next few minutes.

#### Example

A news channel schedules a recorded bulletin at 18:00, then reserves a live slot from 19:00 to 20:00 for a press conference.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="add-event-to-cloud-playout-schedule-channel" method="post" path="/cloud-playout/channels/{id}/schedule-event" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var start = DateTime.UtcNow.AddMinutes(10); // inside the channel window; UTC, no Z
var end = start.AddMinutes(10); // a live event lasts at least 5 minutes

var res = await sdk.CloudPlayoutScheduleChannel.AddEventAsync(
    channelId: "<channelId>",
    body: new CloudPlayoutScheduleEventRequest() {
        Event = new CloudPlayoutScheduleEventCreate() {
            Type = CloudPlayoutScheduleEventCreateType.Live,
            StartTime = start.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
            EndTime = end.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
            Name = "Evening press conference",
        },
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutScheduleEventListResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the schedule channel the event is added to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Body` | [CloudPlayoutScheduleEventRequest](../../Models/Components/CloudPlayoutScheduleEventRequest.md) | :heavy_check_mark: | N/A | {<br/>"event": {<br/>"type": "live",<br/>"startTime": "2026-09-02T19:00:00",<br/>"endTime": "2026-09-02T20:00:00",<br/>"name": "Evening press conference"<br/>}<br/>} |

### Response

**[AddEventToCloudPlayoutScheduleChannelResponse](../../Models/Requests/AddEventToCloudPlayoutScheduleChannelResponse.md)**

### Errors

| Error Type | Status Code | Content Type |
| --- | --- | --- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX | \*/\* |

## UpdateEvent

This endpoint moves or renames an event on a Cloud Playout schedule channel, and is also how you attach a bumper to it. The `id` in the path is the event's ID, not the channel's.

#### How it works

1. Make a PATCH request to this endpoint with the ID of the event you want to change.

2. Send both `startTime` and `endTime`. Both are required, even if only one of them changes, and `startTime` must be in the future. For a `vod` event the span between them must equal the media's duration to the whole second, so to move a `vod` event shift both times by the same amount; any other span is refused with 400 `Duration mismatch`. Fractional seconds (`yyyy-MM-dd'T'HH:mm:ss.SSS`) are accepted and stored as sent.

3. For a `live` event, send its `name` on every update: the stored name is not carried over, and an update without one is rejected with 400 `Invalid name`. For a `vod` event leave `name` out, as its name comes from the media. On a `vod` event you can also send a `bumper` with a `preroll`, a `postroll`, or both. Each bumper needs its `id` and its `duration`, and the duration must match the bumper's own length.

4. The response returns the updated event.

The `type` of an event cannot be changed, and neither can what it plays, so leave `type` out and either leave `referenceId` out (the stored value is used) or send the one the event already has. A `live` event must still be at least 5 minutes long. Bumpers can only be attached to `vod` events; a `bumper` sent for a `live` event is rejected. Sending a `bumper` with no `preroll` and no `postroll` clears the ones already attached, and bumpers cannot be attached to an event that has segments on it. On a channel that is on air, an event cannot be changed if it runs, or would run, in the next few minutes.

#### Example

A broadcaster pushes an evening film back by fifteen minutes and adds a five second sting in front of it.

The snippet moves a `live` event, so it sends the event's `name`; a `live` event must send its `name` on every update, and a `vod` event leaves `name` out. To attach a bumper, set `Bumper = new CloudPlayoutScheduleEventBumperRequest() { Preroll = new CloudPlayoutScheduleEventRoll() { Id = "<bumperId>", Duration = 5 } }` on a `vod` event, keeping its span equal to the media's duration; only a `vod` event can carry bumpers, and a `live` event rejects one.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="update-cloud-playout-schedule-channel-event" method="patch" path="/cloud-playout/channels/{id}/schedule-event" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var start = DateTime.UtcNow.AddMinutes(12); // inside the channel window; UTC, no Z
var end = start.AddMinutes(10);

var res = await sdk.CloudPlayoutScheduleChannel.UpdateEventAsync(
    eventId: "<eventId>",
    body: new CloudPlayoutScheduleEventUpdateRequest() {
        Event = new CloudPlayoutScheduleEventUpdate() {
            StartTime = start.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
            EndTime = end.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
            Name = "Evening press conference",
        },
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutScheduleEventResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `EventId` | *string* | :heavy_check_mark: | The unique identifier of the event to update, as returned in the `id` field by the get the events on a schedule channel endpoint. | b1d6f0a7-4e28-4b93-90c5-7f2a8c1d3e64 |
| `Body` | [CloudPlayoutScheduleEventUpdateRequest](../../Models/Components/CloudPlayoutScheduleEventUpdateRequest.md) | :heavy_check_mark: | N/A | {<br/>"event": {<br/>"startTime": "2026-09-02T18:15:00",<br/>"endTime": "2026-09-02T19:15:00",<br/>"bumper": {<br/>"preroll": {<br/>"id": "9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88",<br/>"duration": 5<br/>}<br/>}<br/>}<br/>} |

### Response

**[UpdateCloudPlayoutScheduleChannelEventResponse](../../Models/Requests/UpdateCloudPlayoutScheduleChannelEventResponse.md)**

### Errors

| Error Type | Status Code | Content Type |
| --- | --- | --- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX | \*/\* |

## DeleteEvent

This endpoint removes an event from a Cloud Playout schedule channel's timeline. The media it played stays in your workspace. The `id` in the path is the event's ID, not the channel's.

#### How it works

1. Make a DELETE request to this endpoint with the ID of the event you want to remove.

2. The response confirms the removal.

Removing an event leaves a gap in the timeline rather than pulling later events forward, so the rest of the schedule keeps its times. The channel shows its slate for the gap. On a channel that is on air, an event cannot be removed if it runs in the next few minutes.

The removal is not repeatable: a second DELETE for the same `id` returns 404, which you can treat as the event already being gone.

#### Example

A broadcaster drops a scheduled repeat from tomorrow's line-up after deciding to leave the slot for a live update.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="delete-cloud-playout-schedule-channel-event" method="delete" path="/cloud-playout/channels/{id}/schedule-event" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var res = await sdk.CloudPlayoutScheduleChannel.DeleteEventAsync(
    eventId: "<eventId>"
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutSuccessResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `EventId` | *string* | :heavy_check_mark: | The unique identifier of the event to remove, as returned in the `id` field by the get the events on a schedule channel endpoint. | b1d6f0a7-4e28-4b93-90c5-7f2a8c1d3e64 |

### Response

**[DeleteCloudPlayoutScheduleChannelEventResponse](../../Models/Requests/DeleteCloudPlayoutScheduleChannelEventResponse.md)**

### Errors

| Error Type | Status Code | Content Type |
| --- | --- | --- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX | \*/\* |

## AddEvents

This endpoint places up to 500 events on a Cloud Playout schedule channel in one request, so a block of programming from a few minutes to a full day can be scheduled in a single call rather than one event at a time.

The request is all or nothing: if any event cannot be placed, none of them are written. Validation runs in stages, and each stage reports every offending entry by its position in the array as `events[0]`, `events[1]` and so on, but a later stage only runs once the earlier one passes, so a batch may need more than one round to get through:

1. Field and shape errors (422 `Request validation failed`): missing or malformed fields, type-conditional rules such as a `vod` without `referenceId` or a `live` without `endTime`, a `startTime` in the past, or a batch that spans more than 24 hours.

2. Media problems (400 `Invalid media`): a `vod` entry whose media does not exist, is not `ready`, or has no duration.

3. Schedule conflicts (409 `Schedule conflict`): an entry outside the channel's start and end times, inside the on-air restriction window, overlapping another entry in the batch, or overlapping an event already on the timeline.

#### How it works

1. Make a POST request to this endpoint with an `events` array. Each entry has a `type` and a `startTime`.

2. What else an entry carries depends on its `type`:

   * `vod` plays one media item. Send its `referenceId` and leave `endTime` and `name` out; the event ends when the media does.
   * `live` reserves a slot for the channel's live input. Send `endTime` and a `name`, and leave `referenceId` out. The slot must be at least 5 minutes long.

3. The response returns the events that were created, in the order they play.

All of the events must fall within a single 24 hour window, measured from the earliest start to the latest end, and every one must sit inside the channel's own start and end times. Events may not overlap one another or anything already scheduled on the channel; an event may start exactly when the previous one ends. Media used by a `vod` event must have reached the `ready` status. On a channel that is on air, no event may start or end in the next few minutes.

Gaps between events are allowed. The channel shows its slate for any time nothing is scheduled.

#### Example

A news channel loads tomorrow's line-up in one request: an overnight loop of recorded bulletins as `vod` events, a two hour `live` slot for the morning show, and the afternoon programming, all placed at their exact times.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="add-events-to-cloud-playout-schedule-channel" method="post" path="/cloud-playout/channels/{channelId}/schedule-events" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var start = DateTime.UtcNow.AddMinutes(25); // inside the channel window; UTC, no Z
var liveStart = start.AddMinutes(10);
var liveEnd = liveStart.AddMinutes(10);

var res = await sdk.CloudPlayoutScheduleChannel.AddEventsAsync(
    channelId: "<channelId>",
    body: new CloudPlayoutScheduleEventBatchRequest() {
        Events = new List<CloudPlayoutScheduleEventBatchItem>() {
            new CloudPlayoutScheduleEventBatchItem() {
                Type = CloudPlayoutScheduleEventBatchItemType.Vod,
                ReferenceId = "<mediaId>",
                StartTime = start.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
            },
            new CloudPlayoutScheduleEventBatchItem() {
                Type = CloudPlayoutScheduleEventBatchItemType.Live,
                StartTime = liveStart.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                EndTime = liveEnd.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                Name = "Morning show",
            },
        },
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutScheduleEventListResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the schedule channel the events are added to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Body` | [CloudPlayoutScheduleEventBatchRequest](../../Models/Components/CloudPlayoutScheduleEventBatchRequest.md) | :heavy_check_mark: | N/A | {<br/>"events": [<br/>{<br/>"type": "vod",<br/>"referenceId": "8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21",<br/>"startTime": "2026-09-02T18:00:00"<br/>},<br/>{<br/>"type": "live",<br/>"name": "Morning show",<br/>"startTime": "2026-09-03T07:00:00",<br/>"endTime": "2026-09-03T09:00:00"<br/>}<br/>]<br/>} |

### Response

**[AddEventsToCloudPlayoutScheduleChannelResponse](../../Models/Requests/AddEventsToCloudPlayoutScheduleChannelResponse.md)**

### Errors

| Error Type | Status Code | Content Type |
| --- | --- | --- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX | \*/\* |
