# CloudPlayoutChannelStatus

The current status of the channel. A newly created channel is in `draft`. Activating it moves it to `scheduled`; at its start time it passes through `starting` to `running`, and it moves to `completed` when it reaches its end time. Deactivating a channel moves it through `stopping` to `idle`, from which it can be activated again. `deleting` is the transient state while a delete request is processed, and `error` is reported when playout could not be started or was interrupted.


## Values

| Name | Value |
| --- | --- |
| `Draft` | draft |
| `Scheduled` | scheduled |
| `Starting` | starting |
| `Running` | running |
| `Stopping` | stopping |
| `Idle` | idle |
| `Completed` | completed |
| `Deleting` | deleting |
| `Error` | error |

The enum is open: unknown server values are preserved. `IsKnown` is false and `Value` holds the raw string.
