# CloudPlayoutChannels

## Overview

### Available Operations

* [List](#list) - Get all channels
* [Create](#create) - Create a channel
* [GetById](#getbyid) - Get channel by ID
* [Update](#update) - Update a channel
* [Delete](#delete) - Delete a channel
* [Activate](#activate) - Activate a channel
* [Deactivate](#deactivate) - Deactivate a channel

## List

This endpoint retrieves the Cloud Playout channels in a workspace. The list is sorted by the time each channel was created and is returned in pages.

#### How it works

1. Make a GET request to this endpoint and set the `type` parameter to `loop`, `schedule`, or `all` to list both kinds of channel together.

2. Optionally use the `offset` and `limit` query parameters to page through the list, and `orderBy` to sort by creation time.

3. The response includes the matching channels and pagination metadata.

#### Example

An operations dashboard lists all channels in the workspace to show which are on air, which are still in draft, and when each one is scheduled to end.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="list-cloud-playout-channels" method="get" path="/cloud-playout/channels" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using Fastpix.Models.Requests;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var res = await sdk.CloudPlayoutChannels.ListAsync(
    type: ListCloudPlayoutChannelsType.All,
    limit: 20,
    offset: 1,
    orderBy: SortOrder.Desc
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutChannelListResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Type` | [ListCloudPlayoutChannelsType](../../Models/Requests/ListCloudPlayoutChannelsType.md) | :heavy_check_mark: | The kind of channel to list. Use `all` to list both `loop` and `schedule` channels. | all |
| `Limit` | *long* | :heavy_minus_sign: | Limit specifies the maximum number of items to display per page. | 20 |
| `Offset` | *long* | :heavy_minus_sign: | Offset determines the starting point for data retrieval within a paginated list. | 1 |
| `OrderBy` | [SortOrder](../../Models/Components/SortOrder.md) | :heavy_minus_sign: | The sort order of the channels, based on the time each channel was created. |  |

### Response

**[ListCloudPlayoutChannelsResponse](../../Models/Requests/ListCloudPlayoutChannelsResponse.md)**

### Errors

| Error Type | Status Code | Content Type |
| --- | --- | --- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX | \*/\* |

## Create

This endpoint creates a Cloud Playout channel. A channel is a continuous, linear stream that FastPix plays out on your behalf between a fixed start and end time.

Choose the `type` that matches how you want to program the channel:

* **`loop`** plays a repeating sequence of programs. You build the line-up with the loop program APIs, and the sequence repeats until the channel ends.
* **`schedule`** plays items at specific times. You place each item on the timeline with the schedule event APIs.

#### How it works

1. Make a POST request with a `name`, the channel `type`, and the `startTime` and `endTime` that bound the channel's run.

2. Provide the fields that apply to the chosen type. For a `loop` channel, `liveSupport` is required, and `slateId` is required when `liveSupport` is `true`. A `schedule` channel accepts `genres`. Fields that do not apply to the chosen type are rejected with a 422.

3. The channel is created in the `draft` status. The response includes the `playbackUrl` your viewers will watch, and the identifier of the live stream that carries the channel's output. Add the channel's content, then use the activate endpoint to put it on air.

`startTime` must be at least 3 minutes in the future, and `endTime` must be after `startTime`. Both are interpreted as UTC.

#### Example

A news publisher creates a `loop` channel that repeats a half-hour bulletin around the clock, with live support enabled so that breaking coverage can interrupt the loop, and a slate to cover any gap.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="create-cloud-playout-channel" method="post" path="/cloud-playout/channels" -->
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

var start = DateTime.UtcNow.AddMinutes(5); // must be at least 3 minutes ahead; UTC, no Z
var end = start.AddHours(1); // must be after start

var res = await sdk.CloudPlayoutChannels.CreateAsync(
    request: new CloudPlayoutChannelRequest() {
        Name = "News 24x7",
        Type = CloudPlayoutChannelRequestType.Loop,
        StartTime = start.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
        EndTime = end.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
        LiveSupport = true,
        SlateId = "default",
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutChannelResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

#### Create a schedule channel

A `schedule` channel takes no `liveSupport` or `slateId`; you place its content with the schedule event APIs after creating it.

<!-- UsageSnippet language="csharp" operationID="create-cloud-playout-channel" variant="schedule" method="post" path="/cloud-playout/channels" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var start = DateTime.UtcNow.AddMinutes(5); // must be at least 3 minutes ahead; UTC, no Z
var end = start.AddHours(1); // must be after start

var res = await sdk.CloudPlayoutChannels.CreateAsync(
    request: new CloudPlayoutChannelRequest() {
        Name = "Weekend movies",
        Type = CloudPlayoutChannelRequestType.Schedule,
        StartTime = start.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
        EndTime = end.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
        Genres = new List<CloudPlayoutChannelRequestGenresItem>() {
            CloudPlayoutChannelRequestGenresItem.Movies,
        },
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutChannelResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `request` | [CloudPlayoutChannelRequest](../../Models/Components/CloudPlayoutChannelRequest.md) | :heavy_check_mark: | N/A | {<br/>"name": "News 24x7",<br/>"type": "loop",<br/>"startTime": "2026-11-01T00:00:00",<br/>"endTime": "2026-11-08T00:00:00",<br/>"liveSupport": true,<br/>"slateId": "default"<br/>} |

### Response

**[CreateCloudPlayoutChannelResponse](../../Models/Requests/CreateCloudPlayoutChannelResponse.md)**

### Errors

| Error Type | Status Code | Content Type |
| --- | --- | --- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX | \*/\* |

## GetById

This endpoint retrieves a single Cloud Playout channel using its unique identifier. Use it to check a channel's current status and its playback details.

#### How it works

1. Make a GET request to this endpoint, replacing `{channelId}` with the UUID that FastPix returned when the channel was created.

2. The response contains the channel, including its `status`, the `playbackUrl` your viewers watch, and the identifiers of the live streams FastPix created for it.

Channels belong to the workspace they were created in, so a channel that belongs to a different workspace is not found.

#### Example

A monitoring dashboard polls this endpoint to show whether a channel is still in `draft`, is `scheduled` to go on air, is `running`, or has been taken off air.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="get-cloud-playout-channel-by-id" method="get" path="/cloud-playout/channels/{channelId}" -->
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

var res = await sdk.CloudPlayoutChannels.GetByIdAsync(channelId: "<channelId>");

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutChannelResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel, returned by FastPix when the channel was created. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |

### Response

**[GetCloudPlayoutChannelByIdResponse](../../Models/Requests/GetCloudPlayoutChannelByIdResponse.md)**

### Errors

| Error Type | Status Code | Content Type |
| --- | --- | --- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX | \*/\* |

## Update

This endpoint updates a Cloud Playout channel's name or the window it runs in. Provide only the fields you want to change; at least one is required.

#### How it works

1. Make a PATCH request to this endpoint, replacing `{channelId}` with the UUID of the channel you want to update.

2. Send any combination of `name`, `startTime`, and `endTime`.

3. The response returns the channel with its updated details.

#### When a channel can be updated

A channel can only be updated while it is in `draft` or `idle`. Once the channel has been activated, it can no longer be updated; deactivate it first. The channel's `status` cannot be changed through this endpoint: use the activate and deactivate endpoints instead.

A new `startTime` must be at least 3 minutes in the future, and `endTime` must stay after `startTime`. Changing the window does not move or remove the events already placed on the channel; events left outside the new window do not play.

#### Example

Before a channel goes on air, a scheduler pushes its `endTime` out by a day to extend a promotional run.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="update-cloud-playout-channel" method="patch" path="/cloud-playout/channels/{channelId}" -->
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

var end = DateTime.UtcNow.AddMinutes(65); // must be after startTime; UTC, no Z

var res = await sdk.CloudPlayoutChannels.UpdateAsync(
    channelId: "<channelId>",
    body: new CloudPlayoutChannelUpdateRequest() {
        EndTime = end.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutChannelResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel, returned by FastPix when the channel was created. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Body` | [CloudPlayoutChannelUpdateRequest](../../Models/Components/CloudPlayoutChannelUpdateRequest.md) | :heavy_check_mark: | At least one of `name`, `startTime` and `endTime` must be provided. | {<br/>"endTime": "2026-11-09T00:00:00"<br/>} |

### Response

**[UpdateCloudPlayoutChannelResponse](../../Models/Requests/UpdateCloudPlayoutChannelResponse.md)**

### Errors

| Error Type | Status Code | Content Type |
| --- | --- | --- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX | \*/\* |

## Delete

This endpoint permanently deletes a Cloud Playout channel together with the content that was set up on it. This action cannot be undone.

**Deactivate the channel before you delete it.** A channel that is `scheduled`, `starting`, `running` or `stopping` cannot be deleted, and the request is refused with a 409.

#### How it works

1. Deactivate the channel, if it has been activated, and wait for it to reach `idle`.

2. Make a DELETE request to this endpoint, replacing `{channelId}` with the UUID of the channel you want to delete.

3. FastPix removes the channel's content along with the channel itself. For a `loop` channel this is its programs, and for a `schedule` channel it is the scheduled events. The live streams that FastPix created for the channel's output, and for its live input, are released as well.

The media assets the channel referenced are not affected and stay in your workspace, as do the overlays and fillers it used, which remain available to other channels.

#### Example

At the end of a limited run pop-up channel, an operator deactivates it, confirms that it has stopped, and then deletes it to release the associated live streams.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="delete-cloud-playout-channel" method="delete" path="/cloud-playout/channels/{channelId}" -->
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

var res = await sdk.CloudPlayoutChannels.DeleteAsync(channelId: "<channelId>");

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutChannelDeleteResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel, returned by FastPix when the channel was created. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |

### Response

**[DeleteCloudPlayoutChannelResponse](../../Models/Requests/DeleteCloudPlayoutChannelResponse.md)**

### Errors

| Error Type | Status Code | Content Type |
| --- | --- | --- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX | \*/\* |

## Activate

This endpoint activates a Cloud Playout channel, so FastPix prepares it and takes it to air at its start time. A channel you have configured stays in `draft` until you activate it.

#### How it works

1. Finish configuring the channel first, so it has something to play. A `schedule` channel needs at least one event inside its start and end times, and a `loop` channel needs at least one program that holds media.

2. Make a POST request to this endpoint with the `channelId`. No request body is needed.

3. The channel moves to `scheduled`, goes to air by itself at its start time, and moves through `starting` to `running`.

The channel's `startTime` must still be at least 3 minutes ahead when you activate it. A channel that is already `scheduled`, `starting` or `running` cannot be activated again, so deactivate it first if you want to restart it. Activation is all or nothing: if any step of it is refused, whatever was started is stopped and the channel returns to the status it had, so it can be activated again.

#### Example

A broadcaster finishes building next month's channel and activates it, leaving FastPix to take it to air on the first of the month.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="activate-cloud-playout-channel" method="post" path="/cloud-playout/channels/{channelId}/activate" -->
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

var res = await sdk.CloudPlayoutChannels.ActivateAsync(channelId: "<channelId>");

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
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel to activate. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |

### Response

**[ActivateCloudPlayoutChannelResponse](../../Models/Requests/ActivateCloudPlayoutChannelResponse.md)**

### Errors

| Error Type | Status Code | Content Type |
| --- | --- | --- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX | \*/\* |

## Deactivate

This endpoint takes a Cloud Playout channel off air, or cancels its scheduled run if it has not started yet. The channel and everything you configured on it are kept, so you can activate it again later.

#### How it works

1. Make a DELETE request to this endpoint with the `channelId`.

2. Playout and stream delivery are stopped, and the channel moves to `idle`. A channel that was already on air passes through `stopping` first.

3. The response confirms the change.

A channel that is in `draft` has never been activated, and a channel that is already `idle` has nothing left to stop, so deactivating either is refused with a 409. The same applies while a channel is already stopping or being deleted.

#### Example

A broadcaster takes a seasonal channel off air at the end of the season, keeping its programs ready for next year.

Deactivating stops the channel's output and returns it to `idle`; it does not delete the channel.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="deactivate-cloud-playout-channel" method="delete" path="/cloud-playout/channels/{channelId}/deactivate" -->
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

var res = await sdk.CloudPlayoutChannels.DeactivateAsync(channelId: "<channelId>");

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
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel to deactivate. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |

### Response

**[DeactivateCloudPlayoutChannelResponse](../../Models/Requests/DeactivateCloudPlayoutChannelResponse.md)**

### Errors

| Error Type | Status Code | Content Type |
| --- | --- | --- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX | \*/\* |
