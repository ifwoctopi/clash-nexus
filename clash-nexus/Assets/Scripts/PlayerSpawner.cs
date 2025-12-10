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
    [SerializeField] public ProjectileScript projectileScript;

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
            bool isPracticeMode = dataManager.IsPracticeMode();
            bool isTwoPlayerMode = dataManager.IsTwoPlayerMode();
            
            if (isPracticeMode)
            {
                // Practice mode: spawn regular character (not CPU) but disable all AI/controls
                Debug.Log($"PlayerSpawner: Practice mode - spawning Player 2 as dummy: {player2CharId}");
                SpawnPlayer(2, player2CharId, player2SpawnPoint, false, true);
            }
            else if (isTwoPlayerMode == true)
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
        
        // Configure enemyLayers for 2-player mode after both players are spawned
        if (dataManager.IsTwoPlayerMode())
        {
            ConfigureEnemyLayersForTwoPlayerMode();
        }
    }

    /// <summary>
    /// Spawns a character for a specific player
    /// </summary>
    private void SpawnPlayer(int playerNumber, string characterId, Transform spawnPoint, bool isCPU = false, bool isPracticeDummy = false)
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
        PlayerIdentity identity = spawnedCharacter.AddComponent<PlayerIdentity>();
        identity.playerNumber = playerNumber;

        // Rename to match player number
        spawnedCharacter.name = $"Player{playerNumber}_{characterId}";
        
        // Set the layer for the player
        // Layer 3 = Player1, Layer 8 = CPU/Player2
        if (playerNumber == 1)
        {
            spawnedCharacter.layer = 3; // Player1 layer
        }
        else if (playerNumber == 2)
        {
            spawnedCharacter.layer = 8; // CPU/Player2 layer
        }
        
        // Also set layer for all children (like attack points, etc.)
        SetLayerRecursively(spawnedCharacter, spawnedCharacter.layer);

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
            
            // Practice mode: disable all controllers/AI on Player 2
            if (isPracticeDummy)
            {
                DisableAllControllers(spawnedCharacter);
                Debug.Log("PlayerSpawner: Disabled all controllers on Player 2 (practice dummy)");
            }
            // Assign Player1's transform to CPUController if it's CPU mode
            else if (isCPU)
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
                    projectileCPUController.playerTransform = spawnedPlayer1.transform;
                    projectileCPUController.player = spawnedPlayer1;
                    Debug.Log("CPUController: Target set to Player 1");
                }
            }

            if (spawnedPlayer1 != null && spawnedPlayer1.GetComponent<HuntressController>() != null)
            {
                HuntressController huntressController = spawnedPlayer1.GetComponent<HuntressController>();
                huntressController.enemy = spawnedPlayer2;
            }
            
            if (spawnedPlayer2 != null && spawnedPlayer2.GetComponent<HuntressController>() != null)
            {
                HuntressController huntressController = spawnedPlayer2.GetComponent<HuntressController>();
                huntressController.enemy = spawnedPlayer1;
            }
            
            // If it's 2-player mode (not CPU, not practice), add Player2ControlsSwapper to use arrow keys
            if (!isCPU && !isPracticeDummy && GameDataManager.Instance.IsTwoPlayerMode())
            {
                spawnedCharacter.AddComponent<Player2ControlsSwapper>();
                Debug.Log($"PlayerSpawner: Added Player2ControlsSwapper to Player 2 for arrow key controls");
            }
            
            PlayerIdentity id = spawnedCharacter.AddComponent<PlayerIdentity>();
            id.playerNumber = playerNumber; // 1 or 2

        }



        Debug.Log($"PlayerSpawner: Spawned {characterId} for Player {playerNumber} at {spawnPoint.position}");
    }
    
    /// <summary>
    /// Sets the layer recursively for a GameObject and all its children
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    
    /// <summary>
    /// Configures enemyLayers for 2-player mode so both players can damage each other
    /// </summary>
    private void ConfigureEnemyLayersForTwoPlayerMode()
    {
        bool isTwoPlayerMode = GameDataManager.Instance.IsTwoPlayerMode();
        if (!isTwoPlayerMode) return;
        
        // Layer 3 = Player1, Layer 8 = CPU/Player2
        int player1Layer = 3;
        int player2Layer = 8;
        LayerMask player1LayerMask = 1 << player1Layer;  // Only Player1 layer (value = 8)
        LayerMask player2LayerMask = 1 << player2Layer;   // Only Player2 layer (value = 256)
        
        // In 2-player mode, both players should be able to damage each other
        // Player1's enemyLayers should ONLY include Player2 layer (layer 8)
        // Player2's enemyLayers should ONLY include Player1 layer (layer 3)
        
        if (spawnedPlayer1 != null)
        {
            Debug.Log($"PlayerSpawner: Configuring Player1 enemyLayers to target Player2 (layer {player2Layer}, mask value {player2LayerMask.value})");
            ConfigureEnemyLayers(spawnedPlayer1, player2LayerMask);
        }
        
        if (spawnedPlayer2 != null)
        {
            Debug.Log($"PlayerSpawner: Configuring Player2 enemyLayers to target Player1 (layer {player1Layer}, mask value {player1LayerMask.value})");
            ConfigureEnemyLayers(spawnedPlayer2, player1LayerMask);
        }
        
        Debug.Log("PlayerSpawner: Configured enemyLayers for 2-player mode");
    }
    
    /// <summary>
    /// Configures enemyLayers on a character using reflection to find all controllers with enemyLayers or playerLayer field
    /// </summary>
    private void ConfigureEnemyLayers(GameObject character, LayerMask enemyLayerMask)
    {
        // Get all MonoBehaviour components
        MonoBehaviour[] components = character.GetComponents<MonoBehaviour>();
        
        bool foundAny = false;
        foreach (MonoBehaviour component in components)
        {
            if (component == null) continue;
            
            // Try to find enemyLayers field first
            System.Reflection.FieldInfo enemyLayersField = component.GetType().GetField("enemyLayers", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            // If not found, try playerLayer (used by some controllers like CPUController)
            if (enemyLayersField == null)
            {
                enemyLayersField = component.GetType().GetField("playerLayer", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            }
            
            if (enemyLayersField != null && enemyLayersField.FieldType == typeof(LayerMask))
            {
                LayerMask oldValue = (LayerMask)enemyLayersField.GetValue(component);
                enemyLayersField.SetValue(component, enemyLayerMask);
                foundAny = true;
                string fieldName = enemyLayersField.Name;
                Debug.Log($"PlayerSpawner: Set {fieldName} on {component.GetType().Name} from {oldValue.value} to {enemyLayerMask.value}");
            }
        }
        
        if (!foundAny)
        {
            Debug.LogWarning($"PlayerSpawner: No enemyLayers or playerLayer field found on {character.name}. Make sure controllers have a public LayerMask enemyLayers or playerLayer field.");
        }
    }

    /// <summary>
    /// Disables all controller components on a character (for practice dummy)
    /// </summary>
    private void DisableAllControllers(GameObject character)
    {
        MonoBehaviour[] components = character.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            if (component == null) continue;
            
            string typeName = component.GetType().Name;
            // Disable all controller components except PlayerHealth
            if (typeName.Contains("Controller") && !typeName.Contains("Health"))
            {
                component.enabled = false;
                Debug.Log($"PlayerSpawner: Disabled {typeName} on {character.name}");
            }
        }
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

