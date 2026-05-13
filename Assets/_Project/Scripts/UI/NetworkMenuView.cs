using PurrNet;
using PurrNet.Transports;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectOni.UI
{
    [DefaultExecutionOrder(10)]
    public class NetworkMenuView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private GameObject _menuPanel;

        private const string ROOM_CODE = "didjecte";

        private void Start()
        {
            if (NetworkManager.main == null)
            {
                Debug.LogWarning("[NetworkMenu] No NetworkManager found in scene!");
                return;
            }

            // Check if already connected
            if (NetworkManager.main.isServer || NetworkManager.main.isClient)
            {
                HideMenu();
                return;
            }

            ShowMenu();

            if (_hostButton != null) _hostButton.onClick.AddListener(OnHostClicked);
            if (_joinButton != null) _joinButton.onClick.AddListener(OnJoinClicked);
        }

        private void OnDestroy()
        {
            if (_hostButton != null) _hostButton.onClick.RemoveListener(OnHostClicked);
            if (_joinButton != null) _joinButton.onClick.RemoveListener(OnJoinClicked);
        }

        private void ShowMenu()
        {
            if (_menuPanel != null) _menuPanel.SetActive(true);
        }

        private void HideMenu()
        {
            if (_menuPanel != null) _menuPanel.SetActive(false);
        }

        private void OnHostClicked()
        {
            if (NetworkManager.main == null) return;

            if (NetworkManager.main.isServer || NetworkManager.main.isClient)
            {
                Debug.LogWarning("[NetworkMenu] Network is already running!");
                return;
            }

            SetRoomName(ROOM_CODE);
            NetworkManager.main.StartHost();
            HideMenu();
        }

        private void OnJoinClicked()
        {
            if (NetworkManager.main == null) return;

            if (NetworkManager.main.isServer || NetworkManager.main.isClient)
            {
                Debug.LogWarning("[NetworkMenu] Network is already running!");
                return;
            }

            SetRoomName(ROOM_CODE);
            NetworkManager.main.StartClient();
            HideMenu();
        }

        private void SetRoomName(string name)
        {
            if (NetworkManager.main.transport is PurrTransport purrTransport)
            {
                purrTransport.roomName = name;
            }
            else
            {
                Debug.LogWarning("[NetworkMenu] Current transport does not support Room Names/Codes.");
            }
        }
    }
}
