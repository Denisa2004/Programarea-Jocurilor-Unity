using System.Collections;
using UnityEngine;

namespace PowerUps
{
    public class CoinMultiplierPowerUp : MonoBehaviour
    {
        [Header("Multiplier Settings")]
        public int multiplier = 2;
        public float duration = 5f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                StartCoroutine(ActivateMultiplier());
            }
        }

        private IEnumerator ActivateMultiplier()
        {
            // Disable visuals/collider
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            var rend = GetComponent<Renderer>();
            if (rend != null) rend.enabled = false;

            // Activate Multiplier
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.SetMultiplier(multiplier);
            }

            yield return new WaitForSeconds(duration);

            // Reset Multiplier
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.SetMultiplier(1);
            }

            Destroy(gameObject);
        }
    }
}
