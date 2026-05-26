# PurrNet Reference Guide

This document contains a summary of the core concepts and latest features of PurrNet, based on the official documentation at [purrnet.dev/docs](https://purrnet.dev/docs).

## 🚀 Core Mechanics

### 1. No Baking
PurrNet does not require any "baking" of component IDs or scene objects. This means you can:
- Nest prefabs inside other prefabs.
- Change the hierarchy at runtime.
- Work with version control without constant merge conflicts on scene files.

### 2. Network Modules
Networking logic is encapsulated in `NetworkModule` classes.
- **Modularity**: Logic can exist outside of `MonoBehaviours`.
- **Sync Types**: `SyncVar<T>`, `SyncList<T>`, etc., are built on the module system.
- **Lifecycle**: Use `OnSpawn()` and `OnDespawned()` for initialization and cleanup.

### 3. State Synchronization

#### SyncVar<T>
Automatically synchronizes values across the network.
- **Buffering**: Handles late-joiners automatically.
- **OnChanged**: You can subscribe to value changes.
- **Built on NetworkModule**: Since `SyncVar` is built on `NetworkModule`, you can create your own custom networked types.

#### Buffered RPCs
For manually syncing state (like an animation state or game phase):
```csharp
[ObserversRpc(bufferLast: true)]
private void RpcSyncState(MyState newState) {
    // This will be called for all clients, including late joiners.
}
```

## 📡 Communication (RPCs)

- **ObserversRpc**: Server -> All Clients.
- **ServerRpc**: Client -> Server.
- **TargetRpc**: Server -> Specific Client.
- **Generic RPCs**: Supported for flexible data types.
- **Awaitable RPCs**: Support for `async/await` patterns.
- **Static RPCs**: Support for static method RPCs.

> [!NOTE]
> RPC attributes support the `runLocally: true` parameter (e.g., `[ServerRpc(runLocally: true)]`), which is a highly used feature for making RPCs execute instantly on the caller's side.

## ⚖️ Authority & Ownership

- **Network Rules**: Managed in the `NetworkManager` UI. You can toggle between `Everyone` (Client-Auth) and `Server Auth` globally or per-object without changing code.
- **isOwner**: True if the local instance owns the object (usually the local player).
- **isServer**: True if running on the server instance.

## 🛠️ Advanced Features

- **PurrDiction**: Client-side prediction for snappy movement.
- **Cookie System**: Persistent user data that survives disconnects.
- **Instantiate/Destroy**: Standard Unity methods are networked automatically for objects with a `NetworkIdentity`.

---
*Last Updated: 2026-05-13*
