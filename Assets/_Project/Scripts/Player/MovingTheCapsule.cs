using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Variabile de viteză și forță
    public float sideSpeed = 10f;       // Viteza pentru stanga/dreapta (AddForce)
    public float forwardSpeed = 10f;    // Viteză MAXIMĂ constantă AUTOMATĂ dorită
    public float forwardAcceleration = 10f; // Forța de accelerare necesară
    public float jumpForce = 50f;      // Forța aplicată pentru săritură (ForceMode.Impulse)
    public float raycastDistance = 0.4f; // Distanța pentru verificarea solului
    
    // Referințe și Acțiuni
    private InputAction moveAction;
    private Vector2 moveRead;
    private Rigidbody rigidbody;
    public GameObject playerCamera;
    

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        ReadInput();

        // LOGICA SĂRITURII: W este folosit DOAR pentru săritură.
        if (moveRead.y > 0.5f && IsGrounded())
        {
            TryJump();
        }
        
        // Poziționarea camerei
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + 3f, transform.position.z - 5f);
    }

    private void FixedUpdate()
    {
        // Păstrează viteza pe Y (gravitația și săritura)
        Vector3 currentVelocity = rigidbody.linearVelocity;
        float currentSpeedZ = Vector3.Dot(currentVelocity, transform.forward); // Viteza curentă înainte

        // 1. 🚶 AVANSARE AUTOMATĂ și Menținere Viteză
        
        // Dacă viteza curentă înainte este mai mică decât viteza dorită, aplică forță.
        if (currentSpeedZ < forwardSpeed)
        {
            // Forța este proporțională cu forwardAcceleration
            rigidbody.AddForce(transform.forward * forwardAcceleration, ForceMode.Acceleration);
        }
        
        // Opțional: Dacă viteza este prea mare (peste forwardSpeed), aplică o frânare ușoară
        else if (currentSpeedZ > forwardSpeed)
        {
             // Forță de frânare aplicată opus direcției de mișcare
             rigidbody.AddForce(-transform.forward * forwardAcceleration * 0.5f, ForceMode.Acceleration);
        }


        // 2. ↔️ MIȘCARE LATERALĂ (A și D)
        // Folosește AddForce pentru controlul lateral (ForceMode.Force pentru precizie).
        Vector3 lateralForce = transform.right * moveRead.x * sideSpeed;
        rigidbody.AddForce(lateralForce, ForceMode.Force);
    }

    private void ReadInput()
    {
        if (moveAction != null)
        {
            moveRead = moveAction.ReadValue<Vector2>();
        }
        else
        {
            moveRead = Vector2.zero;
        }
    }

    private void TryJump()
    {
        // Aplică o forță instantanee pe axa Y
        rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private bool IsGrounded()
    {
        // Trage un Raycast în jos
        return Physics.Raycast(transform.position, Vector3.down, raycastDistance, ~LayerMask.GetMask("Player"));
    }
}

    //IEnumerator RotatePlayer(float angle)
    //{
    //    // Creez o noua rotatie (Quaternion) prin adaugarea sau scaderea a 90 de grade pe planul orizontal (axa Y)
    //    Quaternion turnRotation = Quaternion.Euler(0f, angle, 0f);

    //    // Iau rotatia curenta a Rigidbody-ului
    //    Quaternion currentRotation = rb.rotation;

    //    // Inmultesc rotatia curenta cu noua rotatie de intoarcere(in cazul quaternion-ilor asta inseamna adunarea rotatiilor).
    //    Quaternion finalRotation = currentRotation * turnRotation;

    //    float timeElapsed = 0f;

    //    // ca sa nu se faca rotatia instant, folosim un coroutine pentru a face rotatia treptat in timp
    //    while (timeElapsed < 0.1f)
    //    {
    //        // Calculeaza progresul de timp
    //        float t = timeElapsed / 0.1f;

    //        // Mathf.SmoothStep returneaza o valoare intre 0 si 1 care este atenuata la capete (ajuta la a avea o rotatie mai lina)
    //        float t_eased = Mathf.SmoothStep(0f, 1f, t);

    //        // Mutam rotatia de la start la end pe baza progresului atenuat (t_eased)
    //        Quaternion nextRotation = Quaternion.Slerp(currentRotation, finalRotation, t_eased);


    //        rb.MoveRotation(nextRotation);

    //        timeElapsed += Time.fixedDeltaTime;

    //        yield return new WaitForFixedUpdate();
    //    }
    //}
