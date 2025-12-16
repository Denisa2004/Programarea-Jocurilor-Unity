using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    void Awake()
    {
        // Verificăm dacă există deja o instanță a acestui MusicManager
        if (instance == null)
        {
            instance = this;
            // Această comandă este magică: nu distruge obiectul la încărcarea unei scene noi
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Dacă există deja un manager (de exemplu, ne-am întors în meniu), îl distrugem pe cel nou
            Destroy(gameObject);
        }
    }
}