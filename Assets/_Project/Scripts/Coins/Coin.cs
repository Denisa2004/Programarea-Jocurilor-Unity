using UnityEngine;

public class Coin : MonoBehaviour
{
    //the value this coin gives when collected
    [SerializeField] private int value = 1;

    //triggered when another collider enters this coin's trigger zone
    private void OnTriggerEnter(Collider other)
    {
        //check if the object entering the trigger is the Player
        if (other.CompareTag("Player"))
        {
            //add coin value to the total using the CoinManager singleton
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.AddCoins(value);
            }

            //remove the coin from the scene after collection
            Destroy(gameObject);
        }
    }

}
