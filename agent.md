# Project Oni Agent Context

This project is a 2D hack'n'slash platformer built on Unity using the New Input System.

## Technical Priorities
- **Physics-Synced Logic**: Always use `FixedUpdate` and `Time.fixedDeltaTime` for core movement durations (dodges, dashes) to ensure frame-rate independent travel distances.
- **Strict Controls**: Wall-clinging and wall-jumping are input-dependent; the player must push toward the wall to engage these mechanics.
- **Terminology**: Use "Dodge" for the invulnerability/dash mechanic instead of "Dash".
- **Collision Detection**: Prefer `CapsuleCast` with physics layers over simple triggers for robust platformer physics.

## Core Systems
- **PlayerController**: Physics-driven movement with specialized ground/air states and standardized dodge logic.
- **PlayerMovementData**: ScriptableObject-based configuration for tuning movement feel.
- **PlayerAnimator**: Standardized event-driven animation system.

## AI Agent Instructions
- Follow Unity best practices for C# scripting and the New Input System.
- Maintain project structure and naming conventions.
- When tweaking movement "feel," ensure changes are exposed as parameters in `PlayerMovementData`.
