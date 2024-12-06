using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public Text timerText;
    public float gameDuration = 60f;
    private float timeRemaining;
    public GameObject GameOverUI;

    private NetworkVariable<ulong> itPlayerId = new NetworkVariable<ulong>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Debug.Log("GameManager.Start() called");
        if (IsServer)
        {
            Debug.Log("GameManager running on server");
            timeRemaining = gameDuration;
            StartCoroutine(GameTimer());
            AssignRandomItPlayer();
        }
    }

    private void AssignRandomItPlayer()
    {
        Debug.Log("Assigning random player as 'It'");
        var players = NetworkManager.Singleton.ConnectedClientsList;
        if (players.Count > 0)
        {
            int randomIndex = Random.Range(0, players.Count);
            ulong randomPlayerId = players[randomIndex].ClientId;
            itPlayerId.Value = randomPlayerId;

            Debug.Log($"Player {randomPlayerId} is 'It'");

            var playerObject = NetworkManager.Singleton.ConnectedClients[randomPlayerId].PlayerObject;
            if (playerObject != null)
            {
                playerObject.GetComponent<PlayerCon>()?.TagAsItServerRpc();
            }
            else
            {
                Debug.LogError($"Player object for {randomPlayerId} not found");
            }
        }
        else
        {
            Debug.LogWarning("No players connected to assign as 'It'");
        }
    }

    private IEnumerator GameTimer()
    {
        Debug.Log("GameTimer started");
        while (timeRemaining > 0)
        {
            yield return new WaitForSeconds(1f);
            timeRemaining--;
            Debug.Log($"Time Remaining: {timeRemaining}");
            UpdateTimerUI();
        }
        Debug.Log("Time is up, ending game");
        EndGame();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = $"Time Left: {Mathf.CeilToInt(timeRemaining)}";
            Debug.Log($"Timer UI updated: {timerText.text}");
        }
        else
        {
            Debug.LogError("Timer Text UI is not assigned");
        }
    }

    private void EndGame()
    {
        Debug.Log("Game Over!");
        NotifyGameEndClientRpc();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = client.PlayerObject;
            if (playerObject != null)
            {
                playerObject.GetComponent<PlayerCon>()?.DisableMovement();
            }
        }
    }

    [ClientRpc]
    private void NotifyGameEndClientRpc()
    {
        Debug.Log("Game Over - Client Notified");
        if (GameOverUI != null)
        {
            GameOverUI.SetActive(true);
            Debug.Log("Game Over UI activated");
        }
        else
        {
            Debug.LogError("GameOverUI is not assigned");
        }
    }
}
