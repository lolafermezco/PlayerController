using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettingsManager : MonoBehaviour
{
    // Singleton instance
    public static PlayerSettingsManager Instance;

    // Settings with default values for when the game is first run. 

    //controls
    public bool flyingMinecraft;
    public bool crouchHold;
    public bool proneHold;
    public bool dynamicMovementEnabled;

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

        // Load the saved settings when the game starts
        LoadSettings();
    }

    // Method to save settings (this can be expanded to save to a file)
    public void SaveSettings() {
        // You can use PlayerPrefs or another system for persistent storage
        PlayerPrefs.SetInt("FlyingMinecraft", flyingMinecraft ? 1 : 0);
        PlayerPrefs.SetInt("CrouchHold", crouchHold ? 1 : 0);
        PlayerPrefs.SetInt("ProneHold", proneHold ? 1 : 0);
        PlayerPrefs.SetInt("DynamicMovementEnabled", dynamicMovementEnabled ? 1: 0);
        PlayerPrefs.Save();
    }

    // Method to load settings
    public void LoadSettings() {
        // Load the saved settings from PlayerPrefs, if they exist
        // Default values are provided in case there's no saved data

        //PlayerPrefs.GetInt("FlyingMinecraft", 0) will get the value for FlyingMinecraft and if there is no value saved then it will default to 1,
        //then it compares whatever result it got to 1, and if they are equal the boolean is true and if they are not equal it is false
        flyingMinecraft = PlayerPrefs.GetInt("FlyingMinecraft", 1) == 1;
        crouchHold = PlayerPrefs.GetInt("CrouchHold", 1) == 1;
        proneHold = PlayerPrefs.GetInt("ProneHold", 0) == 1;
        dynamicMovementEnabled = PlayerPrefs.GetInt("DynamicMovementEnabled", 1) == 1;
    }
}
