using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerCrouching : MonoBehaviour
{
    [SerializeField] float crouchHeight = 1f;
    [SerializeField] float crouchTransitionSpeed = 10f;
    [SerializeField] float crouchSpeedMultiplier = 0.5f;

    Player player;
    PlayerInput playerInput;
    InputAction crouchAction;

    Vector3 initialCameraPosition;
    float currentHeight;
    float standingHeight;

    bool IsCrouching => standingHeight - currentHeight > 0.1f;

    void Awake() {
        player = GetComponent<Player>();
        playerInput = GetComponent<PlayerInput>();
        crouchAction = playerInput.actions["crouch"];
    }

    void Start() {
        initialCameraPosition = player.cameraTransform.localPosition;
        standingHeight = currentHeight = player.Height;
    }

    void OnEnable() => player.OnBeforeMove += OnBeforeMove;
    void OnDisable() => player.OnBeforeMove -= OnBeforeMove;

    //checking if the player is trying to crouch
    void OnBeforeMove() {
        //by reading the crouch action value
        var isTryingToChrouch = crouchAction.ReadValue<float>() > 0;

        //if the player is trying to crouch we set the height target to crouch height, otherwise it is set to standing height
        var heightTarget = isTryingToChrouch ? crouchHeight : standingHeight;

        //if we are crouching but are not pressing the crouch key then that means we are trying to stand up
        if (IsCrouching && !isTryingToChrouch) {
            //in that case we are going to cast a ray 20cm up from the top of thtr player capsule and if it hits something were going to calculate the distance to the cailing
            //and were going to set the heightTarget to the current height + the distance to the ceiling - some small margin
            var castOrigin = transform.position + new Vector3(0, currentHeight / 2, 0);
            if (Physics.Raycast(castOrigin, Vector3.up, out RaycastHit hit, standingHeight)) {
                var distanceToCeiling = hit.point.y - castOrigin.y;
                heightTarget = Mathf.Min(standingHeight, (Mathf.Max(
                    currentHeight + distanceToCeiling - 0.1f, crouchHeight
                )));
            }
        }

        //if the heightTarget and Current height are approximately the same then we arent going to do any interpolating or calculations
        if (!Mathf.Approximately( heightTarget, currentHeight ) ) {
            var crouchDelta = Time.deltaTime * crouchTransitionSpeed;
            //slowly interpolating currentHeight to heightTarget by crouchDelta
            currentHeight = Mathf.Lerp(currentHeight, heightTarget, crouchDelta);

            var halfHeightDifference = new Vector3(0, (standingHeight - currentHeight) / 2, 0); //calculating half of the height diference between the players standing and target height
            var newCameraPosition = initialCameraPosition - halfHeightDifference;

            player.cameraTransform.localPosition = newCameraPosition;
            player.Height = heightTarget;
        }

        //if we're crouching reduce speed
        if (IsCrouching) {
            player.movementSpeedMultiplier *= crouchSpeedMultiplier;
        }
    }
}
