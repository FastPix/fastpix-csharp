# UpdateCloudPlayoutChannelOverlayResponse


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `HttpMeta` | [HTTPMetadata](../../Models/Components/HTTPMetadata.md) | :heavy_check_mark: | N/A |  |
| `CloudPlayoutChannelOverlaySingleResponse` | [CloudPlayoutChannelOverlaySingleResponse](../../Models/Components/CloudPlayoutChannelOverlaySingleResponse.md) | :heavy_minus_sign: | Overlay updated successfully | {<br/>  "success": true,<br/>  "data": {<br/>    "id": "5e8b2c31-7a49-4d62-b0f8-9c1e4a7d2b53",<br/>    "overlayId": "2f6c1d84-9b3a-4d0e-8f21-5c7e3a9b4d16",<br/>    "channelId": "cc99cae6-56b4-46ab-9f4a-8b2462d8137c",<br/>    "name": "Channel logo",<br/>    "type": "static",<br/>    "overlayLayoutType": "logo",<br/>    "startTime": "2026-09-02T18:00:00Z",<br/>    "endTime": "2026-09-02T18:10:00Z",<br/>    "duration": "00:10:00",<br/>    "untilChannelEnd": false,<br/>    "customResolution": false,<br/>    "width": 100,<br/>    "height": 100,<br/>    "position": true,<br/>    "positionX": 60,<br/>    "positionY": 60,<br/>    "opacity": 80,<br/>    "repeat": {<br/>      "everySeconds": 300,<br/>      "showForSeconds": 30<br/>    }<br/>  }<br/>} |
| `DefaultError` | [DefaultError](../../Models/Components/DefaultError.md) | :heavy_minus_sign: | See the range of possible <a href="https://fastpix.com/docs/error-codes/error-codes">error</a> responses and their status codes. |  |
