# Nokia Grinder

A first-person chicken-grinding doom clone rendered at 84x48 pixels in glorious 1-bit color, built for [Nokia Jam 7](https://itch.io/jam/nokiajam7).

**[Play on GitHub Pages](https://seanbouk.github.io/nokia-grinder/)** | **[Play on itch.io](https://seanofearth.itch.io/chicken-grinder)** (itch.io version may load faster as it serves compressed assets)

A Unity-based renderer that simulates the visual style of a Nokia 3310 display. This project provides a post-processing effect that transforms any Unity scene into the iconic 1-bit aesthetic of the Nokia 3310, complete with its 84x48 pixel resolution and limited color palette.

## Features

### Nokia 3310 Display Simulation
- **Resolution**: Forces rendering to 84x48 pixels, matching the Nokia 3310's display
- **1-bit Color**: Implements true 1-bit color depth with configurable thresholding
- **Pixel-Perfect**: Maintains crisp, pixelated rendering without blur
- **Multiple Color Palettes**: Three classic color schemes to choose from:
  - Classic Nokia (Light green/Dark green)
  - High Contrast Green (Bright green/Dark green)
  - Monochrome (Gray/Black)

### Controls & Gameplay
- **Movement**: WASD keys for movement (with wall collision)
- **Rotation**: Q and E keys to rotate left/right
- **Palette**: X key to cycle through color palettes
- **Combat**: 
  - Press 2 to shoot a fireball
  - Press 3 to shoot a collected roast (requires at least 1 roast)
- **Health System**: 
  - Player starts with 99 health
  - Lose 15 health when hit by a chicken
  - Screen flashes when taking damage
  - Game pauses when health reaches 0
- **UI Display**:
  - Shows current health (0-99)
  - Shows number of roasts collected
  - Shows score (placeholder "00")

### Wall Collision System
The camera includes a wall collision system that prevents clipping through walls:
- Automatically detects and slides along walls
- Adjusts movement speed when sliding
- Includes debug visualization in the Scene view
- Configurable detection distance and slide sensitivity

## Technical Details

### Components

#### Nokia1BitShader
A custom shader that:
- Forces rendering to an 84x48 pixel grid
- Implements 1-bit color depth using a threshold
- Maintains pixel-perfect scaling
- Supports configurable color palettes

#### Post-Processing
The Nokia1BitPostProcess component applies the shader effect to the final rendered image, ensuring everything in the game maintains the Nokia 3310 aesthetic.

### Code Components

#### Fireball.cs
Controls the behavior of a fireball object:
- Moves in the direction the player is facing
- Self-destructs when too far from the player
- Detects and handles wall collisions
- Destroys chickens on contact and spawns a roast
- Maintains constant Y-position
- Configurable roast prefab in inspector

#### CameraController.cs (NokiaInputHandler)
Handles player movement and interaction:
- WASD movement with wall collision and sliding
- Q/E rotation controls
- X key for cycling color palettes
- Implements wall collision detection and sliding
- Maintains constant Y-position

#### WorldObject.cs
Handles sprite-based objects in the world:
- Billboard rotation to face camera
- Texture animation system
- Random UV rotation for visual variety
- Configurable animation frame duration

#### Chicken.cs
Implements enemy AI behavior:
- State machine (Waiting, Attacking, Resting)
- Player tracking and collision
- Wall avoidance system
- Bouncing movement animation
- Configurable attraction and collision distances

#### Nokia1BitPostProcess.cs
Camera post-processing component:
- Applies the Nokia 3310 shader effect
- Handles depth texture setup
- Manages render texture pipeline

#### GameManager.cs
Singleton component that tracks game state:
- Chicken management:
  - Maintains accurate count of chickens in game
  - Updates count automatically via Sanders' periodic checks
  - Self-corrects if count gets out of sync
  - Logs chicken count changes to console
- Roast tracking:
  - Tracks active roasts in the game world
  - Counts collected roasts (when player touches them)
  - Tracks ground roasts for score
- Health system:
  - Manages player health (starts at 99, decreases by 15 when hit by chickens)
  - Controls screen flash effect when player takes damage
  - Pauses game when health reaches 0
- UI management:
  - Updates health display
  - Shows roasts collected
  - Displays current score
- Debug features:
  - Logs all state changes to console
  - Tracks and reports game statistics

#### RandomRotateEditor.cs
Unity editor tool for level design:
- Randomly rotates leaf objects in 60° increments
- Supports undo operations
- Useful for adding variety to repeated elements

#### ConstantRotation.cs
Utility component for continuous object rotation:
- Rotates object around world Y axis
- Configurable rotation speed
- Frame-rate independent movement
- Useful for rotating pickups or decorative elements

#### Sanders.cs
Manages chicken spawning and population control:
- Implements time-based chicken count schedule:
  - 1 chicken at 0 seconds
  - 2 chickens at 15 seconds
  - 5 chickens at 30 seconds
  - 10 chickens at 60 seconds
- Performs checks every 2 seconds:
  - Calculates required chicken count (floor of lerp between schedule points)
  - Spawns one chicken if current count is below required
  - Selects spawn location from available eggs
- Smart egg selection:
  - Chooses nearest egg to player without line of sight
  - Tracks chicken-egg relationships
  - Only reuses eggs after their spawned chicken has attacked
- Console logging for debugging:
  - Current and required chicken counts
  - Spawn events with egg locations
- Required references:
  - Container object for all chickens
  - Container object for all eggs
  - Chicken prefab
  - Wall layer mask for line of sight checks

#### Roast.cs
Controls the behavior of roasted chicken pickups with three states:
- ATTRACT state:
  - Moves towards the player with configurable initial speed
  - Accelerates over time at a configurable rate
  - Can be collected when close enough to the player
  - Maintains movement in the XZ plane
- REPEL state:
  - Moves away from player when shot (using 3 key)
  - Travels at constant speed
  - Switches back to ATTRACT state when hitting a wall
  - Can only be collected in ATTRACT state
- GRIND state:
  - Triggered when a roast in REPEL state hits the grinder henge
  - Maintains horizontal movement from REPEL state
  - Accelerates downward at configurable rate
  - Destroys itself after 0.5 seconds
  - Adds to player's score when destroyed

### Shaders

#### Nokia1BitShader.shader
Core rendering shader:
- Implements 84x48 pixel resolution
- 1-bit color depth with threshold
- Configurable color palette
- Mosaic effect for pixel-perfect rendering

#### LitTransparentShader.shader
Utility shader for transparent objects:
- Standard lit transparency
- Automatic transparency for dark pixels
- Double-sided rendering
- Proper alpha blending

#### UnlitBlackTransparent.shader
Special effects shader:
- Unlit transparency
- Configurable UV rotation
- Black pixel transparency
- Used for particle effects and sprites

#### ViewAngleLit.shader
Self-lit view-dependent shader:
- Base color configurable in inspector
- Darkens surfaces at steep angles to camera
- Adjustable darkening amount
- No external lighting required

#### WebGL Template
A custom template that:
- Displays the game with the correct Nokia 3310 aspect ratio
- Scales the display while maintaining pixel-perfect rendering
- Centers the game on the webpage with a Nokia 3310 background

### Color Palettes
Three built-in color palettes:
```
Classic Nokia:    #c7f0d8 / #43523d
High Contrast:    #9bc700 / #2b3f09
Monochrome:       #879188 / #1a1914
```

## Setup

### Camera Setup
1. Add the Nokia1BitPostProcess component to your camera
2. Assign the Nokia1BitMaterial to the EffectMaterial field
3. Adjust the threshold value in the material if needed (default: 0.5)

### Wall Collision Setup
4. Configure the camera object:
   - Add a Rigidbody component
   - Set "Use Gravity" to false
   - Set "Is Kinematic" to true
5. For any walls in the scene:
   - Add a MeshCollider component
   - Enable "Convex" on the MeshCollider

### WebGL Setup
6. Use the NokiaTemplate to maintain proper scaling and aspect ratio

### Game State Setup
7. Create an empty GameObject in your scene (name it "GameManager")
8. Add the GameManager component to it
   - Assign Nokia1BitMaterial to enable damage flash effect
   - Assign UI text references for health, roasts, and score
   - Automatically counts chickens at start
   - Tracks player health (99 max, -15 per chicken hit)
   - Logs state changes to console (health, chickens, roasts)
   - Disables player movement on death

### Tutorial Setup
9. Create a Canvas in your scene for the tutorial UI
10. Add the Tutorial component to it
    - Assign UI references for text elements (forwards, shoot, grind, game over)
    - Assign reference to the grinder henge
    - Assign reference to the GameManager
    - Set the Wall layer for wall detection
    - Set the Default layer for grinder detection

The tutorial guides players through core mechanics:
1. Moving forward (W key)
2. Shooting fireballs (2 key)
3. Finding the grinder henge (requires line of sight)
4. Grinding roasts for score (requires collected roast)
5. Game over state on death

### Chicken Spawning Setup
11. Create two empty GameObjects in your scene:
    - One to contain all chickens (name it "ChickenContainer")
    - One to contain all eggs (name it "EggContainer")
12. Add the Sanders component to a GameObject
    - Assign the ChickenContainer reference
    - Assign the EggContainer reference
    - Assign the Chicken prefab
    - Set the wall layer mask (same as used by chickens)

## Usage

The effect will automatically apply to everything rendered by the camera. The scene will be:
1. Rendered normally by Unity
2. Downscaled to 84x48 pixels
3. Converted to 1-bit color using the current palette
4. Displayed with pixel-perfect scaling

Use the X key during gameplay to cycle through the available color palettes.
