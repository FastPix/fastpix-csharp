# AddMediaToCloudPlayoutProgramRequest


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program the media is added to. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `Body` | [CloudPlayoutProgramMediaRequest](../../Models/Components/CloudPlayoutProgramMediaRequest.md) | :heavy_check_mark: | N/A | {<br/>  "mediaIds": [<br/>    "398edd49-322c-4f06-b596-37ba7ddff0be",<br/>    "075bf22c-6261-4dc4-9795-83374ff83495"<br/>  ]<br/>} |
