# DeleteCloudPlayoutChannelSimulcastRequest


## Fields

| Field | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `LivestreamId` | *string* | :heavy_check_mark: | The unique identifier of the stream behind the channel, returned as `destinationLiveId` by the get a channel's simulcast targets endpoint. | 8717422d89288ad5958d4a86e9afe2a2 |
| `SimulcastId` | *string* | :heavy_check_mark: | The unique identifier of the simulcast target to remove. | 5f2c8e1b7a934d06b8e4c1a3d9f07e52 |
