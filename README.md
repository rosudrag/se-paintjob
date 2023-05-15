# Space Engineers PaintJob plugin

SE PluginLoader plugin

Currently has a rather simple algorithm for a paint job:
Rudimentary paint job:
- base block color factor: first color in the palette is the main color for non functional blocks and the others are round robin.
- exterior blocks get a darker shade of their base color
- edge blocks get a lighter shade of their base color
- exterior lights are assigned starboard and port colors (green and red)

Run command in chat to see how to use
```/paint```