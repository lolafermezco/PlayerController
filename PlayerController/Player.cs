using Codice.Client.BaseCommands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {

    [SerializeField] float mouseSensitivity = 3f;
    [SerializeField] float walkingSpeed = 5f;
    [SerializeField] float flyingSpeed = 10f;
    [SerializeField] float climbingSpeed = 2f;
    [SerializeField] float mass = 1f;
    [SerializeField] float acceleration = 20f;

    [SerializeField] float worldBottomBoundary = -30f;

    public Transform cameraTransform;

    //stuff for pausing

    public GameObject pauseMenuUI;
    public bool isPaused;

    public bool IsGrounded => controller.isGrounded;

    public float Height {
        get => controller.height;
        set => controller.height = value;
    }

    public event Action OnBeforeMove;
    public event Action<bool> OnGroundStateChange; //takes one bool argument which is our current ground state

    internal float movementSpeedMultiplier;

    State _state;
    //new property called CurrentState, when re change currentstate the velocity is reset to 0
    public State CurrentState {
        get => _state;
        set {
            _state = value;
            velocity = Vector3.zero;
        }
    }

    public enum State {
        Walking,
        Flying,
        Climbing
    }

    CharacterController controller;
    internal Vector3 velocity;
    Vector2 look;

    (Vector3, Quaternion) initialPositionAndRotation; //new tuple variable to hold our starting position and rotation

    bool wasGrounded;

    //getting references to the PlayerInput componant
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction lookAction;
    InputAction flyUpDownAction;

    void Awake() { //it is good practice to put all of the GetComponant calls in the Awake method because all of the Awake methods are guaranteed to be called before the all of the Start methods
        //reference to Character Controller with a GetComponant call
        controller = GetComponent<CharacterController>();
        //references to playerInput actions
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["move"];
        lookAction = playerInput.actions["look"];
        flyUpDownAction = playerInput.actions["flyUpDown"];
        //setting the player not to be paused when he first enters the game
        isPaused = false;
    }

    // Start is called before the first frame update
    void Start() {
        //locking cursor so you cant see it when youre looking
        Cursor.lockState = CursorLockMode.Locked;
        //setting our initial position and rotation
        initialPositionAndRotation = (transform.position, transform.rotation);
    }

    //take in the desired position and sets it to the players position, and takes the rotation as a quaternion and and it takes euler angles from that and sets those to our look vector, and veloocity is set to 0
    public void Teleport(Vector3 position, Quaternion rotation) {
        transform.position = position;
        Physics.SyncTransforms(); //a call to make sure that the change in player position worked
        look.x = rotation.eulerAngles.x;
        look.y = rotation.eulerAngles.z;
        velocity = Vector3.zero;
    }

    public void PlayerPausing() {
        Debug.Log("PlayerPausing method player");
        if (isPaused) {
            isPaused = false;
        }
        else {
            isPaused = true;
        }
    }


    // Update is called once per frame
    private void Update() {
        /*if (Input.GetKeyDown(KeyCode.Escape) && pauseMenuUI != null) {
            Debug.Log("Escape player");
            PlayerPausing();
        }*/

        if (!isPaused) {
            movementSpeedMultiplier = 1f;
            switch (CurrentState) {
                case State.Walking:
                    //walking
                    UpdateGround();
                    UpdateGravity();
                    UpdateMovement();
                    UpdateLook();
                    CheckBounds();
                    break;
                case State.Flying:
                    //flying
                    UpdateMovementFlying();
                    UpdateLook();
                    break;
                case State.Climbing:
                    //climbing
                    UpdateMovementClimbing();
                    UpdateLook();
                    break;
            }
        }
    }

    void CheckBounds() {
        //if out position is less than the world bottom boundary we will teleport to our initial position
        if (transform.position.y < worldBottomBoundary) {
            var (position, rotation) = initialPositionAndRotation;
            Teleport(position, rotation);
        }
    }

    void UpdateSlopeSliding() {
        //if we are grounded
        if (IsGrounded) {
            //we are going to cast a sphere downwards. to calculate the center of that sphere we are dividing the controller heigth by 2 ans sutracting the controller radius
            var sphereCastVerticalOffset = controller.height / 2 - controller.radius;
            //and then to get the actual point in the world coordinates we are subtracting it from our players position
            var castOrigin = transform.position - new Vector3(0, sphereCastVerticalOffset, 0);

            //sphere cast
            if (Physics.SphereCast(castOrigin //point that were casing from
                , controller.radius - .01f //radius of the sphere we are casting from. we use a value slightly smaller that the player capsule sothat it doesnt hit any walls
                , Vector3.down //direction 
                , out var hit //variable that is going to hold the hit result of this cast if ithits anything 
                , .05f //distance in meters, so we are casting 5cm down
                , ~LayerMask.GetMask("Player") //layer mask. the ~ means bitwise not. so basically what it means is that we are intersted in anything that is not on the player layer.
                                               //if we dont do this than the first hit will obviously be the player since the sphere is being cast from inside tha player capsule
                , QueryTriggerInteraction.Ignore)) //this last argument means we are not interested in triggers, since the ground will alwayse be a solid collider not a trigger
                {
                //if this cast hits anything we are going to take theangle betwen the up vector and the normal vector and pout it into a variable
                var collider = hit.collider;
                var angle = Vector3.Angle(Vector3.up, hit.normal);

                if (angle > controller.slopeLimit) {
                    var normal = hit.normal;
                    var yInverse = 1f - normal.y;
                    velocity.x += yInverse * normal.x;
                    velocity.z = yInverse * normal.z;
                }
            }
        }
    }

    void UpdateGround() {
        UpdateSlopeSliding();

        if (wasGrounded != IsGrounded) {
            OnGroundStateChange?.Invoke(IsGrounded);
            wasGrounded = IsGrounded;
        }
    }

    void UpdateGravity() {
        var gravity = Physics.gravity * mass * Time.deltaTime;
        //vertical velocity
        velocity.y = controller.isGrounded ? -1f : velocity.y + gravity.y; //if we are grounded we will set it to a small negative value so we keep ground contact. otherwise we will gradually accelerate with the gravity.y value
    }

    Vector3 GetMovementInput(float speed, bool horizontal = true) {
        //horizontal and vertical input from the keyboard
        //var x = Input.GetAxis("Horizontal");
        //var y = Input.GetAxis("Vertical");
        var moveInput = moveAction.ReadValue<Vector2>();
        var flyUpDownInput = flyUpDownAction.ReadValue<float>();

        //creating an input vector by adding the vector pointing forawrds from the player multiplied by y (which is the w and s keys)
        //and adding a vector that is pointing to the right multiplied by x (which is our a and d keys)
        var input = new Vector3();
        var referenceTranform = horizontal ? transform : cameraTransform; //if we are horizontal then we use the player treansform to move, but if we are not horizontal we will use the camera stansform
        input += referenceTranform.forward * moveInput.y;
        input += referenceTranform.right * moveInput.x;
        if (!horizontal) {
            input += transform.up * flyUpDownInput;
        }
        input = Vector3.ClampMagnitude(input, 1f); //clamping resultant vector to 1 sotaht we dont move faster diagonally
        input *= speed * movementSpeedMultiplier;

        return input;
    }

    void UpdateMovement() {
        OnBeforeMove?.Invoke();

        var input = GetMovementInput(walkingSpeed);

        var factor = acceleration * Time.deltaTime;
        velocity.x = Mathf.Lerp(velocity.x, input.x, factor);
        velocity.z = Mathf.Lerp(velocity.z, input.z, factor);

        //actually moving the player with a call to transform.Translate
        //transform.Translate(input * movementSpeed * Time.deltaTime, Space.World); //multiplied by Time.deltaTime sothat the movement speed is not framerate dependant
        controller.Move(velocity * Time.deltaTime);
    }

    //not calling OnBeforeMove in this method because thats what we were using for croucuing and jumping which we dont need when were flying
    void UpdateMovementFlying() {
        var input = GetMovementInput(flyingSpeed, false); //passing false to the GMI method to get non-horizontal movement

        var factor = acceleration * Time.deltaTime;
        velocity = Vector3.Lerp(velocity, input, factor); //interpolating velocity as a vector instead of just interpolating x and z componants

        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateMovementClimbing() {
        var input = GetMovementInput(climbingSpeed, false); //passing false to the GMI method to get non-horizontal movement
        //checking which direction we're trying to move bycalculating the dot poduct of transform.forward and input.normalized
        var forwardInputFactor = Vector3.Dot(transform.forward, input.normalized);

        //if its greater than 0 then it means we are trying to move forward
        if (forwardInputFactor > 0) {

            //decreasing horizontal input
            input.x = input.x * .5f;
            input.z = input.z * .5f;

            //checking if the vertical input is greater than some small value in any direction
            if (Mathf.Abs(input.y) > .2f) {
                //and if it is we are going to gert its sign andmultiply it by climbing speed
                input.y = Mathf.Sign(input.y) * climbingSpeed;
            }
        }
        //otherwise we are going to reset the vertical input completely and amplify ther horizontal input
        else {
            input.y = 0;
            input.x = input.x * 3f;
            input.z = input.z * 3f;
        }

        var factor = acceleration * Time.deltaTime;
        velocity = Vector3.Lerp(velocity, input, factor); //interpolating velocity as a vector instead of just interpolating x and z componants

        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateLook() {
        var lookInput = lookAction.ReadValue<Vector2>();
        look.x += lookInput.x * mouseSensitivity;
        look.y += lookInput.y * mouseSensitivity;

        //locking the look.y values sothat you can't look past staright up or staright down
        look.y = Mathf.Clamp(look.y, -89f, 89f);

        cameraTransform.localRotation = Quaternion.Euler(-look.y, 0, 0);
        transform.localRotation = Quaternion.Euler(0, look.x, 0);
    }

    void OnToggleFlying() {
        CurrentState = CurrentState == State.Flying ? State.Walking : State.Flying; //if the current state is flying then we switch to walking otherwise we switch to flying
    }
}
