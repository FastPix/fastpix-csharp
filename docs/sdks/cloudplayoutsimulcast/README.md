# CloudPlayoutSimulcast

## Overview

Restream a Cloud Playout channel to third-party platforms such as YouTube or Twitch. This is not the live-stream [Simulcasts](../simulcasts/README.md) resource: it is addressed by channel, and removing a target takes the channel's `destinationLiveId` rather than a stream id.

### Available Operations

* [List](#list) - Get a channel's simulcast targets
* [Create](#create) - Add a simulcast target to a channel
* [Delete](#delete) - Remove a simulcast target from a channel

## List

This endpoint retrieves the third-party platforms a Cloud Playout channel is being restreamed to, along with the channel's own playback ID.

#### How it works

1. Make a GET request to this endpoint with the `channelId`.

2. The response returns the channel's playback ID, the ID of the stream behind it, and every simulcast target on it.

3. Use a target's `simulcastId`, together with the `destinationLiveId`, to remove it.

#### Example

An operator checks which platforms a channel is currently going out to before adding another one.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="get-cloud-playout-channel-simulcasts" method="get" path="/cloud-playout/channels/simulcast/{channelId}" -->
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

var res = await sdk.CloudPlayoutSimulcast.ListAsync(channelId: "<channelId>");

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutSimulcastListResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel whose simulcast targets are retrieved. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |

### Response

**[GetCloudPlayoutChannelSimulcastsResponse](../../Models/Requests/GetCloudPlayoutChannelSimulcastsResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## Create

This endpoint restreams a Cloud Playout channel to a third-party platform such as YouTube or Twitch, alongside the channel's own playback.

#### How it works

1. Get the RTMP server URL and stream key from the platform you want to restream to.

2. Make a POST request to this endpoint with the two joined into a single `url`, so the stream key is the last part of it.

3. The response returns the simulcast target, including the `simulcastId` you need to remove it later.

A channel can restream to at most three targets at once.

#### Example

A broadcaster restreams a live event to both YouTube and Twitch while it plays out on their own channel.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="add-cloud-playout-channel-simulcast" method="post" path="/cloud-playout/channels/simulcast/{channelId}" -->
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

var res = await sdk.CloudPlayoutSimulcast.CreateAsync(
    channelId: "<channelId>",
    body: new CloudPlayoutSimulcastRequest() {
        Url = "rtmp://a.rtmp.youtube.com/live2/abcd-1234-efgh-5678",
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutSimulcastCreateResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel to restream. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Body` | [CloudPlayoutSimulcastRequest](../../Models/Components/CloudPlayoutSimulcastRequest.md) | :heavy_check_mark: | N/A | {<br/>"url": "rtmp://a.rtmp.youtube.com/live2/abcd-1234-efgh-5678"<br/>} |

### Response

**[AddCloudPlayoutChannelSimulcastResponse](../../Models/Requests/AddCloudPlayoutChannelSimulcastResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## Delete

This endpoint stops a Cloud Playout channel being restreamed to one third-party platform. The channel's own playback and its other simulcast targets are unaffected.

#### How it works

1. Make a DELETE request to this endpoint with the `livestreamId` and the `simulcastId`, both of which you can get from the get a channel's simulcast targets endpoint.

2. The response confirms the removal.

#### Example

A broadcaster stops restreaming to a platform once a rights window for that platform has ended.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="delete-cloud-playout-channel-simulcast" method="delete" path="/cloud-playout/channels/{livestreamId}/simulcast/{simulcastId}" -->
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

var res = await sdk.CloudPlayoutSimulcast.DeleteAsync(
    livestreamId: "<destinationLiveId>",
    simulcastId: "<simulcastId>"
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
| -------- | -------- | -------- | -------- | -------- |
| `LivestreamId` | *string* | :heavy_check_mark: | The unique identifier of the stream behind the channel, returned as `destinationLiveId` by the get a channel's simulcast targets endpoint. | 8717422d89288ad5958d4a86e9afe2a2 |
| `SimulcastId` | *string* | :heavy_check_mark: | The unique identifier of the simulcast target to remove. | 5f2c8e1b7a934d06b8e4c1a3d9f07e52 |

### Response

**[DeleteCloudPlayoutChannelSimulcastResponse](../../Models/Requests/DeleteCloudPlayoutChannelSimulcastResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |
