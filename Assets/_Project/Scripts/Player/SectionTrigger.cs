using UnityEngine;

//WIP & TO DO: generarea se face doar pentru straightSegment, dea adaugat sa poata genera right/left

// Un segment i.e. StraightSegment are NewSectionTrigger care are un Box Collider care se afla la mijlocul segmentului

// Scopul acestui Script e ca atunci cand Collider-ul jucatorului interactioneaza cu collider-ul platformei, generam segment nou

public class SectionTrigger : MonoBehaviour
{
    // se adauga Prefabul de StraightSegment in client
    public GameObject roadSegment;

    public void OnTriggerEnter(Collider other)
    {
        // verificam tagul trigger, care e tag-ul segmentului
        if (other.gameObject.CompareTag("Trigger"))
        {
            // luam pozitia segmentului (parintele trigger-ului, NU al player-ului!)
            Vector3 currentPosition = other.transform.parent.position;

            // adaugam +30 la Z
            Vector3 newPosition = currentPosition + new Vector3(0, 0, 30);

            // instantiem nou segment
            Instantiate(roadSegment, newPosition, Quaternion.identity);
        }
    }
}