using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using NaughtyAttributes;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Stargaze.Mono.Networking
{
    public enum LobbyDataKeys
    {
        HostAddress,
        LobbyName,
        LobbyValidationCheck
    }
    
    [RequireComponent(typeof(StargazeNetworkManager))]
    public class SteamLobby : MonoBehaviour
    {
        public static SteamLobby Instance;
        
        private StargazeNetworkManager _networkManager;
        
        public SteamId CurrentLobbyID { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            _networkManager = GetComponent<StargazeNetworkManager>();

            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        }

        public async void HostLobby(string lobbyName, bool isPublic)
        {
            Lobby? lobbyRequest = await SteamMatchmaking.CreateLobbyAsync(_networkManager.maxConnections);
            
            if (lobbyRequest.HasValue)
            {
                if (isPublic)
                    lobbyRequest.Value.SetPublic();
                else
                    lobbyRequest.Value.SetFriendsOnly();
                
                lobbyRequest.Value.SetData(LobbyDataKeys.LobbyName.ToString(), lobbyName);
                    
                lobbyRequest.Value.SetJoinable(true);
            }
        }

        [Button]
        public async void HostFriendOnlyLobby()
        {
            Lobby? lobbyRequest = await SteamMatchmaking.CreateLobbyAsync(_networkManager.maxConnections);

            if (lobbyRequest.HasValue)
            {
                lobbyRequest.Value.SetFriendsOnly();
                lobbyRequest.Value.SetJoinable(true);
                lobbyRequest.Value.SetData(LobbyDataKeys.LobbyName.ToString(), $"{SteamClient.Name}'s Lobby");
            }
        }
        
        [Button]
        public async void HostPublicLobby()
        {
            Lobby? lobbyRequest = await SteamMatchmaking.CreateLobbyAsync(_networkManager.maxConnections);

            if (lobbyRequest.HasValue)
            {
                lobbyRequest.Value.SetPublic();
                lobbyRequest.Value.SetJoinable(true);
                lobbyRequest.Value.SetData(LobbyDataKeys.LobbyName.ToString(), $"{SteamClient.Name}'s Lobby");
            }
        }

        public async Task<Lobby[]> GetLobbyList()
        {
            //SteamMatchmaking.LobbyList.WithKeyValue(LobbyDataKeys.LobbyValidationCheck.ToString(), "Stargaze");
            
            return await SteamMatchmaking.LobbyList.RequestAsync();
        }
        
        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            if (result != Result.OK)
            {
                Debug.Log($"Lobby creation failed with error: {result}");
                return;
            }
            
            Debug.Log("Lobby created with success!");

            CurrentLobbyID = lobby.Id;

            lobby.SetData(LobbyDataKeys.HostAddress.ToString(), SteamClient.SteamId.ToString());
            lobby.SetData(LobbyDataKeys.LobbyValidationCheck.ToString(), "Stargaze");

            _networkManager.StartHost();
        }

        private void OnGameLobbyJoinRequested(Lobby lobby, SteamId id)
        {
            SteamMatchmaking.JoinLobbyAsync(lobby.Id);
        }

        private void OnLobbyEntered(Lobby lobby)
        {
            if (NetworkServer.active)
                return;

            CurrentLobbyID = lobby.Id;

            string hostAddress = lobby.GetData(LobbyDataKeys.HostAddress.ToString());

            _networkManager.networkAddress = hostAddress;
            _networkManager.StartClient();
        }
    }
}