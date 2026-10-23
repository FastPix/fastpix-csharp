# CloudPlayoutProgram

A program on a Cloud Playout loop channel.


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Id` | *string* | :heavy_minus_sign: | The unique identifier FastPix assigns to the program. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `Index` | *double* | :heavy_minus_sign: | The program's position in the channel's loop. Programs are played in ascending order of index. | 10 |
| `Name` | *string* | :heavy_minus_sign: | The name given to the program. | Morning block |
| `ChannelId` | *string* | :heavy_minus_sign: | The unique identifier of the channel the program belongs to. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `MediaCount` | *long* | :heavy_minus_sign: | The number of items currently in the program. | 0 |
| `Duration` | *string* | :heavy_minus_sign: | The combined duration of the program's content, formatted as `HH:MM:SS`, or `HH:MM:SS.mmm` when the total includes a fraction of a second. | 00:00:00 |
| `WorkspaceId` | *string* | :heavy_minus_sign: | The unique identifier of the workspace the program was created in. | 37d62550-e2a5-4827-b16a-3826e66d77ad |
| `CreatedAt` | [DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0) | :heavy_minus_sign: | Time the program was created, defined as a localDateTime (UTC Time). | 2025-03-12T06:17:26.403017Z |
| `UpdatedAt` | [DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0) | :heavy_minus_sign: | Time the program was last updated, defined as a localDateTime (UTC Time). | 2025-03-12T06:17:26.403017Z |
