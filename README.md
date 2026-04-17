# Project Oni

**Project Oni** is a high-octane 2D Hack'n'Slash Platformer built with Unity. It features a modular, event-driven architecture designed for scalability and smooth combat gameplay.

---

## 🎮 Core Features

- **Fluid Combat System**: Precise hitbox/hurtbox detection and weapon-based data.
- **Dynamic AI**: Enemy state machines and modular behavior.
- **Responsive Platforming**: Tight controls for movement and combat integration.
- **Centralized Systems**: Managed audio, scene transitions, and game states.
- **Reactive UI**: Decoupled UI components that respond to game events.

---

## 🏗️ Architecture Overview

The project follows a modular approach to ensure systems are decoupled and easy to maintain:

- **Managers (Singleton Pattern)**: Persistent systems like `GameStateManager`, `AudioManager`, and `SceneController` handle global logic.
- **Event-Driven Design**: Uses `GameEvents` to communicate between disconnected systems (e.g., Player death triggering UI or Game Over state).
- **Data-Oriented approach with ScriptableObjects**: Character stats, weapon properties, and game settings are stored as `ScriptableObject` assets for easy iteration.
- **Component-Based Character Logic**: Players and enemies are built from reusable components (Damageable, Movement, Combat).

---

## 📁 Project Structure

The project is organized within `Assets/_Project` for a clean workflow:

```text
Assets/_Project/
├── Combat/         # Damageable, Hitboxes, Hurtboxes
├── Core/           # Global Managers (GameState, Audio, Scene)
├── Data/           # ScriptableObject Data Containers
├── Enemies/        # Enemy AI and State Logic
├── Player/         # Player Movement and Combat Logic
├── Scenes/         # Game Levels and Core UI Scene
├── Settings/       # URP and Input Action Assets
└── UI/             # HUD, Menus, and Reactive UI logic
```

---

## 🛠️ Technologies Used

- **Engine**: Unity 2023.x (Universal Render Pipeline)
- **Rendering**: URP for optimized 2D lighting and performance.
- **Input**: New **Unity Input System** for multi-device support.
- **Architecture**: C# Events and ScriptableObjects.

---

## 🚀 Getting Started

1. **Clone the Repository**:
   ```bash
   git clone <repository-url>
   ```
2. **Open with Unity**:
   - Open the project folder using **Unity Hub**.
   - Ensure the required packages (URP, Input System) are installed via the Package Manager.
3. **Core Scenes**:
   - Start with `Assets/_Project/Scenes/00_Boot.unity` to initialize all core systems.
   - The hierarchy should include the `[Systems]` container for persistent behavior.

---

## 📜 License

This project is currently private. All rights reserved.
