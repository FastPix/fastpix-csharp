# GetCloudPlayoutChannelScheduleResponse


## Fields

| Field | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `HttpMeta` | [HTTPMetadata](../../Models/Components/HTTPMetadata.md) | :heavy_check_mark: | N/A |  |
| `CloudPlayoutChannelScheduleResponse` | [CloudPlayoutChannelScheduleResponse](../../Models/Components/CloudPlayoutChannelScheduleResponse.md) | :heavy_minus_sign: | Channel timeline retrieved successfully | {<br/>"success": true,<br/>"data": {<br/>"schedule": [<br/>{<br/>"id": "8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21",<br/>"name": "Morning headlines",<br/>"type": "vod",<br/>"startTime": "2026-09-02T18:00:00Z",<br/>"endTime": "2026-09-02T18:10:30Z",<br/>"duration": "00:10:30"<br/>}<br/>],<br/>"videoCount": 1<br/>}<br/>} |
| `DefaultError` | [DefaultError](../../Models/Components/DefaultError.md) | :heavy_minus_sign: | See the range of possible <a href="https://fastpix.com/docs/error-codes/error-codes">error</a> responses and their status codes. |  |
