# CreateCloudPlayoutChannelResponse


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `HttpMeta` | [HTTPMetadata](../../Models/Components/HTTPMetadata.md) | :heavy_check_mark: | N/A |  |
| `CloudPlayoutChannelResponse` | [CloudPlayoutChannelResponse](../../Models/Components/CloudPlayoutChannelResponse.md) | :heavy_minus_sign: | Channel created successfully | {<br/>"success": true,<br/>"data": {<br/>"id": "cc99cae6-56b4-46ab-9f4a-8b2462d8137c",<br/>"name": "News 24x7",<br/>"ingestionPoint": "rtmps://live.fastpix.com:443/live/493e43e13ac83bb49672403f0fba52b8k17eed9dd1c9c18fe8177d5b547007ef4",<br/>"startTime": "2026-11-01T00:00:00Z",<br/>"endTime": "2026-11-08T00:00:00Z",<br/>"liveSupport": true,<br/>"status": "draft",<br/>"workspaceId": "37d62550-e2a5-4827-b16a-3826e66d77ad",<br/>"createdAt": "2026-10-12T06:17:26.403017Z",<br/>"updatedAt": "2026-10-12T06:17:26.403017Z",<br/>"slateId": "default",<br/>"type": "loop",<br/>"captions": false,<br/>"encodingTier": "standard",<br/>"playbackUrl": "https://stream.fastpix.com/d6bf9101-7d5f-4247-b5a4-0ffc91896ba3.m3u8",<br/>"inputPlaybackUrl": "https://stream.fastpix.com/396ce915-0978-4d49-a662-06c7f975dd6b.m3u8",<br/>"destinationUrl": "rtmps://live.fastpix.com:443/live/9150bf73432f52483b4b819237eab3afk41b0002bba7a1f971fbada1cc746a04f",<br/>"destinationLiveId": "41b0002bba7a1f971fbada1cc746a04f",<br/>"inputLiveId": "17eed9dd1c9c18fe8177d5b547007ef4"<br/>}<br/>} |
| `DefaultError` | [DefaultError](../../Models/Components/DefaultError.md) | :heavy_minus_sign: | See the range of possible <a href="https://fastpix.com/docs/error-codes/error-codes">error</a> responses and their status codes. |  |
