# AddCloudPlayoutChannelSimulcastResponse


## Fields

| Field | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `HttpMeta` | [HTTPMetadata](../../Models/Components/HTTPMetadata.md) | :heavy_check_mark: | N/A |  |
| `CloudPlayoutSimulcastCreateResponse` | [CloudPlayoutSimulcastCreateResponse](../../Models/Components/CloudPlayoutSimulcastCreateResponse.md) | :heavy_minus_sign: | Simulcast target added successfully | {<br/>"success": true,<br/>"data": {<br/>"simulcastId": "5f2c8e1b7a934d06b8e4c1a3d9f07e52",<br/>"url": "rtmp://a.rtmp.youtube.com/live2/",<br/>"streamKey": "abcd-1234-efgh-5678",<br/>"isEnabled": true<br/>}<br/>} |
| `DefaultError` | [DefaultError](../../Models/Components/DefaultError.md) | :heavy_minus_sign: | See the range of possible <a href="https://fastpix.com/docs/error-codes/error-codes">error</a> responses and their status codes. |  |
