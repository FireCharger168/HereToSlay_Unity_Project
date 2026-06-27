using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

namespace HereToSlay.UI
{
    /// <summary>
    /// Handles the pre-game lobby screen: hosting, joining, and the ready button.
    /// Wire up in Inspector. Place on a Canvas that gets hidden when the game starts.
    /// </summary>
    public class LobbyUI : MonoBehaviour
    {
        [Header("Lobby Panels")]
        [SerializeField] GameObject lobbyPanel;
        [SerializeField] GameObject gamePanel;

        [Header("Input")]
        [SerializeField] TMP_InputField playerNameInput;
        [SerializeField] TMP_InputField ipAddressInput;

        [Header("Buttons")]
        [SerializeField] Button hostButton;
        [SerializeField] Button joinButton;
        [SerializeField] Button readyButton;
        [SerializeField] Button disconnectButton;

        [Header("Status")]
        [SerializeField] TextMeshProUGUI statusLabel;
        [SerializeField] Transform       playerListArea;
        [SerializeField] TextMeshProUGUI playerEntryPrefab;

        NetworkManager _netManager;

        void Awake()
        {
            _netManager = NetworkManager.singleton;

            hostButton.onClick.AddListener(Host);
            joinButton.onClick.AddListener(Join);
            readyButton.onClick.AddListener(Ready);
            disconnectButton.onClick.AddListener(Disconnect);

            // Mirror events
            NetworkManagerHook.OnClientConnected    += OnConnected;
            NetworkManagerHook.OnClientDisconnected += OnDisconnected;
            HereToSlay.Core.GameManager.OnStateChanged += _ => RefreshPlayerList();
        }

        void OnDestroy()
        {
            NetworkManagerHook.OnClientConnected    -= OnConnected;
            NetworkManagerHook.OnClientDisconnected -= OnDisconnected;
        }

        // ── Actions ───────────────────────────────────────────────────────────

        void Host()
        {
            SaveName();
            _netManager.StartHost();
            SetStatus("Hosting on port 7777 — share your IP with friends.");
            ShowLobby();
        }

        void Join()
        {
            SaveName();
            string ip = ipAddressInput.text.Trim();
            if (string.IsNullOrEmpty(ip)) ip = "localhost";
            _netManager.networkAddress = ip;
            _netManager.StartClient();
            SetStatus($"Connecting to {ip}…");
        }

        void Ready()
        {
            Networking.NetworkPlayerController.LocalInstance?.CmdReady();
            readyButton.interactable = false;
            SetStatus("Waiting for others to ready up…");
        }

        void Disconnect()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
                _netManager.StopHost();
            else
                _netManager.StopClient();

            lobbyPanel.SetActive(true);
            gamePanel.SetActive(false);
            SetStatus("Disconnected.");
        }

        void SaveName()
        {
            string name = playerNameInput.text.Trim();
            if (string.IsNullOrEmpty(name)) name = "Player";
            PlayerPrefs.SetString("PlayerName", name);
        }

        void ShowLobby()
        {
            lobbyPanel.SetActive(true);
            gamePanel.SetActive(false);
            hostButton.interactable = false;
            joinButton.interactable = false;
            readyButton.interactable = true;
        }

        void OnConnected()
        {
            SetStatus("Connected! Click Ready when all players have joined.");
            ShowLobby();
        }

        void OnDisconnected() => SetStatus("Connection lost.");

        void RefreshPlayerList()
        {
            // Rebuild player list (placeholder — replace with state.players list)
            SetStatus($"Lobby — waiting for players to ready.");
        }

        void SetStatus(string msg)
        {
            if (statusLabel) statusLabel.text = msg;
        }
    }

    /// <summary>
    /// Hooks into Mirror's NetworkManager events.
    /// Subclass of NetworkManager — set this as your NetworkManager type in Inspector.
    /// </summary>
    public class HereToSlayNetworkManager : NetworkManager
    {
        public override void OnClientConnect()
        {
            base.OnClientConnect();
            NetworkManagerHook.ClientConnected();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            NetworkManagerHook.ClientDisconnected();
        }
    }

    public static class NetworkManagerHook
    {
        public static event System.Action OnClientConnected;
        public static event System.Action OnClientDisconnected;
        public static void ClientConnected()    => OnClientConnected?.Invoke();
        public static void ClientDisconnected() => OnClientDisconnected?.Invoke();
    }
}
