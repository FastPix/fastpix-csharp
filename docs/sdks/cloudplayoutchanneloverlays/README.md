# CloudPlayoutChannelOverlays

## Overview

Operations for overlays placed on Cloud Playout channels

### Available Operations

* [List](#list) - Get all overlays on a channel
* [Add](#add) - Add an overlay to a channel
* [Update](#update) - Update an overlay on a channel
* [Delete](#delete) - Remove an overlay from a channel

## List

This endpoint retrieves every overlay placed on a Cloud Playout channel, so you can see what the channel renders and when.

#### How it works

1. Make a GET request to this endpoint with the `channelId`.

2. The response returns each placement with the overlay it uses, when it comes on air, and how it is sized and positioned.

3. Use the `id` of a placement to update or remove it.

Every placement on the channel is returned. Narrow the list to a period with `startTime` and `endTime`, or leave both out to get them all.

#### Example

A playout dashboard lists the overlays due in the next hour so an operator can confirm the right sponsor bug is scheduled.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="get-all-cloud-playout-channel-overlays" method="get" path="/cloud-playout/channels/{channelId}/overlays" -->
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
var end = start.AddHours(1);

var res = await sdk.CloudPlayoutChannelOverlays.ListAsync(
    channelId: "<channelId>",
    startTime: start.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
    endTime: end.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutChannelOverlayListResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel whose overlays are retrieved. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `StartTime` | *string* | :heavy_minus_sign: | Only return overlays that are on air at or after this time, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. Applies to schedule channels only. | 2026-09-02T18:00:00 |
| `EndTime` | *string* | :heavy_minus_sign: | Only return overlays that are on air at or before this time, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. Applies to schedule channels only. | 2026-09-02T19:00:00 |

### Response

**[GetAllCloudPlayoutChannelOverlaysResponse](../../Models/Requests/GetAllCloudPlayoutChannelOverlaysResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## Add

This endpoint places an overlay on a Cloud Playout channel, so the channel renders it onto the output while it is on air. Loop and schedule channels take the same body: one placement object, and the response is always the single placement that was created. An array body is refused with a 422 reported against `requestBody`; to place several overlays, make one request per overlay.

Create the overlay itself first with the create an overlay endpoint, and use the ID it returns here.

#### Placing an overlay

Send a single `overlayId` together with a `startTime`, and either a `duration` in seconds or `untilChannelEnd` set to true to keep the overlay on air until the channel ends. The overlay's whole run must sit inside the channel's own start and end times, so a placement that would end after the channel does is rejected.

By default the overlay keeps the size and position it was created with. Set `customResolution` to true to send your own `width` and `height`, and `position` to true to send your own `positionX` and `positionY`. The upper bounds come from the channel's own resolution, so a 1080p channel accepts a width up to 1920 and a height up to 1080. `opacity` runs from 1 to 100.

Dynamic overlays render their own layout, so `customResolution` and `position` are not supported for them and `opacity` stays at 100.

#### Showing an overlay repeatedly

A `static` overlay can come back on air on a cycle instead of staying up for the whole placement. Send `repeat` with `everySeconds` and `showForSeconds`: the overlay is shown for `showForSeconds` every `everySeconds`, measured from one appearance to the next and anchored at `startTime`. `showForSeconds` must be shorter than `everySeconds`, and the last appearance is cut short at the end of the placement window rather than dropped.

#### How many overlays can be on air at once

At most two static overlays can be on air at the same instant, and only one dynamic overlay. An aston and an L-band cannot be on air together, and while either is on air only a logo or a dynamic overlay can join it.

#### Example

A news channel places its logo in the top right corner for the whole broadcast, then adds an aston for the first ten minutes of the evening bulletin.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="add-overlay-to-cloud-playout-channel" method="post" path="/cloud-playout/channels/{channelId}/overlays" -->
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

var res = await sdk.CloudPlayoutChannelOverlays.AddAsync(
    channelId: "<channelId>",
    body: new CloudPlayoutChannelOverlayRequest() {
        OverlayId = "<overlayId>",
        StartTime = start.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
        Duration = 600,
        Position = true,
        PositionX = 40,
        PositionY = 40,
        Opacity = 100,
        Repeat = new CloudPlayoutOverlayRepeat() {
            EverySeconds = 300,
            ShowForSeconds = 30,
        },
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutChannelOverlaySingleResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel the overlay is placed on. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Body` | [CloudPlayoutChannelOverlayRequest](../../Models/Components/CloudPlayoutChannelOverlayRequest.md) | :heavy_check_mark: | N/A | {<br/>  "overlayId": "2f6c1d84-9b3a-4d0e-8f21-5c7e3a9b4d16",<br/>  "startTime": "2026-09-02T18:00:00",<br/>  "duration": 600,<br/>  "position": true,<br/>  "positionX": 40,<br/>  "positionY": 40,<br/>  "opacity": 100<br/>} |

### Response

**[AddOverlayToCloudPlayoutChannelResponse](../../Models/Requests/AddOverlayToCloudPlayoutChannelResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## Update

This endpoint changes how an overlay already placed on a Cloud Playout channel looks, without removing and adding it again.

#### How it works

1. Make a PATCH request to this endpoint with the ID of the placement, which you can get from the get all overlays on a channel endpoint.

2. Send only the fields you want to change. At least one field is required.

3. The response returns the updated placement.

When the overlay runs is fixed once it is placed, so `startTime`, `duration` and `untilChannelEnd` cannot be changed here. To move a placement, remove it and add it again. Which overlay the placement uses is fixed too.

Set `customResolution` to true to send your own `width` and `height`, and `position` to true to send your own `positionX` and `positionY`. The upper bounds come from the channel's own resolution, so a 1080p channel accepts a width up to 1920 and a height up to 1080.

Send `repeat` to change how often the overlay comes back on air, or `removeRepeat` set to true to stop it repeating and leave it on air for the whole placement. The two cannot be sent together.

What you can change depends on the overlay. A `dynamic` overlay renders its own layout, so size, position, opacity and `repeat` are not accepted for it. An aston or an L-band keeps the size and position the channel gives it, so only `repeat` can be changed on those.

#### Example

A broadcaster nudges its logo further from the frame edge and drops the opacity to 80 while a sponsor card is on air.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="update-cloud-playout-channel-overlay" method="patch" path="/cloud-playout/channels/{channelId}/overlays/{overlayConfigId}" -->
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

var res = await sdk.CloudPlayoutChannelOverlays.UpdateAsync(
    channelId: "<channelId>",
    overlayConfigId: "<overlayConfigId>",
    body: new CloudPlayoutChannelOverlayUpdateRequest() {
        Position = true,
        PositionX = 60,
        PositionY = 60,
        Opacity = 80,
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutChannelOverlaySingleResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel the overlay is placed on. Loop and schedule channels are both accepted. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `OverlayConfigId` | *string* | :heavy_check_mark: | The unique identifier of the placement to update, as returned in the `id` field by the get all overlays on a channel endpoint. | 5e8b2c31-7a49-4d62-b0f8-9c1e4a7d2b53 |
| `Body` | [CloudPlayoutChannelOverlayUpdateRequest](../../Models/Components/CloudPlayoutChannelOverlayUpdateRequest.md) | :heavy_check_mark: | N/A | {<br/>  "position": true,<br/>  "positionX": 60,<br/>  "positionY": 60,<br/>  "opacity": 80<br/>} |

### Response

**[UpdateCloudPlayoutChannelOverlayResponse](../../Models/Requests/UpdateCloudPlayoutChannelOverlayResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## Delete

This endpoint takes an overlay off a Cloud Playout channel. The overlay itself stays in your workspace and can be placed again later.

#### How it works

1. Make a DELETE request to this endpoint with the ID of the placement, which you can get from the get all overlays on a channel endpoint.

2. The response confirms the removal.

If the channel is on air, the overlay stops being rendered from the next moment it would have appeared.

The removal is not repeatable: a second DELETE for the same `overlayConfigId` returns 404, which you can treat as the placement already being gone.

#### Example

A broadcaster removes a sponsor bug from the channel once the sponsorship period ends, keeping the overlay in the workspace for the next campaign.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="delete-cloud-playout-channel-overlay" method="delete" path="/cloud-playout/channels/{channelId}/overlays/{overlayConfigId}" -->
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

var res = await sdk.CloudPlayoutChannelOverlays.DeleteAsync(
    channelId: "<channelId>",
    overlayConfigId: "<overlayConfigId>"
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
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel the overlay is placed on. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `OverlayConfigId` | *string* | :heavy_check_mark: | The unique identifier of the placement to remove, as returned in the `id` field by the get all overlays on a channel endpoint. | 5e8b2c31-7a49-4d62-b0f8-9c1e4a7d2b53 |

### Response

**[DeleteCloudPlayoutChannelOverlayResponse](../../Models/Requests/DeleteCloudPlayoutChannelOverlayResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |
