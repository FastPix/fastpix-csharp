# AddOverlayToCloudPlayoutChannelRequest


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the channel the overlay is placed on. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Body` | [CloudPlayoutChannelOverlayRequest](../../Models/Components/CloudPlayoutChannelOverlayRequest.md) | :heavy_check_mark: | N/A | {<br/>  "overlayId": "2f6c1d84-9b3a-4d0e-8f21-5c7e3a9b4d16",<br/>  "startTime": "2026-09-02T18:00:00",<br/>  "duration": 600,<br/>  "position": true,<br/>  "positionX": 40,<br/>  "positionY": 40,<br/>  "opacity": 100<br/>} |
