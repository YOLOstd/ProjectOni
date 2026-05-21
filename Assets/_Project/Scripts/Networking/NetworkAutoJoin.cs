using PurrNet;
using PurrNet.Transports;
using UnityEngine;

namespace ProjectOni.Networking
{
    /// <summary>
    /// Automatically connects a client instance to a specified room name in PurrNet.
    /// Supports ParrelSync clone detection, command-line arguments, and forced inspector configurations.
    /// </summary>
    [DefaultExecutionOrder(0)]
    public class NetworkAutoJoin : MonoBehaviour
    {
        [Header("Room Configuration")]
        [Tooltip("The room/code to automatically join.")]
        [SerializeField] private string _roomCode = "didjecte";

        [Header("Detection Settings")]
        [Tooltip("Force this instance to behave as a client in the Unity Editor.")]
        [SerializeField] private bool _forceClientInEditor = false;

        [Tooltip("Automatically detect and connect if running inside a ParrelSync clone editor.")]
        [SerializeField] private bool _checkParrelSync = true;

        [Tooltip("Automatically detect and connect if the executable was started with the '-client' or '--client' command line arguments.")]
        [SerializeField] private bool _checkCommandLine = true;

        private void Start()
        {
            if (NetworkManager.main == null)
            {
                Debug.LogWarning("[NetworkAutoJoin] No NetworkManager found in the scene.");
                return;
            }

            // If the network is already running, do nothing to prevent interrupting existing connections
            if (NetworkManager.main.isServer || NetworkManager.main.isClient)
            {
                Debug.Log("[NetworkAutoJoin] Network is already running. Auto-connect skipped.");
                return;
            }

            if (ShouldAutoConnectAsClient())
            {
                Debug.Log($"[NetworkAutoJoin] Client instance detected! Automatically connecting to room: '{_roomCode}'");
                SetRoomName(_roomCode);
                NetworkManager.main.StartClient();
            }
        }

        /// <summary>
        /// Evaluates the active detection strategies to determine if this instance should auto-connect as a client.
        /// </summary>
        private bool ShouldAutoConnectAsClient()
        {
            // 1. Forced configuration for developer testing in the main editor
            if (_forceClientInEditor && Application.isEditor)
            {
                Debug.Log("[NetworkAutoJoin] Client connection forced via Inspector flag '_forceClientInEditor'.");
                return true;
            }

            // 2. ParrelSync clone detection for seamless local multiplayer editing
            if (_checkParrelSync)
            {
#if UNITY_EDITOR
                if (ParrelSync.ClonesManager.IsClone())
                {
                    Debug.Log("[NetworkAutoJoin] ParrelSync clone detected.");
                    return true;
                }
#endif
            }

            // 3. Command line arguments check (useful for standalone automated client testing)
            if (_checkCommandLine)
            {
                string[] args = System.Environment.GetCommandLineArgs();
                foreach (string arg in args)
                {
                    string normalizedArg = arg.Trim().ToLower();
                    if (normalizedArg == "-client" || normalizedArg == "--client")
                    {
                        Debug.Log($"[NetworkAutoJoin] Command line client argument '{arg}' detected.");
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Assigns the room name to the active transport.
        /// </summary>
        private void SetRoomName(string name)
        {
            if (NetworkManager.main.transport is PurrTransport purrTransport)
            {
                purrTransport.roomName = name;
            }
            else
            {
                Debug.LogWarning("[NetworkAutoJoin] The active transport is not a PurrTransport and may not support Room Names/Codes.");
            }
        }
    }
}
