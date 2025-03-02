using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsManager : MonoBehaviour
{
    // Singleton instance
    public static PlayerStatsManager Instance;

    //refernce to PlayerStatsGroup so the UI can be changed
    public PlayerStatsGroup playerStatsGroup;

    //stats
    private float playerInsulation;
    private float playerHealth;
    private float playerHydration;
    private float playerHunger;
    private float playerAmbientTemperature;
    private float playerAmbientTemperatureMin;
    private float playerAmbientTemperatureMax;
    private float playerInternalTemperature;
    private float playerInternalAmbientTemperatureMin;
    private float playerInternalAmbientTemperatureMax;
    private float playerInternalTemperatureMin;
    private float playerInternalTemperatureMax;
    private float playerMovementStamina;

    //default values
    private float defaultPlayerInsulation = 0f;
    private float defaultPlayerHealth = 100f;
    private float defaultPlayerHydration = 100f;
    private float defaultPlayerHunger = 100f;
    private float defaultPlayerAmbientTemperature = 20f; // Ambient Temp
    private float defaultPlayerAmbientTemperatureMin = -30f;
    private float defaultPlayerAmbientTemperatureMax = 60f;

    private float defaultPlayerInternalTemperature = 36f; //Internal Temp
    private float defaultPlayerInternalAmbientTemperatureMin = -10f;
    private float defaultPlayerInternalAmbientTemperatureMax = 40f;
    private float defaultPlayerInternalTemperatureMin = 30f;
    private float defaultPlayerInternalTemperatureMax = 42f;

    private float defaultPlayerMovementStamina = 100f;

    // timeElapsed values
    private float timeElapsedInternalTemperature = 0f;
    private float timeElapsedAmbientTemperature = 0f;

    private void Awake() {
        // Check if an instance already exists
        if (Instance == null) {
            // If not, set the instance to this object
            Instance = this;
            DontDestroyOnLoad(gameObject); // Prevent this object from being destroyed when loading a new scene
        }
        else {
            // If an instance already exists, destroy this object
            Destroy(gameObject);
        }

        // Load the saved stats when the game starts
        LoadAllStats();
        ReloadAllStatsUI();
    }

    public void Update() {
        // Ambient Temperature
        // Damage from ambient temperature extreems being exceeded
        if (playerAmbientTemperature < playerAmbientTemperatureMin) {
            Debug.Log("death from ambient freezing");
            // Increment the timer by the time passed since last frame
            timeElapsedAmbientTemperature += Time.deltaTime;
            Debug.Log("timeElapsed = " + timeElapsedAmbientTemperature);
            if (timeElapsedAmbientTemperature >= 0.5f) {
                Debug.Log("freeze burning death happening");
                ChangeHealth(-1f); // Decrease by 1
                timeElapsedAmbientTemperature = 0f;
            }
        }
        else if (playerAmbientTemperatureMax < playerAmbientTemperature) {
            Debug.Log("death from ambient heat");
            // Increment the timer by the time passed since last frame
            timeElapsedAmbientTemperature += Time.deltaTime;
            Debug.Log("timeElapsed = " + timeElapsedAmbientTemperature);
            if (timeElapsedAmbientTemperature >= 0.5f) {
                Debug.Log("burning death happening");
                ChangeHealth(-1f); // Decrease by 1
                timeElapsedAmbientTemperature = 0f;
            }
        }

        // Internal temperature
        if (playerAmbientTemperature < defaultPlayerInternalAmbientTemperatureMin) {
            //Debug.Log("ambientTemp: " + playerAmbientTemperature + " < min: " + defaultPlayerInternalAmbientTemperatureMin);
            SetInternalTemperature(InternalTemperatureCalculator.CalculateInternalTemperature(playerInternalTemperature, false)); //decrease internal temp
        }
        else if (defaultPlayerInternalAmbientTemperatureMax < playerAmbientTemperature) {
            //Debug.Log("max: " + defaultPlayerInternalAmbientTemperatureMax + " < ambientTemp: " + playerAmbientTemperature);
            SetInternalTemperature(InternalTemperatureCalculator.CalculateInternalTemperature(playerInternalTemperature, true)); //increase internal temp
        }

        // Damage from internal temperature extreems being exceeded
        if (playerInternalTemperature < playerInternalTemperatureMin) {
            Debug.Log("death from internal freezing");
            // Increment the timer by the time passed since last frame
            timeElapsedInternalTemperature += Time.deltaTime;
            Debug.Log("timeElapsed = " + timeElapsedInternalTemperature);
            if (timeElapsedInternalTemperature >= 0.5f) {
                Debug.Log("freezing death happening");
                ChangeHealth(-1f); // Decrease by 1
                timeElapsedInternalTemperature = 0f;
            }
        }
        else if (playerInternalTemperatureMax < playerInternalTemperature) {
            Debug.Log("death from heat stroke");
            // Increment the timer by the time passed since last frame
            timeElapsedInternalTemperature += Time.deltaTime;
            Debug.Log("timeElapsed = " + timeElapsedInternalTemperature);
            if (timeElapsedInternalTemperature >= 0.5f) {
                Debug.Log("heat death happening");
                ChangeHealth(-1f); // Decrease by 1
                timeElapsedInternalTemperature = 0f;
            }
        }
    }

    // Method to save stats (this can be expanded to save to a file)
    public void SaveStats() {
        // You can use PlayerPrefs or another system for persistent storage
        PlayerPrefs.SetFloat("PlayerInsulation", playerInsulation);
        PlayerPrefs.SetFloat("PlayerHealth", playerHealth);
        PlayerPrefs.SetFloat("PlayeyHydration", playerHydration);
        PlayerPrefs.SetFloat("PlayerHunger", playerHunger);
        PlayerPrefs.SetFloat("PlayerAmbientTemperature", playerAmbientTemperature);
        PlayerPrefs.SetFloat("PlayerAmbientTemperatureMin", playerAmbientTemperatureMin);
        PlayerPrefs.SetFloat("PlayerAmbientTemperatureMax", playerAmbientTemperatureMax);
        PlayerPrefs.SetFloat("PlayerInternalTemperature", playerInternalTemperature);
        PlayerPrefs.SetFloat("PlayerInternalAmbientTemperatureMin", playerInternalAmbientTemperatureMin);
        PlayerPrefs.SetFloat("PlayerInternalAmbientTemperatureMax", playerInternalAmbientTemperatureMax);
        PlayerPrefs.SetFloat("PlayerInternalTemperatureMin", playerInternalTemperatureMin);
        PlayerPrefs.SetFloat("PlayerInternalTemperatureMax", playerInternalTemperatureMax);
        PlayerPrefs.SetFloat("PlayerMovementStamina", playerMovementStamina);
        PlayerPrefs.Save();
    }

    // Method to load stats
    public void LoadAllStats() {
        // Load the saved stats from PlayerPrefs, if they exist
        // Default values are provided in case there's no saved data

        //PlayerPrefs.GetFloat("PlayerHealth", 100f) will get the value for PlayerHealth and if there is no value saved then it will default to 100
        playerInsulation = PlayerPrefs.GetFloat("PlayerInsulation", defaultPlayerInsulation);
        playerHealth = PlayerPrefs.GetFloat("PlayerHealth", defaultPlayerHealth);
        playerHydration = PlayerPrefs.GetFloat("PlayeyHydration", defaultPlayerHydration);
        playerHunger = PlayerPrefs.GetFloat("PlayerHunger", defaultPlayerHunger);
        playerAmbientTemperature = PlayerPrefs.GetFloat("PlayerAmbientTemperature", defaultPlayerAmbientTemperature); // Ambient Temperature
        playerAmbientTemperatureMin = PlayerPrefs.GetFloat("PlayerAmbientTemperatureMin", defaultPlayerAmbientTemperatureMin);
        playerAmbientTemperatureMax = PlayerPrefs.GetFloat("PlayerAmbientTemperatureMax", defaultPlayerAmbientTemperatureMax);
        playerInternalTemperature = PlayerPrefs.GetFloat("PlayerInternalTemperature", defaultPlayerInternalTemperature); // InternalTemperature
        playerInternalAmbientTemperatureMin = PlayerPrefs.GetFloat("PlayerInternalAmbientTemperatureMin", defaultPlayerInternalAmbientTemperatureMin);
        playerInternalAmbientTemperatureMax = PlayerPrefs.GetFloat("PlayerInternalAmbientTemperatureMax", defaultPlayerInternalAmbientTemperatureMax);
        playerInternalTemperatureMin = PlayerPrefs.GetFloat("PlayerInternalTemperatureMin", defaultPlayerInternalTemperatureMin);
        playerInternalTemperatureMax = PlayerPrefs.GetFloat("PlayerInternalTemperatureMax", defaultPlayerInternalTemperatureMax);
        playerMovementStamina = PlayerPrefs.GetFloat("PlayerMovementStamina", defaultPlayerMovementStamina);
    }

    public void ReloadAllStatsUI() {
        // Actually reloading the stats from the file
        LoadAllStats();

        // Re-displaying them on the UI
        playerStatsGroup.ChangeInsulationUI(playerInsulation);
        playerStatsGroup.ChangeHealthUI(playerHealth);
        playerStatsGroup.ChangeHydrationUI(playerHydration);
        playerStatsGroup.ChangeHungerUI(playerHunger);
        playerStatsGroup.ChangeAmbientTemperatureUI(playerAmbientTemperature);
        playerStatsGroup.ChangeAmbientTemperatureMinUI(playerAmbientTemperatureMin);
        playerStatsGroup.ChangeAmbientTemperatureMaxUI(playerAmbientTemperatureMax);
        playerStatsGroup.ChangeInternalTemperatureUI(playerInternalTemperature);
        playerStatsGroup.ChangeInternalTemperatureMinUI(playerInternalTemperatureMin);
        playerStatsGroup.ChangeInternalTemperatureMaxUI(playerInternalTemperatureMax);
        playerStatsGroup.ChangeMovementStaminaUI(playerMovementStamina);
    }

    //methods for Insulation
    public void ChangeInsulation(float value) {
        playerInsulation += playerInsulation <= 0 && value <=0 ? 0 : value; //wont reduce insulation below 0
        playerStatsGroup.ChangeInsulationUI(playerInsulation);
    }

    public void SetInsulation(float value) {
        playerInsulation = value < 0 ? playerInsulation : value; //wont set the insulation below 0
        playerStatsGroup.ChangeInsulationUI(playerInsulation);
    }

    //methods for Health
    public void ChangeHealth(float value) {
        playerHealth += playerHealth<=0 && value <= 0? 0 : value; //wont reduce health below 0
        playerStatsGroup.ChangeHealthUI(playerHealth);
    }

    public void SetHealth(float value) {
        playerHealth = value < 0 ? playerHealth : value; //wont set the health below 0
        playerStatsGroup.ChangeHealthUI(playerHealth);
    }

    //methods for Hydration
    public void ChangeHydration(float value) {
        playerHydration += playerHydration <= 0 && value <= 0 ? 0 : value; //wont reduce hydration below 0
        playerStatsGroup.ChangeHydrationUI(playerHydration);
    }

    public void SetHydration(float value) {
        playerHydration = value < 0 ? playerHydration : value; //wont set the hydration below 0
        playerStatsGroup.ChangeHydrationUI(playerHydration);
    }

    //methods for Hunger
    public void ChangeHunger(float value) {
        playerHunger += playerHunger <= 0 && value <= 0 ? 0 : value; //wont reduce hunger below 0
        playerStatsGroup.ChangeHungerUI(playerHunger);
    }

    public void SetHunger(float value) {
        playerHunger = value < 0 ? playerHunger : value; //wont set the hunger below 0
        playerStatsGroup.ChangeHungerUI(playerHunger);
    }

    //methods for changing playerAmbientTemperature
    public void NewtonianChangeAmbientTemperature(float volume, float objectTemp, float distance, bool hot) {
        playerAmbientTemperature = NewtonianTemperatureCalculator.CalculateNewtonianTemperature(volume, objectTemp, distance, playerAmbientTemperature, playerInsulation, hot);
        playerStatsGroup.ChangeAmbientTemperatureUI(playerAmbientTemperature);
    }

    public void ChangeAmbientTemperature(float value) {
        playerAmbientTemperature += value;
        playerStatsGroup.ChangeAmbientTemperatureUI(playerAmbientTemperature);
    }

    public void SetAmbientTemperature(float value) {
        playerAmbientTemperature = value < -293.15 ? playerAmbientTemperature : value; //wont set the temperature below absolute 0
        playerStatsGroup.ChangeAmbientTemperatureUI(playerAmbientTemperature);
    }

    //methods for changing playerInternalTemperature
    public void ChangeInternalTemperature(float value) {
        playerInternalTemperature += value;
        playerStatsGroup.ChangeInternalTemperatureUI(playerInternalTemperature);
    }

    public void SetInternalTemperature(float value) {
        playerInternalTemperature = value < -293.15 ? playerInternalTemperature : value; //wont set the temperature below absolute 0
        playerStatsGroup.ChangeInternalTemperatureUI(playerInternalTemperature);
    }


    //methods for changing stamina
    public void ChangeMovementStamina(float value) {
        playerMovementStamina += playerMovementStamina <= 0 && value <= 0 ? 0 : value; //wont reduce movement stamina below 0
        playerStatsGroup.ChangeMovementStaminaUI(playerMovementStamina);
    }

    public void SetMovementStamina(float value) {
        playerMovementStamina = value < 0 ? playerMovementStamina : value; //wont set the movement stamina below 0
        playerStatsGroup.ChangeMovementStaminaUI(playerMovementStamina);
    }
}
