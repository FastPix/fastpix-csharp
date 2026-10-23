# CloudPlayoutLoopPrograms

## Overview

### Available Operations

* [List](#list) - Get all programs
* [Create](#create) - Create a program
* [AddMedia](#addmedia) - Add media to a program
* [AddPlaylist](#addplaylist) - Add a playlist to a program
* [GetMediaDetails](#getmediadetails) - Get the items in a program
* [DeleteItem](#deleteitem) - Remove an item from a program
* [DeleteItems](#deleteitems) - Remove several items from a program
* [AddBumper](#addbumper) - Add a bumper to an item
* [GetById](#getbyid) - Get program by ID
* [Update](#update) - Update a program
* [Delete](#delete) - Delete a program

## List

This endpoint retrieves every program on a Cloud Playout loop channel, in the order the channel plays them. Use it to review a channel's line-up before or while it is on air.

#### How it works

1. Make a GET request to this endpoint, replacing `{channelId}` with the UUID of the loop channel.

2. The response lists the channel's programs sorted by their position in the loop, each with the number of items it holds and its combined duration.

The complete line-up is returned in a single response, so this endpoint is not paginated.

#### Example

Before extending a channel's run, an operator lists its programs to check the total duration of one full pass through the loop.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="list-cloud-playout-programs" method="get" path="/cloud-playout/channels/{channelId}/programs" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var res = await sdk.CloudPlayoutLoopPrograms.ListAsync(
    channelId: "<channelId>"
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutProgramListResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel whose programs are listed. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |

### Response

**[ListCloudPlayoutProgramsResponse](../../Models/Requests/ListCloudPlayoutProgramsResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## Create

This endpoint creates a program on a Cloud Playout **loop** channel. A program is an ordered block of content, and a loop channel plays its programs in order and then repeats them for as long as the channel runs.

Programs apply to loop channels only. Creating a program on a `schedule` channel is rejected, because a schedule channel is built from timed events instead.

#### How it works

1. Make a POST request to this endpoint with a `name` for the program.

2. The program is created empty and is appended after the channel's existing programs. Add content to it with the media, live, and playlist endpoints.

3. The response returns the program, including the `duration` and `mediaCount` that FastPix maintains as you add content to it.

A program can be added while the channel is still being set up, and also while it is on air. When the channel is already running, the program is placed so that it does not disturb the item that is currently playing or one that is about to play.

#### Example

A broadcaster building a 24x7 channel creates a "Morning block" program, then adds the episodes that should play in that part of the loop.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="create-cloud-playout-program" method="post" path="/cloud-playout/channels/{channelId}/programs" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var res = await sdk.CloudPlayoutLoopPrograms.CreateAsync(
    channelId: "<channelId>",
    body: new CloudPlayoutProgramRequest() {
        Name = "Morning block",
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutProgramResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Body` | [CloudPlayoutProgramRequest](../../Models/Components/CloudPlayoutProgramRequest.md) | :heavy_check_mark: | N/A | {<br/>  "name": "Morning block"<br/>} |

### Response

**[CreateCloudPlayoutProgramResponse](../../Models/Requests/CreateCloudPlayoutProgramResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## AddMedia

This endpoint adds one or more media assets to a program on a Cloud Playout loop channel. The media are appended to the end of the program, in the order they appear in the request, and the channel plays them in that order each time it reaches the program.

#### How it works

1. Add the media to your workspace through the Media APIs and wait until each one reaches the `ready` status.

2. Make a POST request to this endpoint with the `mediaIds` of the assets you want to add.

3. FastPix reads each media's duration and adds it to the program, updating the program's item count and total duration.

Each media asset has to exist in the same workspace, be in the `ready` status, and have a public access policy. Media that was split into segments cannot be added to a program directly.

#### Channel duration limit

The combined duration of everything scheduled on a channel cannot exceed the channel's own run time. FastPix rejects any request that would take the channel past that limit, and adds none of the media in it.

#### Example

A broadcaster adds the four episodes that make up the morning block, in playout order, to the program that represents that part of the loop.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="add-media-to-cloud-playout-program" method="post" path="/cloud-playout/channels/{channelId}/programs/{programId}/media" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var res = await sdk.CloudPlayoutLoopPrograms.AddMediaAsync(
    channelId: "<channelId>",
    programId: "<programId>",
    body: new CloudPlayoutProgramMediaRequest() {
        MediaIds = new List<string>() {
            "<mediaId>",
        },
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutProgramResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program the media is added to. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `Body` | [CloudPlayoutProgramMediaRequest](../../Models/Components/CloudPlayoutProgramMediaRequest.md) | :heavy_check_mark: | N/A | {<br/>  "mediaIds": [<br/>    "398edd49-322c-4f06-b596-37ba7ddff0be",<br/>    "075bf22c-6261-4dc4-9795-83374ff83495"<br/>  ]<br/>} |

### Response

**[AddMediaToCloudPlayoutProgramResponse](../../Models/Requests/AddMediaToCloudPlayoutProgramResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## AddPlaylist

This endpoint adds every media item in a FastPix playlist to a program on a Cloud Playout loop channel, in one request. It is a faster way to fill a program than adding media items one at a time, and it keeps the playlist's order.

#### How it works

1. Make a POST request to this endpoint with the `playlistId` of the playlist you want to add.

2. The media items in the playlist are appended to the end of the program, in the order they appear in the playlist.

3. The response returns the updated program, with the added items counted in its item count and total duration.

Every media item in the playlist must have a public access policy, and the playlist must contain at least one item. The playlist itself is not linked to the program; its items are copied in as they are at the time of the request, so later changes to the playlist do not change the program.

#### Example

A music channel keeps a curated playlist of new releases in its media library, and adds it to the channel's evening program each week in a single request.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="add-playlist-to-cloud-playout-program" method="post" path="/cloud-playout/channels/{channelId}/programs/{programId}/playlist" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var res = await sdk.CloudPlayoutLoopPrograms.AddPlaylistAsync(
    channelId: "<channelId>",
    programId: "<programId>",
    body: new CloudPlayoutProgramPlaylistRequest() {
        PlaylistId = "<playlistId>",
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutProgramResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program the playlist is added to. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `Body` | [CloudPlayoutProgramPlaylistRequest](../../Models/Components/CloudPlayoutProgramPlaylistRequest.md) | :heavy_check_mark: | N/A | {<br/>  "playlistId": "7b2a1f4c-3d5e-4a1b-8c9d-2e6f4a7b1c30"<br/>} |

### Response

**[AddPlaylistToCloudPlayoutProgramResponse](../../Models/Requests/AddPlaylistToCloudPlayoutProgramResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## GetMediaDetails

This endpoint retrieves every item in a program on a Cloud Playout loop channel, in the order the channel plays them. Each item carries the segments defined on it, along with any bumpers attached to those segments.

#### How it works

1. Make a GET request to this endpoint with the `channelId` and the `programId`.

2. The response returns the program's items in play order, each with its `type`, `duration` and `referenceId`.

3. For each item, `segments` lists the time windows marked on it, and each segment's `bumper` shows the pre-roll and post-roll attached to that window.

Items are returned in full, without pagination.

#### Example

A playout dashboard renders the rundown for a program, showing each item in order together with the ad breaks marked on it and the bumpers that play around them.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="get-cloud-playout-program-media-details" method="get" path="/cloud-playout/channels/{channelId}/programs/{programId}/media-details" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var res = await sdk.CloudPlayoutLoopPrograms.GetMediaDetailsAsync(
    channelId: "<channelId>",
    programId: "<programId>"
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutProgramMediaListResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program whose items are retrieved. | d85083a0-e935-4c29-9c32-6720bdc9b22f |

### Response

**[GetCloudPlayoutProgramMediaDetailsResponse](../../Models/Requests/GetCloudPlayoutProgramMediaDetailsResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## DeleteItem

This endpoint removes a single item from a program on a Cloud Playout loop channel. The remaining items keep their order, and the program's item count and total duration are updated.

#### How it works

1. Make a DELETE request to this endpoint with the ID of the item you want to remove, which you can get from the get items in a program endpoint.

2. Any segments marked on the item are removed with it, and the clips FastPix created for those segments are deleted from your media library. The media the item played is not affected.

3. The response confirms the removal.

On a channel that is on air, an item cannot be removed if it is already due to play in the next few minutes, because that part of the line-up has already been sent to the playout system. To remove several items at once, use the remove several items from a program endpoint.

The removal is not repeatable: a second DELETE for the same `eventId` returns 404, which you can treat as the item already being gone.

A bumper that was added against a removed item is an item of its own and stays in the program. Remove it in the same way if it should go.

#### Example

A broadcaster drops a trailer from the evening block after the campaign it promoted has ended.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="delete-cloud-playout-program-item" method="delete" path="/cloud-playout/channels/{channelId}/programs/{programId}/event/{eventId}" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var res = await sdk.CloudPlayoutLoopPrograms.DeleteItemAsync(
    channelId: "<channelId>",
    programId: "<programId>",
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
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program the item is removed from. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `EventId` | *string* | :heavy_check_mark: | The unique identifier of the item to remove, as returned in the `id` field by the get items in a program endpoint. | 4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90 |

### Response

**[DeleteCloudPlayoutProgramItemResponse](../../Models/Requests/DeleteCloudPlayoutProgramItemResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## DeleteItems

This endpoint removes up to 500 items from a program on a Cloud Playout loop channel in one request. Use it to clear out a block of content without making a request per item.

#### How it works

1. Make a POST request to this endpoint with the IDs of the items you want to remove, which you can get from the get items in a program endpoint.

2. The request is all or nothing. If any id does not belong to the program, or any of the items cannot be removed right now, nothing is removed and the response tells you which ids were the problem.

3. Any segments marked on the removed items go with them, and the clips FastPix created for those segments are deleted from your media library. The media the items played is not affected.

Repeating the same id in the list has no extra effect. On a channel that is on air, an item cannot be removed if it is already due to play in the next few minutes, and the channel must be left with at least one item to play.

A bumper that was added against a removed item is an item of its own and stays in the program. Remove it in the same way if it should go.

#### Example

A broadcaster clears an entire promotional block from the morning program in one request at the end of a campaign.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="delete-cloud-playout-program-items" method="post" path="/cloud-playout/channels/{channelId}/programs/{programId}/event/delete" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var res = await sdk.CloudPlayoutLoopPrograms.DeleteItemsAsync(
    channelId: "<channelId>",
    programId: "<programId>",
    body: new CloudPlayoutProgramItemDeleteRequest() {
        EventIds = new List<string>() {
            "<eventId>",
        },
    }
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
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program the items are removed from. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `Body` | [CloudPlayoutProgramItemDeleteRequest](../../Models/Components/CloudPlayoutProgramItemDeleteRequest.md) | :heavy_check_mark: | N/A | {<br/>  "eventIds": [<br/>    "4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90",<br/>    "7b4d9a12-5f3c-4c8e-9d21-3a7e2c5b1f04"<br/>  ]<br/>} |

### Response

**[DeleteCloudPlayoutProgramItemsResponse](../../Models/Requests/DeleteCloudPlayoutProgramItemsResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## AddBumper

This endpoint places a bumper immediately before or after an item in a program on a Cloud Playout loop channel, so the channel plays a short clip around that item every time it comes round in the loop.

#### How it works

1. Create the bumper first with the create a filler endpoint, using the `bumper` type.

2. Make a POST request to this endpoint with the `bumperId`, the `eventId` of the item you are placing it against, and whether it is a `preroll` or a `postroll`.

3. The bumper is inserted into the program as an item of its own, directly before the target item for a `preroll` and directly after it for a `postroll`.

   Because it is an item of its own, the bumper is not tied to the target item afterwards: removing the target item leaves the bumper in the program, where it plays before or after whatever is next to it. Remove the bumper as well if it should go.

4. The response returns the updated program, with the bumper counted in its item count and total duration.

The bumper's duration is taken from the bumper itself, and the channel's total program time must still fit inside the channel's start and end times. On a channel that is on air, a bumper cannot be placed against an item that is already due to play in the next few minutes.

#### Example

A news channel places a five second sting before its headlines block, so the sting introduces the block on every pass of the loop.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="add-bumper-to-cloud-playout-program-item" method="post" path="/cloud-playout/channels/{channelId}/programs/{programId}/bumper" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var res = await sdk.CloudPlayoutLoopPrograms.AddBumperAsync(
    channelId: "<channelId>",
    programId: "<programId>",
    body: new CloudPlayoutProgramBumperRequest() {
        BumperId = "<bumperId>",
        EventId = "<eventId>",
        BumperType = CloudPlayoutProgramBumperRequestBumperType.Preroll,
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutProgramResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program the bumper is added to. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `Body` | [CloudPlayoutProgramBumperRequest](../../Models/Components/CloudPlayoutProgramBumperRequest.md) | :heavy_check_mark: | N/A | {<br/>  "bumperId": "9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88",<br/>  "eventId": "4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90",<br/>  "bumperType": "preroll"<br/>} |

### Response

**[AddBumperToCloudPlayoutProgramItemResponse](../../Models/Requests/AddBumperToCloudPlayoutProgramItemResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## GetById

This endpoint retrieves a single program on a Cloud Playout loop channel, using the channel's and the program's unique identifiers.

#### How it works

1. Make a GET request to this endpoint, replacing `{channelId}` and `{programId}` with the UUIDs of the channel and the program.

2. The response contains the program, including its position in the loop, the number of items it holds, and their combined duration.

The program has to belong to the channel named in the path. A program that belongs to a different channel or workspace is not returned.

#### Example

Before adding another episode to a block, an operator retrieves the program to check how long it already runs.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="get-cloud-playout-program-by-id" method="get" path="/cloud-playout/channels/{channelId}/programs/{programId}" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var res = await sdk.CloudPlayoutLoopPrograms.GetByIdAsync(
    channelId: "<channelId>",
    programId: "<programId>"
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutProgramResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program, returned by FastPix when the program was created. | d85083a0-e935-4c29-9c32-6720bdc9b22f |

### Response

**[GetCloudPlayoutProgramByIdResponse](../../Models/Requests/GetCloudPlayoutProgramByIdResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## Update

This endpoint renames a program on a Cloud Playout loop channel. Only the `name` can be changed. A program's content is changed with the media, live, and playlist endpoints.

#### How it works

1. Make a PATCH request to this endpoint, replacing `{channelId}` and `{programId}` with the UUIDs of the channel and the program.

2. Provide the new `name` in the request body.

3. The response returns the program with its updated details.

A program can be renamed at any time, including while the channel is on air. Renaming does not change what the channel plays.

#### Example

A broadcaster renames "Block A" to "Morning block" so that the line-up reads clearly to the operators scheduling around it.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="update-cloud-playout-program" method="patch" path="/cloud-playout/channels/{channelId}/programs/{programId}" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var res = await sdk.CloudPlayoutLoopPrograms.UpdateAsync(
    channelId: "<channelId>",
    programId: "<programId>",
    body: new CloudPlayoutProgramRequest() {
        Name = "Morning block",
    }
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutProgramResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program, returned by FastPix when the program was created. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `Body` | [CloudPlayoutProgramRequest](../../Models/Components/CloudPlayoutProgramRequest.md) | :heavy_check_mark: | N/A | {<br/>  "name": "Morning block"<br/>} |

### Response

**[UpdateCloudPlayoutProgramResponse](../../Models/Requests/UpdateCloudPlayoutProgramResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |

## Delete

This endpoint deletes a program from a Cloud Playout loop channel, together with the content that was added to it. The channel's remaining programs keep their order and the loop continues without the deleted block. This action cannot be undone.

#### How it works

1. Make a DELETE request to this endpoint, replacing `{channelId}` and `{programId}` with the UUIDs of the channel and the program.

2. FastPix removes the program along with the items in it. Any media that was split into segments for the program is cleaned up as well.

3. The response confirms that the program was deleted.

The media assets the program referenced are not affected and stay in your workspace, as do the overlays and fillers it used.

The deletion is not repeatable: a second DELETE for the same `programId` returns 404, which you can treat as the program already being gone.

#### When a program can be deleted

On a channel that is not yet on air, a program can be deleted at any time.

On a channel that is on air, a program cannot be deleted while it is playing or is about to play, and it cannot be deleted while any item on the channel is close to finishing. Wait for the channel to move past that point and try again.

#### Example

A broadcaster drops a seasonal block from a running channel, choosing a moment when that block is not on air and no item is about to end.

### Example Usage

<!-- UsageSnippet language="csharp" operationID="delete-cloud-playout-program" method="delete" path="/cloud-playout/channels/{channelId}/programs/{programId}" -->
```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

var res = await sdk.CloudPlayoutLoopPrograms.DeleteAsync(
    channelId: "<channelId>",
    programId: "<programId>"
);

// handle response
Console.WriteLine(
    JToken.Parse(
        JsonConvert.SerializeObject(
            res.CloudPlayoutProgramDeleteResponse,
            Utilities.GetDefaultJsonSerializerSettings()
        )
    ).ToString(Formatting.Indented)
);
```

### Parameters

| Parameter | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program, returned by FastPix when the program was created. | d85083a0-e935-4c29-9c32-6720bdc9b22f |

### Response

**[DeleteCloudPlayoutProgramResponse](../../Models/Requests/DeleteCloudPlayoutProgramResponse.md)**

### Errors

| Error Type                         | Status Code                        | Content Type                       |
| ---------------------------------- | ---------------------------------- | ---------------------------------- |
| Fastpix.Models.Errors.APIException | 4XX, 5XX                           | \*/\*                              |
