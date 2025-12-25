using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Setari Miscare")]
    public float sideSpeed = 10f;
    public float forwardSpeed = 10f;
    public float forwardAcceleration = 10f;
    public float raycastDistance = 1.1f;

    [Header("Setari Jetpack")]
    public float thrustForce = 25f;
    public float maxFlightTime = 2f;    // Timpul maxim de zbor
    public float maxJumpHeight = 1.2f;   // Inaltimea maxima fata de punctul de decolare
    public float maxFuel = 1f;           // Capacitate rezervor
    private float currentFuel;           // Combustibil actual
    private float jumpStartY;            // Pozitia Y de unde a inceput saritura
    private bool isJumping = false;      // Verifica daca suntem in aer

    // Referinte catre noul Input System
    private InputAction moveAction;
    private InputAction rotateLeftAction;
    private InputAction rotateRightAction;

    private Vector2 moveRead;
    private Rigidbody rb;
    public GameObject playerCamera;

    private bool isJumpHeld = false;     // Daca tasta de saritura (W/Space) este apasata
    private float speedMultiplier = 1f;  // Multiplicator pentru viteza (ex: power-ups)
    private bool isRotating = false;     // Previne suprapunerea rotatiilor

    void Awake()
    {
        // Initializam referintele catre actiunile de input din sistem
        moveAction = InputSystem.actions.FindAction("Move");
        rotateLeftAction = InputSystem.actions.FindAction("RotateLeft");
        rotateRightAction = InputSystem.actions.FindAction("RotateRight");

        rb = GetComponent<Rigidbody>();
    }

    // Metoda apelata cand obiectul este activat (prevenim MissingReferenceException)
    private void OnEnable()
    {
        if (rotateLeftAction != null) rotateLeftAction.performed += OnRotateLeft;
        if (rotateRightAction != null) rotateRightAction.performed += OnRotateRight;
    }

    // Metoda apelata cand obiectul este distrus sau dezactivat (curatam legaturile)
    private void OnDisable()
    {
        if (rotateLeftAction != null) rotateLeftAction.performed -= OnRotateLeft;
        if (rotateRightAction != null) rotateRightAction.performed -= OnRotateRight;
    }

    // Functii de legatura pentru evenimentele de input
    private void OnRotateLeft(InputAction.CallbackContext ctx) => TryRotate(-90f);
    private void OnRotateRight(InputAction.CallbackContext ctx) => TryRotate(90f);

    void Start()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        currentFuel = maxFlightTime;
    }

    void TryRotate(float angle)
    {
        // Pornim corutina de rotire doar daca nu ne rotim deja si obiectul exista
        if (this != null && !isRotating && gameObject.activeInHierarchy)
        {
            StartCoroutine(RotatePlayer(angle));
        }
    }

    void Update()
    {
        ReadInput();

        // Verificam daca tasta de "Inainte/Sari" este apasata (axa Y a stick-ului/WASD)
        isJumpHeld = moveRead.y > 0.5f;

        // Inregistram inaltimea de start cand incepem sa zburam de pe sol
        if (isJumpHeld && !isJumping && IsGrounded())
        {
            isJumping = true;
            jumpStartY = transform.position.y;
        }

        // Reincarcam combustibilul cand atingem solul si nu mai incercam sa sarim
        if (IsGrounded() && !isJumpHeld)
        {
            currentFuel = maxFlightTime;
            isJumping = false;
        }

        // Actualizam pozitia camerei sa urmareasca jucatorul (offset fix)
        if (playerCamera != null)
        {
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + 3f, transform.position.z - 5f);
        }
    }

    private void FixedUpdate()
    {
        // Logica Jetpack: daca apasam butonul, avem combustibil si suntem sub inaltimea maxima
        float currentHeightRelative = transform.position.y - jumpStartY;
        bool belowMaxHeight = currentHeightRelative < maxJumpHeight;
        bool hasFuel = currentFuel > 0f;

        if (isJumpHeld && hasFuel && belowMaxHeight)
        {
            // Aplicam forta in sus ignorand masa (Acceleration)
            rb.AddForce(Vector3.up * thrustForce, ForceMode.Acceleration);
            // Consumam combustibil in functie de timpul scurs
            currentFuel -= Time.fixedDeltaTime;
        }

        // Gestionarea vitezei de inaintare (Forward)
        Vector3 currentVelocity = rb.linearVelocity;
        float currentSpeedForward = Vector3.Dot(currentVelocity, transform.forward);

        if (currentSpeedForward < forwardSpeed * speedMultiplier)
        {
            rb.AddForce(transform.forward * forwardAcceleration * speedMultiplier, ForceMode.Acceleration);
        }

        // Gestionarea miscarii laterale (Left/Right)
        Vector3 lateralForce = transform.right * moveRead.x * sideSpeed;
        rb.AddForce(lateralForce, ForceMode.Force);
    }

    private void ReadInput()
    {
        // Citim valorile din noul Input System
        if (moveAction != null)
            moveRead = moveAction.ReadValue<Vector2>();
    }

    private bool IsGrounded()
    {
        // Tragem o raza invizibila in jos pentru a vedea daca suntem pe sol
        // Ignoram layer-ul Player pentru a nu ne detecta pe noi insine
        return Physics.Raycast(transform.position, Vector3.down, raycastDistance, ~LayerMask.GetMask("Player"));
    }

    public void SetSpeedMultiplier(float multiplier) => speedMultiplier = multiplier;

    // Corutina pentru rotire lina la 90 de grade
    IEnumerator RotatePlayer(float angle)
    {
        isRotating = true;

        Quaternion currentRotation = rb.rotation;
        Quaternion finalRotation = currentRotation * Quaternion.Euler(0f, angle, 0f);

        float timeElapsed = 0f;
        float duration = 0.1f; // Durata rotatiei in secunde

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            // SmoothStep face rotitia sa inceapa lina si sa se termine lina
            float t_eased = Mathf.SmoothStep(0f, 1f, t);

            // Folosim Slerp pentru interpolare intre rotatii
            rb.MoveRotation(Quaternion.Slerp(currentRotation, finalRotation, t_eased));

            timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Ne asiguram ca rotitia este exacta la final
        rb.MoveRotation(finalRotation);
        isRotating = false;
    }
}