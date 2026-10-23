# GetCloudPlayoutChannelSimulcastsResponse


## Fields

| Field | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `HttpMeta` | [HTTPMetadata](../../Models/Components/HTTPMetadata.md) | :heavy_check_mark: | N/A |  |
| `CloudPlayoutSimulcastListResponse` | [CloudPlayoutSimulcastListResponse](../../Models/Components/CloudPlayoutSimulcastListResponse.md) | :heavy_minus_sign: | Simulcast targets retrieved successfully | {<br/>"success": true,<br/>"data": {<br/>"destinationLiveId": "8717422d89288ad5958d4a86e9afe2a2",<br/>"playbackId": "4fd9d0a8-1c3e-4b2f-9a6d-7c5e8b0f2a41",<br/>"simulcastResponses": [<br/>{<br/>"simulcastId": "5f2c8e1b7a934d06b8e4c1a3d9f07e52",<br/>"url": "rtmp://a.rtmp.youtube.com/live2/",<br/>"streamKey": "abcd-1234-efgh-5678",<br/>"isEnabled": true<br/>}<br/>]<br/>}<br/>} |
| `DefaultError` | [DefaultError](../../Models/Components/DefaultError.md) | :heavy_minus_sign: | See the range of possible <a href="https://fastpix.com/docs/error-codes/error-codes">error</a> responses and their status codes. |  |
