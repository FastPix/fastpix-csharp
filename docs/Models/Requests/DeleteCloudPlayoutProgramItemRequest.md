# DeleteCloudPlayoutProgramItemRequest


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program the item is removed from. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `EventId` | *string* | :heavy_check_mark: | The unique identifier of the item to remove, as returned in the `id` field by the get items in a program endpoint. | 4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90 |
