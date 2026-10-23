# UpdateCloudPlayoutChannelOverlayRequest


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel the overlay is placed on. Loop and schedule channels are both accepted. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `OverlayConfigId` | *string* | :heavy_check_mark: | The unique identifier of the placement to update, as returned in the `id` field by the get all overlays on a channel endpoint. | 5e8b2c31-7a49-4d62-b0f8-9c1e4a7d2b53 |
| `Body` | [CloudPlayoutChannelOverlayUpdateRequest](../../Models/Components/CloudPlayoutChannelOverlayUpdateRequest.md) | :heavy_check_mark: | N/A | {<br/>  "position": true,<br/>  "positionX": 60,<br/>  "positionY": 60,<br/>  "opacity": 80<br/>} |
