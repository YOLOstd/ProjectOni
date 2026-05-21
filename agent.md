# Project Oni Agent Context

This project is a 2D hack'n'slash COOP platformer built on Unity using PurrNet for networking (authority: everyone) and the New Input System.

## Technical Priorities
- **Physics-Synced Logic**: Always use `FixedUpdate` and `Time.fixedDeltaTime` for core movement durations (dodges, dashes) to ensure frame-rate independent travel distances.
- **Strict Controls**: Wall-clinging and wall-jumping are input-dependent; the player must push toward the wall to engage these mechanics.
- **Terminology**: Use "Dodge" for the invulnerability/dash mechanic instead of "Dash".
- **Collision Detection**: Prefer `CapsuleCast` with physics layers over simple triggers for robust platformer physics.

## Core Systems
- **PlayerController**: Physics-driven movement with specialized ground/air states and standardized dodge logic.
- **PlayerMovementData**: ScriptableObject-based configuration for tuning movement feel.
- **PlayerAnimator**: Standardized event-driven animation system.

## Networking Strategy (PurrNet)
- **Everyone Authority Rules**: We prioritize "Optimistic UI" and instant responsiveness. Networking rules should generally allow "Everyone" (or the Owner) to modify sync data directly for zero-latency feedback on actions like equipping, moving, and state changes.
- **State Synchronization**: Use `SyncVar`, `SyncDictionary`, and `Buffered RPCs` (bufferLast: true) to ensure state consistency and late-join support.
- **Visuals**: Only the most essential visual indicators (like active weapons) should be synced to observers to minimize bandwidth.

## Combat Design
- **Weapon Attacks**: Melee and ranged attacks come from weapons equipped in weapon slots.
- **Spells**: Spells come from rings equipped in ring slots. They are a separate system from weapon attacks.

## AI Agent Instructions
- Follow Unity best practices for C# scripting and the New Input System.
- Maintain project structure and naming conventions.
- When tweaking movement "feel," ensure changes are exposed as parameters in `PlayerMovementData`.
- **Read Documentation**: Always refer to the `Docs` folder and specifically `PurrNet_Reference.md` for networking implementation details and project standards.
