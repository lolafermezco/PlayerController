using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerJumping : MonoBehaviour
{
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float jumpPressBuffertime = 0.05f;
    [SerializeField] float jumpGroundGraceTime = 0.2f;

    //reference to player script
    Player player;

    bool tryingToJump;
    float lastJumpPressTime;
    float lastGroundedTime;

    void Awake() {
        player = GetComponent<Player>();
    }

    void OnEnable() {
        player.OnBeforeMove += OnBeforeMove;
        player.OnGroundStateChange += OnGroundStateChange;
    }

    void OnDisable() {
        player.OnBeforeMove -= OnBeforeMove;
        player.OnGroundStateChange -= OnGroundStateChange;
    }

    public void OnJump() {
        tryingToJump = true;
        lastJumpPressTime = Time.time;
    }

    void OnBeforeMove() {
        //testing to see if we're trying to jump or if were grounded
        bool wasTryingToJump = Time.time - lastJumpPressTime < jumpPressBuffertime; //bool is true is left is lesser than right
        bool wasGrounded = Time.time - lastGroundedTime < jumpGroundGraceTime; 

        bool isOrWasTryingToJump = tryingToJump || (wasTryingToJump && player.IsGrounded); //wasTryingToJump should only be valid if the player is grounded (otherwise you fly into space)
        bool isOrWasGrounded = player.IsGrounded || wasGrounded;

        //if we we are trying to jump or tried recently AND if we are grounded or grounded recently, then we can jump
        if (isOrWasTryingToJump && isOrWasGrounded) {
            player.velocity.y += jumpSpeed;
        }
        tryingToJump = false;
    }

    void OnGroundStateChange(bool isGrounded) {
        //checking if were gfrounded if were not then we assign the current time to the last grounded time
        if (!isGrounded) lastGroundedTime = Time.time;
    }
}
