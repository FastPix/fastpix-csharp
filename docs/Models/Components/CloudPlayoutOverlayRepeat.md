# CloudPlayoutOverlayRepeat

Optional recurrence for a `static` overlay placement. Without it the overlay stays on air for the whole placement window.


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `EverySeconds` | *long* | :heavy_check_mark: | How often the overlay comes back on air, in seconds, measured from one appearance to the next and anchored at `startTime`. | 300 |
| `ShowForSeconds` | *long* | :heavy_check_mark: | How long the overlay stays on air each time, in seconds. It must be shorter than `everySeconds`. The last appearance is cut short at the end of the placement window rather than dropped. | 30 |
