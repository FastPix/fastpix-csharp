# DeleteCloudPlayoutProgramItemsRequest


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program the items are removed from. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `Body` | [CloudPlayoutProgramItemDeleteRequest](../../Models/Components/CloudPlayoutProgramItemDeleteRequest.md) | :heavy_check_mark: | N/A | {<br/>  "eventIds": [<br/>    "4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90",<br/>    "7b4d9a12-5f3c-4c8e-9d21-3a7e2c5b1f04"<br/>  ]<br/>} |
