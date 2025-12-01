# Space Engineers Paint Job - Skin System

## Overview

The skin system extends the paint job plugin to apply block skins (textures) in addition to colors. Skins can create visual effects like rust, camouflage, carbon fiber, and more.

## Architecture

### Core Components

- **ISkinProvider**: Interface for discovering and selecting skins
- **SkinManager**: Manages skin application to blocks
- **SkinPalette**: Stores available skins for a paint job
- **ISkinPainter**: Interface for different skin application strategies

### Integration with Paint Algorithms

The new `SkinAwarePaintAlgorithm` base class extends the original `PaintAlgorithm` to support both colors and skins:

```csharp
public class MyCustomPaintJob : SkinAwarePaintAlgorithm
{
    protected override void GenerateColorPalette(MyCubeGrid grid)
    {
        // Generate colors
    }
    
    protected override void InitializeSkinPainters()
    {
        // Add skin painters
        _skinPainters.Add(new PatternSkinPainter(_skinProvider));
    }
}
```

## Usage Example

```csharp
// Create enhanced military paint job with skins
var paintJob = new EnhancedMilitaryPaintJob();

// Configure variant and skin options
paintJob.SetVariant("desert", useAdaptiveCamo: true);
paintJob.EnableSkins = true;
paintJob.SkinTheme = "military";

// Apply to grid
paintJob.Run(grid);
```

## Available Skin Themes

- **military**: Camouflage, heavy armor, tactical skins
- **industrial**: Rusty, worn, corroded skins
- **racing**: Carbon fiber, neon, sport skins
- **alien**: Sci-fi, exotic, energy skins
- **stealth**: Dark, matte, tactical skins
- **desert**: Sand, dusty, weathered skins
- **arctic**: Frozen, icy, winter skins
- **urban**: Concrete, metal, city skins
- **jungle**: Moss, vegetation, natural skins

## Skin Painters

### PatternSkinPainter
Applies skins in geometric patterns:
- Stripes
- Checkerboard
- Gradient
- Random distribution

### MilitarySkinPainter
Military-specific skin application:
- Camouflage patches for armor
- Tactical skins for weapons
- Utility skins for functional blocks
- Accent skins for details

## Custom Skin Painters

Create custom skin painters by implementing `ISkinPainter`:

```csharp
public class CustomSkinPainter : ISkinPainter
{
    public string Name => "Custom Painter";
    public int Priority => 10;
    
    public void ApplySkins(
        MyCubeGrid grid, 
        Dictionary<Vector3I, MyStringHash> skinResults,
        SkinPalette skinPalette,
        object context)
    {
        // Custom skin application logic
    }
}
```

## Skin Selection Context

The `SkinSelectionContext` provides information for intelligent skin selection:

- Block position and type
- Selected color index
- Pattern type (camouflage, stripes, etc.)
- Zone classification (hull, interior, weapon)
- Random seed for consistency

## Known Skins

Common Space Engineers skins discovered:
- Rusty_Armor
- Heavy_Rust_Armor
- Golden_Armor
- Carbon_Armor
- Digital_Camo_Armor
- Moss_Armor
- Sand_Armor
- Silver_Armor
- Neon_Colorable_Surface
- Sci_Fi_Armor
- And many more...

## Technical Details

### Skin Application
Skins are applied using the game's `ChangeColorAndSkin()` method, which handles both color and skin changes atomically.

### Validation
Skins are validated through `MySessionComponentGameInventory.ValidateArmor()` to ensure players own the skins they're applying.

### Performance
The system caches skin discoveries and uses deterministic selection based on grid entity ID for consistency.

## Future Enhancements

- Dynamic skin discovery from mods
- Pattern recognition for adaptive skinning
- Skin preview system
- Custom skin pattern editor
- Skin sets and collections
- Conditional skin application based on environment