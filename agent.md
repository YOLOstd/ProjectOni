# Project Oni Agent Context

This project is a 2D hack'n'slash COOP platformer built on Unity using PurrNet for networking (authority: everyone) and the New Input System. Inspired by Path of Exile

## Technical Priorities

- **Physics-Synced Logic**: Always use `FixedUpdate` and `Time.fixedDeltaTime` for core movement durations (dodges, dashes) to ensure frame-rate independent travel distances.
- **Strict Controls**: Wall-clinging and wall-jumping are input-dependent; the player must push toward the wall to engage these mechanics.
- **Terminology**: Use "Dodge" for the invulnerability/dash mechanic instead of "Dash".
- **Collision Detection**: Prefer `CapsuleCast` with physics layers over simple triggers for robust platformer physics.
- **VFX & Projectile Flipping**: Symmetrical horizontal flipping/mirroring of visual effects and projectiles **MUST** be handled via Y-axis rotation (multiplying rotation by `Quaternion.Euler(0, 180, 0)` when facing left) instead of modifying local scale.

## Core Systems

- **PlayerController**: Physics-driven movement with specialized ground/air states and standardized dodge logic.
- **PlayerMovementData**: ScriptableObject-based configuration for tuning movement feel.
- **PlayerAnimator**: Standardized event-driven animation system.
- **CombatAnimator**: Resolves local visual and sound asset paths, handles horizontal mirroring, offset flipping, and network visual hint reproduction.

## Networking Strategy (PurrNet)

- **Everyone Authority Rules**: We prioritize "Optimistic UI" and instant responsiveness. Networking rules should generally allow "Everyone" (or the Owner) to modify sync data directly for zero-latency feedback on actions like equipping, moving, and state changes.
- **State Synchronization**: Use `SyncVar`, `SyncDictionary`, and `Buffered RPCs` (bufferLast: true) to ensure state consistency and late-join support.
- **Visuals**: Only the most essential visual indicators (like active weapons) should be synced to observers to minimize bandwidth.

## Combat & Skill Architecture

- **CombatSkill Conduit Layer**: Traits (`WeaponTrait` and `SpellTrait`) are separated from raw attack node trees by a conduit layer (`WeaponSkill` and `SpellSkill` inheriting from `CombatSkill`).
- **SkillTag System**: Categorization and identity filters are handled using the `SkillTag` bitwise flags enum (e.g. `Melee`, `Spell`, `Slash`, `Thrust`, `Fire`, `Ice`, `Physical`, `Magical`). Buffs, items, and passives query these tags to dynamically scale stats.
- **Combo State Machine Pacing**: 
  - **Dynamic Transitions**: Pacing transitions to next combo nodes (e.g., `normalNextNode`) is driven by the actual (scaled by speed stat) `GlobalLockTime` of the active `AttackNode`.
  - **Spam & Hold Loop**: Holding the button or buffering a click instantly chains the next strike upon global lock expiration. If no next node exists (end of combo), it resets the combo and instantly executes the root strike of the slot.
  - **Global Reset Timeout**: A serialized `_comboResetDelay` parameter on `CombatController` dictates the idle reset threshold back to neutral after the global lock has expired.

## Negative Guidelines (Negative Prompt)

When developing features or fixing bugs in Project Oni, **NEVER** violate the following constraints:

1. **DO NOT use negative scale to flip visuals**: 
   - Never set local scale components (e.g., `transform.localScale.x = -1`) to flip VFX, projectiles, or character models horizontally. This breaks lighting normals, disrupts physics colliders, and corrupts Unity Particle System rotations. Use `Quaternion.Euler(0, 180, 0)` multiplication.
2. **DO NOT lock players with static totalDuration**:
   - Never use a hardcoded static timer (like the raw `totalDuration` of an `AttackNode`) to lock player inputs or block combat progression if the scaled `GlobalLockTime` is meant to determine recovery and spam pacing.
3. **DO NOT use legacy data classes**:
   - Never reference or restore deleted legacy ScriptableObjects (`AttackDataSO`, `MeleeAttackDataSO`, `RangedAttackDataSO`, `SpellAttackDataSO`). All combat assets must utilize the unified `AttackNode` tree system.
5. **DO NOT ignore network authority**:
   - Never write RPCs or sync procedures that limit client authority in a way that introduces input delay or lags client-side feedback (Optimistic UI is key).

## AI Agent Instructions

- Follow Unity best practices for C# scripting and the New Input System.
- Maintain project structure and naming conventions.
- When tweaking movement "feel," ensure changes are exposed as parameters in `PlayerMovementData`.
- **Read Documentation**: Always refer to the `Docs` folder and specifically `PurrNet_Reference.md` for networking implementation details and project standards.
