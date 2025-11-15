using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class MovingTheCapsule : MonoBehaviour
{
    // [SerializeField] face variabila vizibila în Inspectorul Unity
    // Se poate schimba viteza direct din Unity, fara sa mai intri in cod
    [SerializeField] private float moveSpeed = 8f;

    // Aici vom stoca o referinta catre componenta Rigidbody
    private Rigidbody rb;

    // Functia Start() este chemata o sg data, la inceputul jocului
    void Start()
    {
        // Caut componenta Rigidbody atasata de ACELASI obiect si o salvez in variabila 'rb'
        rb = GetComponent<Rigidbody>();
    }

    // FixedUpdate() este chemata la intervale fixe de timp
    // Este cea mai buna functie pentru a lucra cu fizica (Rigidbody)
    void FixedUpdate()
    {
        // Definim cum vrem sa fie viteza
        // Vector3.forward inseamna (0, 0, 1) - adica "inainte" pe axa Z
        // Inmultim cu viteza noastra (moveSpeed)
        Vector3 forwardMove = transform.forward * moveSpeed * Time.fixedDeltaTime;

        // Ca sa nu avem probleme cu gravitatia, aplicam miscarea doar pe axele X si Z, dar pastram viteza pe Y (caderea/saritura)
        rb.MovePosition(rb.position + forwardMove);
    }
}