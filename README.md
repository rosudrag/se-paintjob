# Space Engineers PaintJob plugin

SE PluginLoader plugin

Currently has a rather simple algorithm for a paint job:
- uses the top line from the color palette for non functional blocks
- uses teh bottom line from the color palette for functional blocks
- exterior lights are assigned starboard and port colors (green and red)

Working with plugin:
- the plugin uses your color palette to apply a paint job to the grid you are facing
- use the Build Color plugin to generate color palettes

Run command in chat:
```/paint run```

Known issue:
- some colors dont apply very well to functional or modded blocks.