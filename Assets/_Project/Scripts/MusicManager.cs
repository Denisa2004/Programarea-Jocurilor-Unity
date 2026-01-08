using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    void Awake()
    {
        // Check if an instance of this MusicManager already exists
        if (instance == null)
        {
            instance = this;
            // This command is magical: it doesn't destroy the object when loading a new scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If a manager already exists (for example, we went back to menu), we destroy the new one
            Destroy(gameObject);
        }
    }
}