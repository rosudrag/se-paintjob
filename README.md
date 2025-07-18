# Space Engineers PaintJob plugin

SE PluginLoader plugin that applies custom paint jobs to grids (ships/stations) using various styles and algorithms.

## Features

- Multiple paint job styles including classic, military, and specialized camouflage patterns
- Automatic color assignment based on block type and position
- Starboard (green) and port (red) navigation light coloring
- Unique color variations for each ship based on its ID
- Integration with Build Colors mod for custom palettes

## Usage

Point at any grid and use the following chat commands:

### Commands

```
/paint help                     Show help menu
/paint run                      Paint with rudimentary style (default)
/paint run rudimentary          Classic colorful paint job
/paint run military             Military green-grey camouflage
/paint run military stealth     Ultra-dark for stealth operations
/paint run military asteroid    Orange-brown asteroid camouflage
/paint run military industrial  Yellow-grey with hazard markings
/paint run military deep_space  Dark blue-purple for deep space
```

### Paint Styles

- **Rudimentary**: Classic algorithm using your current color palette
  - Top palette line for non-functional blocks
  - Bottom palette line for functional blocks
  - Automatic port/starboard light coloring
  
- **Military**: Various tactical camouflage patterns
  - **Standard**: Green-grey military camouflage
  - **Stealth**: Ultra-dark colors for reduced visibility
  - **Asteroid**: Orange-brown blend for asteroid fields
  - **Industrial**: Yellow-grey with hazard stripe accents
  - **Deep Space**: Dark blue-purple for space operations

Each ship receives unique color variations based on its grid ID, ensuring no two ships look exactly the same even with the same style.

## Tips

- Use the Build Colors mod to create custom color palettes for the rudimentary style
- Each paint style is optimized for different environments and purposes
- Military styles include strategic color placement for better camouflage
