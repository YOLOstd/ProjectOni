using UnityEngine;
using System.Collections.Generic;
using TMPro; // Assuming TextMeshPro is used in Unity 6 projects

namespace ProjectOni.UI
{
    public class DamageNumbersManager : MonoBehaviour
    {
        [Header("Prefab & Pool Settings")]
        [SerializeField] private GameObject damageTextPrefab;
        [SerializeField] private int poolSize = 20;

        private Queue<GameObject> _pool = new Queue<GameObject>();

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            if (damageTextPrefab == null) return;

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(damageTextPrefab, transform);
                obj.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public void ShowDamage(float amount, Vector3 position)
        {
            if (_pool.Count == 0) return;

            GameObject obj = _pool.Dequeue();
            obj.transform.position = position;
            obj.SetActive(true);

            // Set text (TMP setup)
            if (obj.TryGetComponent(out TextMeshProUGUI text))
            {
                text.text = amount.ToString("F0");
            }

            // Return to pool after some time (simplified - normally handled by the text script itself)
            StartCoroutine(ReturnToPool(obj, 1f));
        }

        private System.Collections.IEnumerator ReturnToPool(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}
