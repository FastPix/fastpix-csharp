# CloudPlayoutScheduleAndEpg

## Overview

Read what a Cloud Playout channel plays and when. Get the timeline as JSON, export it as CSV, or fetch the program guide as XMLTV.

### Available Operations

* [GetSchedule](#getschedule) - Get what a channel plays
* [ExportSchedule](#exportschedule) - Export what a channel plays as CSV
* [GetEpg](#getepg) - Get a channel's program guide

## GetSchedule

This endpoint returns what a Cloud Playout channel plays and when, as a flat timeline. It works for both channel types, and is the endpoint to use when you want the actual running order rather than the configuration behind it.

#### How it works

1. Make a GET request to this endpoint with the `channelId` and the channel's `type`.

2. The response returns the timeline in play order, with every entry carrying its own `startTime`, `endTime` and `duration`.

3. Narrow the response to a period with `startTime` and `endTime`.

For a loop channel the programs are expanded into the times they actually play, pass after pass, so an item that comes round four times appears four times. Bumpers appear as entries of their own, in the place they play, carrying `bumperId` and `bumperName`.

For a schedule channel the bumpers belong to the event instead, and are returned nested in the entry's `bumper` object as `preroll` and `postroll` rather than as entries of their own. Each carries the `id` of the media that plays for the bumper, the `bumperId`, its `name` and its `duration`. A bumper plays inside the event's slot, so the preroll runs from the entry's `startTime` for its duration, the postroll ends at the entry's `endTime`, and the main media fills what is left between them.

A `vod` entry's `name` is the media's `title` (set with `title` on the create media or update media endpoints; `metadata.name` is not used), or the media's id when it has no title. `videoCount` is the number of items configured on the channel, not the number of entries in the window.

#### Example

A channel guide shows viewers what is on now and next by asking for the timeline between now and two hours ahead.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="get-cloud-playout-channel-schedule" method="get" path="/cloud-playout/channels/{channelId}/schedule" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using Fastpix.Models.Requests;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var start = DateTime.UtcNow.AddMinutes(5);
var end = start.AddHours(2);

var res = await sdk.CloudPlayoutScheduleAndEpg.GetScheduleAsync(
    channelId: "<channelId>",
    type: GetCloudPlayoutChannelScheduleType.Loop,
    startTime: start.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
    endTime: end.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutChannelScheduleResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel whose timeline is retrieved. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Type` | [GetCloudPlayoutChannelScheduleType](../../Models/Requests/GetCloudPlayoutChannelScheduleType.md) | :heavy_check_mark: | The type of the channel. It must match the channel's own type. | loop |
| `StartTime` | *string* | :heavy_minus_sign: | Only return what plays at or after this time, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. | 2026-09-02T18:00:00 |
| `EndTime` | *string* | :heavy_minus_sign: | Only return what plays at or before this time, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It must be later than `startTime`. | 2026-09-02T20:00:00 |

### Response

**[GetCloudPlayoutChannelScheduleResponse](../../Models/Requests/GetCloudPlayoutChannelScheduleResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## ExportSchedule

This endpoint returns the same timeline as the get what a channel plays endpoint, as a CSV file you can open in a spreadsheet or hand to a scheduling team.

#### How it works

1. Make a GET request to this endpoint with the channel's `type` and the period you want, using `startTime` and `endTime`. Both are required.

2. The response is a CSV file, sent as `text/csv; charset=utf-8` with a `Content-Disposition` header naming it `schedule_<channelId>_<type>_from_<start>_to_<end>.csv`. The body has no byte-order mark and starts with the header row `Media ID,Duration (seconds),Start Time,End Time,Type`; a loop channel adds `,Program ID`.

3. Each row is one thing the channel plays, in play order. `Duration (seconds)` is whole seconds, rounded down; `Start Time` and `End Time` keep sub-second precision, so a value such as `2026-09-02T18:00:52.208Z` is normal.

#### Bumpers

On a schedule channel a bumper plays inside the event's slot: the preroll runs from the event's `startTime` for its own duration, the postroll ends at the event's `endTime`, and the main media fills what is left between them. The CSV lists each bumper as a row of its own, with Type `bumper` and Media ID set to the id of the media that plays for the bumper, followed or preceded by the row for the main media with its shortened times. On a loop channel bumpers are already items of their own and appear the same way.

A single export can cover at most 10 days. A longer window is refused with a 422 naming `endTime`.

#### Example

A scheduling team exports next week's running order for a channel and reviews it in a spreadsheet before the week starts.

The CSV body is returned unchanged in `Csv`, and the `Content-Disposition` header (attachment filename) is readable from `HttpMeta.Response`.

`Csv` is the decoded text of the response body. A leading UTF-8 byte-order mark, if the server sends one, is not part of the string. The `Content-Disposition` header is not in `Csv`; read it from `res.HttpMeta.Response`, as the example shows.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="export-cloud-playout-channel-schedule" method="get" path="/cloud-playout/channels/{channelId}/schedule/export" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using Fastpix.Models.Requests;
using System.Globalization;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var start = DateTime.UtcNow.AddMinutes(5);
var end = start.AddDays(7); // at most 10 days

var res = await sdk.CloudPlayoutScheduleAndEpg.ExportScheduleAsync(
    channelId: "<channelId>",
    type: ExportCloudPlayoutChannelScheduleType.Schedule,
    startTime: start.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
    endTime: end.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
);

// handle response
Console.WriteLine(res.Csv);

// the attachment filename comes from the Content-Disposition response header
var fileName = res.HttpMeta.Response.Content.Headers.ContentDisposition?.FileName;
Console.WriteLine(fileName);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel whose timeline is exported. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Type` | [ExportCloudPlayoutChannelScheduleType](../../Models/Requests/ExportCloudPlayoutChannelScheduleType.md) | :heavy_check_mark: | The type of the channel. It must match the channel's own type. | schedule |
| `StartTime` | *string* | :heavy_check_mark: | The start of the period to export, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. | 2026-09-02T00:00:00 |
| `EndTime` | *string* | :heavy_check_mark: | The end of the period to export, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It must be later than `startTime`. | 2026-09-09T00:00:00 |

### Response

**[ExportCloudPlayoutChannelScheduleResponse](../../Models/Requests/ExportCloudPlayoutChannelScheduleResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## GetEpg

This endpoint returns a Cloud Playout channel's program guide as XMLTV, the format television guides and players expect, so you can publish what is on to viewers.

#### How it works

1. Make a GET request to this endpoint with the `channelId` and the channel's `type`.

2. The response is an XML document describing the channel and the programs in the window, with each program carrying its start and stop times. `<length units="seconds">` is a whole number of seconds. `<title>` is the media's `title` (set with `title` on the create media or update media endpoints; `metadata.name` is not used), or the media's id when it has no title.

3. Set the window with `startTime` and `endTime`. Leave them out and FastPix picks a window around the present moment, inside the channel's own start and end times.

An `<icon>` is emitted on the `<channel>` element only when the channel has a logo, and on a `<programme>` element only when the item has a thumbnail. It is never a placeholder, so a missing icon means there is no image.

#### Bumpers

On a schedule channel a bumper plays inside the event's slot: the preroll runs from the event's `startTime` for its own duration, the postroll ends at the event's `endTime`, and the main media fills what is left between them. The guide lists each bumper as a `<programme>` of its own with `<category>bumper</category>`, and the main media's programme starts after the preroll and stops before the postroll. Loop channels list bumpers the same way.

When you send both `startTime` and `endTime`, the window cannot be longer than 14 days; a longer window is refused with a 422 naming `endTime`. The guide is returned in full, without pagination, and the number of programmes in it is also returned in the `X-Total-Count` header.

#### Example

A broadcaster publishes tomorrow's guide to its smart TV app by fetching the XMLTV document for that day.

The XMLTV document is returned unchanged and unparsed in `Xml`, and the `X-Total-Count` header (number of programmes) is readable from `HttpMeta.Response`.

`Xml` is the XMLTV document, returned unchanged and unparsed. The `X-Total-Count` header (number of programmes) is not in `Xml`; read it from `res.HttpMeta.Response`, as the example shows.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="get-cloud-playout-channel-epg" method="get" path="/cloud-playout/channels/{channelId}/epg" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using Fastpix.Models.Requests;
using System.Globalization;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var start = DateTime.UtcNow.AddMinutes(5);
var end = start.AddDays(1); // at most 14 days

var res = await sdk.CloudPlayoutScheduleAndEpg.GetEpgAsync(
    channelId: "<channelId>",
    type: GetCloudPlayoutChannelEpgType.Loop,
    startTime: start.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
    endTime: end.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
);

// handle response
Console.WriteLine(res.Xml);

// the number of programmes comes from the X-Total-Count response header
if (res.HttpMeta.Response.Headers.TryGetValues("X-Total-Count", out var total))
{
    Console.WriteLine(string.Join(",", total));
}
```

### Parameters

| Parameter | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel whose program guide is retrieved. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Type` | [GetCloudPlayoutChannelEpgType](../../Models/Requests/GetCloudPlayoutChannelEpgType.md) | :heavy_check_mark: | The type of the channel. It must match the channel's own type. | loop |
| `StartTime` | *string* | :heavy_minus_sign: | The start of the guide window, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. | 2026-09-02T00:00:00 |
| `EndTime` | *string* | :heavy_minus_sign: | The end of the guide window, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. When both times are given, the window cannot be longer than 14 days. | 2026-09-03T00:00:00 |

### Response

**[GetCloudPlayoutChannelEpgResponse](../../Models/Requests/GetCloudPlayoutChannelEpgResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |
