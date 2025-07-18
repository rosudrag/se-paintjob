# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

PaintJob is a Space Engineers plugin that applies custom paint jobs to game grids (ships/stations) using color palettes. It integrates with the SE PluginLoader system and uses Harmony for runtime patching.

## Development Setup

Before opening the solution, run:
```bash
Edit-and-run-before-opening-solution.bat
```
This creates required symbolic links to the Space Engineers game directory.

## Common Commands

### Build and Deploy
```bash
# Build the project (auto-deploys via post-build event)
msbuild PaintJobPlugin.sln /p:Configuration=Release

# Clean build artifacts
Clean.bat

# Deploy manually (usually not needed - handled by post-build)
deploy.bat
```

### Testing
```bash
# Kill Space Engineers for quick restart
Kill-Space-Engineers.bat

# In-game command to run paint job
/paint run
```

## Architecture Overview

### Core Components

1. **Plugin Entry Point**: `Plugin.cs`
   - Implements `IPlugin` interface
   - Initializes IoC container and registers services
   - Manages plugin lifecycle (Init, Update, Dispose)

2. **Dependency Injection**: `IoC.cs` + `SimpleIoC.cs`
   - Custom lightweight IoC container
   - All services registered in `Plugin.Init()`

3. **Main Application**: `PaintApp.cs`
   - Coordinates painting operations
   - Manages grid selection and paint job execution

4. **Paint System**:
   - `PaintJob.cs`: Core painting logic
   - `RudimentaryPaintJob.cs`: Main painting algorithm implementation
   - Color Factors (modular system):
     - `BaseBlockColorFactor`: Base color determination
     - `BlockExposureColorFactor`: Colors based on block exposure
     - `EdgeBlockColorFactor`: Special handling for edge blocks
     - `LightBlockColorFactor`: Port/starboard light coloring

5. **Command System**: `CommandInterpreter.cs`
   - Processes chat commands (`/paint` prefix)
   - Routes to appropriate handlers

6. **State Management**: `State.cs` + `IStateManager.cs`
   - Persists paint job configurations
   - Saves to user data directory

### Key Patterns

- **Factory Pattern**: Color factors use factory pattern for creation
- **Strategy Pattern**: Different paint algorithms can be implemented
- **Command Pattern**: Chat command processing
- **Dependency Injection**: All major components injected via IoC

## Important Development Notes

1. **Game References**: All Space Engineers DLLs are referenced from the symlinked `Bin64` folder
2. **Harmony Patches**: Located in `Shared/Patches/` - modify game behavior at runtime
3. **Configuration**: 
   - Plugin config in `PluginConfig.cs`
   - Assembly redirects in `App.config` for compatibility
4. **Debugging**: Use the Rider run configuration in `.run/Plugin Loader Launcher.run.xml`

## File Locations

- **Plugin Deployment**: `..\..\..\Bin64\Plugins\Local\`
- **User Data**: `%AppData%\SpaceEngineers\PluginData\PaintJob\`
- **Game Installation**: Symlinked via `Bin64` folder

## Testing Approach

Currently no automated tests. Testing is done manually in-game:
1. Build and deploy the plugin
2. Launch Space Engineers with PluginLoader
3. Load a world with a grid
4. Execute `/paint run` command
5. Verify paint job applied correctly

## Common Tasks

### Adding a New Color Factor
1. Create new class inheriting from `BaseBlockColorFactor` in `PaintFactors/`
2. Implement `GetColor()` method
3. Register in IoC container if needed
4. Add to paint algorithm as required

### Adding a New Command
1. Add command handler in `CommandInterpreter.cs`
2. Follow existing pattern for command parsing
3. Update help text if applicable

### Modifying Paint Algorithm
1. Main algorithm in `RudimentaryPaintJob.cs`
2. Implement `IPaintJobFactory` for new algorithms
3. Register new implementation in IoC container