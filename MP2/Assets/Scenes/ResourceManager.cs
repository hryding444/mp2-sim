
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class ResourceManager : MonoBehaviour
{
    [Header("Generators")]
    public float num_mineralgenerators = 0;
    public float num_energygenerators = 0;

    [Header("UI Display")]
    public TextMeshProUGUI resourceText;
    public TextMeshProUGUI tutorialText;

    [Header("Tutorial Settings")]
    public float tutorialDisplayTime = 20f; // How long it stays on screen
    private Coroutine currentTutorialCoroutine; // Tracks the active timer

    [Header("Current Resources")]
    public float currentMinerals = 0f;
    public float currentEnergy = 0f;
    public float currentOxygen = 600f; 

    [Header("Cooldown Settings")]
    public float actionCooldownDuration = 10f; // 10 seconds per action
    public float mineralCooldown = 0f;
    public float energyCooldown = 0f;
    public float oxygenCooldown = 0f;
    

    [Header("Resource Rates (Per Second)")]
    public float mineralRate = 1f; 
    public float energyRate = 0f;  
    public float oxygenDepletionRate = 1f; 

    [Header("Exponential Cost Settings")]
    public float costMultiplier = 1.2f; 

    [Header("1. Mineral Generator Settings")]
    public GameObject mineralGeneratorPrefab; 
    public float mineralGeneratorCost = 10f;
    public float mineralBoostRate = 1f;
    public bool hasUnlockedMineral = false;

    [Header("2. Energy Generator Settings")]
    public GameObject energyGeneratorPrefab; 
    public float energyGeneratorCost = 15f;
    public float energyBoostRate = 1f;
    public bool hasUnlockedEnergy = false;

    [Header("3. Oxygen Refill Settings")]
    public float oxygenRefillCost = 30f; 
    public float oxygenRefillAmount = 100f; 
    public float maxOxygen = 600f; 
    public bool hasUnlockedOxygen = false;

    private void Start()
    {
        
    }
    void Update()
    {
        // 1. Ramping Resources & Depleting Oxygen
        currentMinerals += num_mineralgenerators * (mineralRate * Time.deltaTime);
        currentEnergy += num_energygenerators * (energyRate * Time.deltaTime);
        currentOxygen -= oxygenDepletionRate * Time.deltaTime;

        if (currentOxygen < 0f) currentOxygen = 0f;

        // 2. Cooldown Timers (Counting down)
        if (mineralCooldown > 0) mineralCooldown -= Time.deltaTime;
        if (energyCooldown > 0) energyCooldown -= Time.deltaTime;
        if (oxygenCooldown > 0) oxygenCooldown -= Time.deltaTime;

        // 2. Tutorial & Unlock Logic
        CheckForUnlocks();

        // 3. Update the UI Text
        UpdateUIText();

        // 4. Keyboard Testing
        HandleKeyboardInputs();
    }

    // --- LOGIC FUNCTIONS ---

    void CheckForUnlocks()
    {
        // Unlock Mineral Generator logic (first tutorial)
        if (currentMinerals >=  mineralGeneratorCost&& !hasUnlockedMineral)
        {
            hasUnlockedMineral = true;
            ShowTutorial("TIP: Reach " + mineralGeneratorCost + " Minerals to deploy your first Mineral Generator!");
        }

        // Unlock Energy Generator 
        if (currentMinerals >= energyGeneratorCost && !hasUnlockedEnergy)
        {
            hasUnlockedEnergy = true;
            ShowTutorial("NEW UNLOCK: Deploy the Energy Generator for " + energyGeneratorCost + " Minerals now");
        }

        // Unlock Oxygen Refill function
        if (currentMinerals >= oxygenRefillCost && !hasUnlockedOxygen)
        {
            hasUnlockedOxygen = true;
            ShowTutorial("NEW UNLOCK: Refill " + oxygenRefillAmount + " of Oxygen for" + oxygenRefillCost + " Minerals now");
        }

        // Unlock Oxygen Refill (e.g., when Oxygen drops below 300)
        if (currentOxygen <= 300)
        {
            ShowTutorial("DANGER: Oxygen Low!");
        }
    }

    void ShowTutorial(string message)
    {
        if (tutorialText != null)
        {
            tutorialText.text = message;

            // If there's already a timer running from a previous message, stop it!
            if (currentTutorialCoroutine != null)
            {
                StopCoroutine(currentTutorialCoroutine);
            }

            // Start a fresh 5-second countdown
            currentTutorialCoroutine = StartCoroutine(ClearTutorialAfterDelay());
        }
    }

    void UpdateUIText()
    {
        if (resourceText != null)
        {
            // Formatting the cooldowns to show 1 decimal place (e.g., 9.5s) or "Ready"
            string minStatus = mineralCooldown > 0 ? $"Wait: {mineralCooldown:F1}s" : "Ready";
            string engStatus = energyCooldown > 0 ? $"Wait: {energyCooldown:F1}s" : "Ready";
            string oxyStatus = oxygenCooldown > 0 ? $"Wait: {oxygenCooldown:F1}s" : "Ready";

            resourceText.text = 
                $"Minerals: {Mathf.FloorToInt(currentMinerals)} (rate: {mineralRate}/s) | Min Gen: [{minStatus}]\n" +
                $"Energy: {Mathf.FloorToInt(currentEnergy)} (rate: {energyRate}/s) | Eng Gen: [{engStatus}]\n" +
                $"Oxygen: {Mathf.FloorToInt(currentOxygen)} (rate: -{oxygenDepletionRate}/s) | Oxy Refill: [{oxyStatus}]";
        }
    }

    void HandleKeyboardInputs()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) TryDeployMineralGenerator();
            if (Keyboard.current.digit2Key.wasPressedThisFrame) TryDeployEnergyGenerator();
            if (Keyboard.current.digit3Key.wasPressedThisFrame) TryRefillOxygen();
        }
    }

    // --- DEPLOYMENT FUNCTIONS ---

    public void TryDeployMineralGenerator()
    {
        if (mineralCooldown > 0)
        {
            Debug.Log("Mineral Generator is on cooldown!");
            return; // Stops the function immediately
        }
        if (currentMinerals >= mineralGeneratorCost)
        {
            currentMinerals -= mineralGeneratorCost;
            mineralRate += mineralBoostRate;
            mineralGeneratorCost *= costMultiplier;
            SpawnInFront(mineralGeneratorPrefab);

            mineralCooldown = actionCooldownDuration;
            Debug.Log("Mineral Generator bought!");
        }
    }

    public void TryDeployEnergyGenerator()
    {
        if (energyCooldown > 0)
        {
            Debug.Log("Energy Generator is on cooldown!");
            return; 
        }
        if (currentMinerals >= energyGeneratorCost)
        {
            currentMinerals -= energyGeneratorCost;
            energyRate += energyBoostRate;
            energyGeneratorCost *= costMultiplier;
            SpawnInFront(energyGeneratorPrefab);

            energyCooldown = actionCooldownDuration;
            Debug.Log("Energy Generator deployed!");
        }
    }

    public void TryRefillOxygen()
    {
        if (oxygenCooldown > 0)
        {
            Debug.Log("Oxygen Refill is on cooldown!");
            return; 
        }
        if (currentMinerals >= oxygenRefillCost)
        {
            currentMinerals -= oxygenRefillCost;
            currentOxygen = Mathf.Min(currentOxygen + oxygenRefillAmount, maxOxygen);

            oxygenCooldown = actionCooldownDuration;
            Debug.Log("Oxygen refilled!");
        }
    }

    void SpawnInFront(GameObject prefab)
    {
        if (prefab != null)
        {
            Transform playerHead = Camera.main.transform;
            Vector3 spawnPosition = playerHead.position + (playerHead.forward * 1.5f);
            spawnPosition.y -= 0.5f;
            Instantiate(prefab, spawnPosition, playerHead.rotation);
        }
    }

    private IEnumerator ClearTutorialAfterDelay()
    {
        // 1. Wait for the specified time
        yield return new WaitForSeconds(tutorialDisplayTime);
        
        // 2. Erase the text
        if (tutorialText != null)
        {
            tutorialText.text = ""; 
        }
    }
}