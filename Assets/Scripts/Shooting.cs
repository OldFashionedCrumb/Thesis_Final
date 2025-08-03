using UnityEngine;
    
    public class Shooting : MonoBehaviour
    {
        public GameObject shootingItem;
        public Transform shootingPoint;
        public bool canShoot = true;
        [SerializeField] float shootDelay = 0.5f;
        [SerializeField] float lastShootTime = -999f;
        public float projectileSpeed = 10f;
        public Transform playerTransform; // Assign the player transform in the inspector
    
        private void Update()
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
    
            // Flip player to face mouse
            if (playerTransform != null)
            {
                if (mouseWorldPos.x < playerTransform.position.x)
                    playerTransform.localScale = new Vector3(-Mathf.Abs(playerTransform.localScale.x), playerTransform.localScale.y, playerTransform.localScale.z);
                else
                    playerTransform.localScale = new Vector3(Mathf.Abs(playerTransform.localScale.x), playerTransform.localScale.y, playerTransform.localScale.z);
            }
    
            if (Input.GetMouseButtonDown(0) && Time.time >= lastShootTime + shootDelay)
            {
                Shoot(mouseWorldPos);
                lastShootTime = Time.time;
            }
        }
    
        void Shoot(Vector3 mouseWorldPos)
        {
            if (!canShoot)
                return;
        
            Vector2 direction = (mouseWorldPos - shootingPoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
        
            GameObject si = Instantiate(shootingItem, shootingPoint.position, rotation);
        
            // Sync fireball scale with player direction
            if (playerTransform != null)
            {
                Vector3 scale = si.transform.localScale;
                scale.x = Mathf.Sign(playerTransform.localScale.x) * Mathf.Abs(scale.x);
                si.transform.localScale = scale;
            }
        
            Rigidbody2D rb = si.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * projectileSpeed;
            }
        }
    }