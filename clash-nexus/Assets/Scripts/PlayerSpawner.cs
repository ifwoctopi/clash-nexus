using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns the selected player characters in the game scene based on selections from CharacterSelect scene.
/// Place this in the game scene (SampleScene) and assign the spawn points and character prefabs.
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [System.Serializable]
    public class CharacterPrefabEntry
    {
        public string characterId;
        public GameObject characterPrefab;
    }

    [Header("Spawn Points")]
    [Tooltip("Transform where Player 1 should spawn")]
    [SerializeField] private Transform player1SpawnPoint;
    
    [Tooltip("Transform where Player 2 (CPU) should spawn")]
    [SerializeField] private Transform player2SpawnPoint;

    [Header("Character Prefabs")]
    [Tooltip("Map of character IDs to their prefabs")]
    [SerializeField] private List<CharacterPrefabEntry> characterPrefabs = new List<CharacterPrefabEntry>();

    [Header("CPU Character")]
    [Tooltip("Character ID for the CPU opponent (if player 2 is CPU)")]
    [SerializeField] private string cpuCharacterId = "Knight1";
    
    [Tooltip("Map of character IDs to their CPU prefabs (for Player 2)")]
    [SerializeField] private List<CharacterPrefabEntry> cpuCharacterPrefabs = new List<CharacterPrefabEntry>();

    private Dictionary<string, GameObject> characterPrefabDict;
    private GameObject spawnedPlayer1;
    private GameObject spawnedPlayer2;

    void Awake()
    {
        // Build dictionary for quick lookup
        characterPrefabDict = new Dictionary<string, GameObject>();
        foreach (var entry in characterPrefabs)
        {
            if (entry.characterPrefab != null && !string.IsNullOrEmpty(entry.characterId))
            {
                characterPrefabDict[entry.characterId] = entry.characterPrefab;
            }
        }
        
        // Add CPU prefabs to dictionary (they override regular prefabs if same ID)
        foreach (var entry in cpuCharacterPrefabs)
        {
            if (entry.characterPrefab != null && !string.IsNullOrEmpty(entry.characterId))
            {
                characterPrefabDict[entry.characterId] = entry.characterPrefab;
            }
        }

        // Find spawn points if not assigned
        if (player1SpawnPoint == null)
        {
            GameObject spawnPoint = GameObject.Find("Player1");
            if (spawnPoint != null)
            {
                player1SpawnPoint = spawnPoint.transform;
            }
        }

        if (player2SpawnPoint == null)
        {
            GameObject spawnPoint = GameObject.Find("Player2");
            if (spawnPoint != null)
            {
                player2SpawnPoint = spawnPoint.transform;
            }
        }
    }

    void Start()
    {
        SpawnPlayers();
    }

    /// <summary>
    /// Spawns the players based on their selected characters
    /// </summary>
    public void SpawnPlayers()
    {
        // Get selected characters from GameDataManager
        GameDataManager dataManager = GameDataManager.Instance;
        string player1CharId = dataManager.GetPlayerCharacter(1);
        string player2CharId = dataManager.GetPlayerCharacter(2);

        // If no selection, use defaults (for testing)
        if (string.IsNullOrEmpty(player1CharId))
        {
            Debug.LogWarning("PlayerSpawner: No character selected for Player 1, using first available character");
            if (characterPrefabs.Count > 0)
            {
                player1CharId = characterPrefabs[0].characterId;
            }
        }

        // For Player 2, if not selected, use CPU character
        if (string.IsNullOrEmpty(player2CharId))
        {
            player2CharId = cpuCharacterId;
        }

        // Spawn Player 1
        if (player1SpawnPoint != null && !string.IsNullOrEmpty(player1CharId))
        {
            SpawnPlayer(1, player1CharId, player1SpawnPoint);
        }

        // Spawn Player 2 - check game mode to determine if CPU or human player
        if (player2SpawnPoint != null && !string.IsNullOrEmpty(player2CharId))
        {
            bool isTwoPlayerMode = dataManager.IsTwoPlayerMode();
            
            Debug.Log($"PlayerSpawner: Game mode is {(isTwoPlayerMode ? "2 Player" : "1 Player vs CPU")}");
            
            if (isTwoPlayerMode == true)
            {
                // 2-player mode: spawn regular player prefab (not CPU)
                Debug.Log($"PlayerSpawner: Spawning regular player prefab for Player 2: {player2CharId}");
                SpawnPlayer(2, player2CharId, player2SpawnPoint, false);
            }
            else
            {
                // 1-player mode: spawn CPU prefab
                string cpuCharId = player2CharId + "CPU";
                if (!characterPrefabDict.ContainsKey(cpuCharId))
                {
                    // Fall back to regular character if no CPU version exists
                    cpuCharId = player2CharId;
                }
                Debug.Log($"PlayerSpawner: Spawning CPU prefab for Player 2: {cpuCharId}");
                SpawnPlayer(2, cpuCharId, player2SpawnPoint, true);
                
            }
        }
    }

    /// <summary>
    /// Spawns a character for a specific player
    /// </summary>
    private void SpawnPlayer(int playerNumber, string characterId, Transform spawnPoint, bool isCPU = false)
    {
        if (!characterPrefabDict.TryGetValue(characterId, out GameObject prefab))
        {
            Debug.LogError($"PlayerSpawner: Character prefab not found for ID: {characterId}");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError($"PlayerSpawner: Spawn point is null for Player {playerNumber}");
            return;
        }

        // Instantiate the character
        GameObject spawnedCharacter = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        
        // Rename to match player number
        spawnedCharacter.name = $"Player{playerNumber}_{characterId}";

        // Flip Player 2 to face the opposite direction
        if (playerNumber == 2)
        {
            SpriteRenderer spriteRenderer = spawnedCharacter.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = true;
            }
        }

        // Store reference
        if (playerNumber == 1)
        {
            spawnedPlayer1 = spawnedCharacter;
        }
        else if (playerNumber == 2)
        {
            spawnedPlayer2 = spawnedCharacter;
            
            // Assign Player1's transform to CPUController if it's CPU mode
            if (isCPU)
            {
                CPUController cpuController = spawnedCharacter.GetComponent<CPUController>();
                if (cpuController != null && spawnedPlayer1 != null)
                {
                    cpuController.player = spawnedPlayer1.transform;
                    Debug.Log("CPUController: Target set to Player 1");
                }
                else if (cpuController == null && spawnedPlayer1 != null)
                {
                    ProjectileCPUController projectileCPUController = spawnedCharacter.GetComponent<ProjectileCPUController>();
                    projectileCPUController.player = spawnedPlayer1.transform;
                    Debug.Log("CPUController: Target set to Player 1");
                }
            }
            
            // If it's 2-player mode (not CPU), add Player2ControlsSwapper to use arrow keys
            if (!isCPU && GameDataManager.Instance.IsTwoPlayerMode())
            {
                spawnedCharacter.AddComponent<Player2ControlsSwapper>();
                Debug.Log($"PlayerSpawner: Added Player2ControlsSwapper to Player 2 for arrow key controls");
            }
        }

        Debug.Log($"PlayerSpawner: Spawned {characterId} for Player {playerNumber} at {spawnPoint.position}");
    }

    /// <summary>
    /// Gets the spawned player GameObject
    /// </summary>
    public GameObject GetSpawnedPlayer(int playerNumber)
    {
        if (playerNumber == 1)
        {
            return spawnedPlayer1;
        }
        else if (playerNumber == 2)
        {
            return spawnedPlayer2;
        }
        return null;
    }
}

