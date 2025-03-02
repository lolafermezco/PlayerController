using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerSprinting : MonoBehaviour
{
    [SerializeField] float speedMultiplier = 2f;

    //references to the
    Player player; //player script
    PlayerInput playerInput; //player input componant
    InputAction sprintAction; //sprint action

    void Awake() {
        player = GetComponent<Player>();
        playerInput = GetComponent<PlayerInput>();
        sprintAction = playerInput.actions["sprint"];
    }

    void OnEnable() => player.OnBeforeMove += OnBeforeMove;
    void OnDisable() => player.OnBeforeMove -= OnBeforeMove;

    void OnBeforeMove() {
        //reading sprint action value
        var sprintInput = sprintAction.ReadValue<float>();
        //we dont want to the the next calculations if the player isnt sprinting so we check for that now
        if (sprintInput == 0) return;
        //preventing the player from sprinting sideways or backwards
        var forwardMovementFactor = Mathf.Clamp01(
            //calculating the dot productr of the players forward vector with the players velocity (forward = 1, Left/right = 0, backwards = -1). we clamp it to the 0-1 range to only get positive values
            Vector3.Dot(player.transform.forward, player.velocity.normalized)
            );
        var multiplier = Mathf.Lerp(1f, speedMultiplier, forwardMovementFactor);
        player.movementSpeedMultiplier *= multiplier; //if > 0 then multiply the players movementSpeedMultiplier by our speed multiplier, otherwise we justm multiply it by 1.
    }
}
