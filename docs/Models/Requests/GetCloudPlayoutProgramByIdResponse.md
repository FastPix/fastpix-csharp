# GetCloudPlayoutProgramByIdResponse


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `HttpMeta` | [HTTPMetadata](../../Models/Components/HTTPMetadata.md) | :heavy_check_mark: | N/A |  |
| `CloudPlayoutProgramResponse` | [CloudPlayoutProgramResponse](../../Models/Components/CloudPlayoutProgramResponse.md) | :heavy_minus_sign: | Program details fetched successfully | {<br/>  "success": true,<br/>  "data": {<br/>    "id": "d85083a0-e935-4c29-9c32-6720bdc9b22f",<br/>    "index": 10,<br/>    "name": "Morning block",<br/>    "channelId": "cc99cae6-56b4-46ab-9f4a-8b2462d8137c",<br/>    "mediaCount": 0,<br/>    "duration": "00:00:00",<br/>    "workspaceId": "37d62550-e2a5-4827-b16a-3826e66d77ad",<br/>    "createdAt": "2025-03-12T06:17:26.403017Z",<br/>    "updatedAt": "2025-03-12T06:17:26.403017Z"<br/>  }<br/>} |
| `DefaultError` | [DefaultError](../../Models/Components/DefaultError.md) | :heavy_minus_sign: | See the range of possible <a href="https://fastpix.com/docs/error-codes/error-codes">error</a> responses and their status codes. |  |
