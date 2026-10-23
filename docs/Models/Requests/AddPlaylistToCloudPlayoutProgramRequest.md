# AddPlaylistToCloudPlayoutProgramRequest


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `ChannelId` | *string* | :heavy_check_mark: | The unique identifier of the loop channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `ProgramId` | *string* | :heavy_check_mark: | The unique identifier of the program the playlist is added to. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `Body` | [CloudPlayoutProgramPlaylistRequest](../../Models/Components/CloudPlayoutProgramPlaylistRequest.md) | :heavy_check_mark: | N/A | {<br/>  "playlistId": "7b2a1f4c-3d5e-4a1b-8c9d-2e6f4a7b1c30"<br/>} |
