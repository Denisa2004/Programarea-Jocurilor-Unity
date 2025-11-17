using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Variabile pentru viteza si forta
    public float sideSpeed = 10f;
    public float forwardSpeed = 10f;
    public float forwardAcceleration = 10f;
    public float jumpForce = 500f;
    public float raycastDistance = 1.1f;
    private InputAction moveAction;
    private Vector2 moveRead;
    private Rigidbody rb;
    public GameObject playerCamera;


    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        // Obtine componenta Rigidbody de pe acelasi GameObject si o stocheaza in rb
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        ReadInput();

        // Daca tasta W este apasata si daca jucatorul este pe pamant se apeleaza functia Jump().
        if (moveRead.y > 0.5f && IsGrounded())
        {
            Jump();
        }

        // Seteaza pozitia camerei relativ la jucator
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + 3f, transform.position.z - 5f);
    }

    private void FixedUpdate()
    {
        // obtinem vectorul vitezei curente a Rigidbody-ului
        Vector3 currentVelocity = rb.linearVelocity;
        // Calculeaza viteza jucatorului in directia sa inainte
        float currentSpeedZ = Vector3.Dot(currentVelocity, transform.forward);

        // Daca viteza curenta inainte este mai mica decat viteza maxima dorita se aplica accelerarea pentru atunci cand incepe jocul si player ul sta pe loc..
        if (currentSpeedZ < forwardSpeed)
        {
            rb.AddForce(transform.forward * forwardAcceleration, ForceMode.Acceleration);
        }

        // pentru miscarea laterala
        Vector3 lateralForce = transform.right * moveRead.x * sideSpeed;
        rb.AddForce(lateralForce, ForceMode.Force);
    }

    private void ReadInput()
    {
        // se verifica daca exista un input
        if (moveAction != null)
        {
            // Citeste input ul (X pentru A/D, Y pentru W/S) si il stocheaza in moveRead
            moveRead = moveAction.ReadValue<Vector2>();
        }
        else
        {
            moveRead = Vector2.zero;
        }
    }

    private void Jump()
    {
        // Aplica o forta jumpForce instant pe axa Y
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private bool IsGrounded()
    {
        // Trage un Raycast in jos de la pozitia jucatorului.
        // Returneaza 'true' daca raza loveste un collider in interiorul razei 'raycastDistance'.
        // '~LayerMask.GetMask("Player")' se asigura ca Raycast-ul ignora propriul Layer al jucatorului.
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













