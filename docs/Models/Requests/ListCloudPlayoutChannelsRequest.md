# ListCloudPlayoutChannelsRequest


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Type` | [ListCloudPlayoutChannelsType](../../Models/Requests/ListCloudPlayoutChannelsType.md) | :heavy_check_mark: | The kind of channel to list. Use `all` to list both `loop` and `schedule` channels. | all |
| `Limit` | *long* | :heavy_minus_sign: | Limit specifies the maximum number of items to display per page. | 20 |
| `Offset` | *long* | :heavy_minus_sign: | Offset determines the starting point for data retrieval within a paginated list. | 1 |
| `OrderBy` | [SortOrder](../../Models/Components/SortOrder.md) | :heavy_minus_sign: | The sort order of the channels, based on the time each channel was created. |  |
