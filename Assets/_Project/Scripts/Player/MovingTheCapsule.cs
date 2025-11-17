using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class MovingTheCapsule : MonoBehaviour
{

    // [SerializeField] face variabila vizibila în Inspectorul Unity
    // Se poate schimba viteza direct din Unity, fara sa mai intri in cod
    [SerializeField] private float moveSpeed = 8;
    [SerializeField] private float jumpForce = 5;

    // parametri de care am nevoie pentru sarituri
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;

    // Aici vom stoca o referinta catre componenta Rigidbody
    private Rigidbody rb;

    void Start()
    {
        // Caut componenta Rigidbody atasata de ACELASI obiect si o salvez in variabila 'rb'
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {

        // daca tasta 'A' sau sageata stanga au fost apasate se apeleaza functia rotire player care roteste jucatorul cu 90 de grade spre stanga
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StartCoroutine(RotatePlayer(-90f));
        }
        // daca tasta 'D' sau sageata dreapta au fost apasate se apeleaza functia rotire player care roteste jucatorul cu 90 de grade spre dreapta
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(RotatePlayer(90f));
        }
        // daca tasta 'W' sau sageata sus au fost apasate si daca jucatorul este pe sol se apeleaza functia Jump
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && IsGrounded())
        {
            Jump();
        }
    }

    void Jump()
    {
        // Vector3.up * jumpForce este un vector care indica directia si forta sariturii
        // ForceMode.Impulse pentru a avea o forta brusca de saritura

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    bool IsGrounded()
    {
        // Physics.CheckSphere creeaza o sfera la pozitia groundCheck.position
        // Daca aceasta sfera se suprapune cu obiectul cu eticheta groundLayer, returneaza true
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
    }

    // FixedUpdate() este chemata la intervale fixe de timp
    // Este cea mai buna functie pentru a lucra cu fizica (Rigidbody)
    void FixedUpdate()
    {
        // Definim cum vrem sa fie viteza
        // Vector3.forward inseamna (0, 0, 1) - adica "inainte" pe axa Z
        // Inmultim cu viteza noastra (moveSpeed)
        Vector3 forwardMove = transform.forward * moveSpeed * Time.fixedDeltaTime;

        // Ca sa nu avem probleme cu gravitatia, aplicam miscarea doar pe axele X si Z, dar pastram viteza pe Y
        rb.MovePosition(rb.position + forwardMove);
    }

    IEnumerator RotatePlayer(float angle)
    {
        // Creez o noua rotatie (Quaternion) prin adaugarea sau scaderea a 90 de grade pe planul orizontal (axa Y)
        Quaternion turnRotation = Quaternion.Euler(0f, angle, 0f);

        // Iau rotatia curenta a Rigidbody-ului
        Quaternion currentRotation = rb.rotation;

        // Inmultesc rotatia curenta cu noua rotatie de intoarcere(in cazul quaternion-ilor asta inseamna adunarea rotatiilor).
        Quaternion finalRotation = currentRotation * turnRotation;

        float timeElapsed = 0f;

        // ca sa nu se faca rotatia instant, folosim un coroutine pentru a face rotatia treptat in timp
        while (timeElapsed < 0.1f)
        {
            // Calculeaza progresul de timp
            float t = timeElapsed / 0.1f;

            // Mathf.SmoothStep returneaza o valoare intre 0 si 1 care este atenuata la capete (ajuta la a avea o rotatie mai lina)
            float t_eased = Mathf.SmoothStep(0f, 1f, t);

            // Mutam rotatia de la start la end pe baza progresului atenuat (t_eased)
            Quaternion nextRotation = Quaternion.Slerp(currentRotation, finalRotation, t_eased);


            rb.MoveRotation(nextRotation);

            timeElapsed += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }
    }
}