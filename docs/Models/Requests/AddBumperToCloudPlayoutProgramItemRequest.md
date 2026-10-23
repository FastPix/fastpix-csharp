# AddBumperToCloudPlayoutProgramItemRequest


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program the bumper is added to. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `Body` | [CloudPlayoutProgramBumperRequest](../../Models/Components/CloudPlayoutProgramBumperRequest.md) | :heavy_check_mark: | N/A | {<br/>  "bumperId": "9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88",<br/>  "eventId": "4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90",<br/>  "bumperType": "preroll"<br/>} |
