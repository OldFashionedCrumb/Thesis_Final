using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(BoxCollider2D))]
public class EnemyAI : MonoBehaviour
{
    //Reference to waypoints
    public List<Transform> points;
    //The int value for next point index
    public int nextID = 0;
    //The value of that applies to ID for changing
    int idChangeValue = 1;
    //Speed of movement or flying
    public float speed = 2;
    
    //Chase player parameters
    public float chaseSpeed = 3f;
    public float chaseRange = 4f;

    //Player detection fields
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    private Transform playerTransform;
    private bool chasingPlayer = false;
    
    //Raycast parameters for ground detection
    public float raycastDistance = 0.5f;
    public Vector2 raycastOffset = new Vector2(0.3f, 0f);
    
    private bool isScene2 = false;

    private void Start()
    {
        isScene2 = SceneManager.GetActiveScene().buildIndex == 2 || SceneManager.GetActiveScene().name == "Scene2";
        if (isScene2)
        {
            speed = 4f;
            chaseSpeed = 7f;
            chaseRange = 7f;
        }
    }

    private void Reset()
    {
        Init();
    }

    void Init()
    {
        //Make box collider trigger
        GetComponent<BoxCollider2D>().isTrigger = true;

        //Create Root object
        GameObject root = new GameObject(name + "_Root");
        //Reset Position of Root to enemy object
        root.transform.position = transform.position;
        //Set enemy object as child of root
        transform.SetParent(root.transform);
        //Create Waypoints object
        GameObject waypoints = new GameObject("Waypoints");
        //Make waypoints object child of root
        waypoints.transform.SetParent(root.transform);
        waypoints.transform.position = root.transform.position;
        //Create two points (gameobject) and reset their position to waypoints objects
        //Make the points children of waypoint object
        GameObject p1 = new GameObject("Point1");
        p1.transform.SetParent(waypoints.transform);
        p1.transform.position = root.transform.position;

        GameObject p2 = new GameObject("Point2");
        p2.transform.SetParent(waypoints.transform);
        p2.transform.position = root.transform.position;

        //Init points list then add the points to it
        points = new List<Transform>();
        points.Add(p1.transform);
        points.Add(p2.transform);
        
        // Initialize ground layer
        groundLayer = LayerMask.GetMask("Ground");
    }

    private void Update()
    {
        MoveToNextPoint();
        CheckGroundCollision();
    }
    bool IsPlayerInChaseRange()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.transform.position) <= chaseRange;
    }
    void CheckGroundCollision()
    {
        // Determine raycast direction based on the enemy's current movement direction
        Vector2 rayDirection = transform.localScale.x < 0 ? Vector2.right : Vector2.left;
    
        // Calculate the raycast origin with offset in the movement direction
        Vector2 rayOrigin = (Vector2)transform.position + 
                            (transform.localScale.x < 0 ? raycastOffset : new Vector2(-raycastOffset.x, raycastOffset.y));
    
        // Perform the raycast
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, raycastDistance, groundLayer);
    
        // Debug visualization of the raycast
        Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, hit.collider != null ? Color.red : Color.green);
    
        // If we hit any ground object (using the layer, not tag)
        if (hit.collider != null)
        {
            // Change direction by inverting the idChangeValue
            idChangeValue *= -1;
        
            // Set nextID to force direction change
            nextID += idChangeValue;
        
            // Clamp the nextID within valid range
            nextID = Mathf.Clamp(nextID, 0, points.Count - 1);
        }
    }

    bool IsPlayerBetweenPoints()
    {
        if (points.Count < 2) return false;
        
        // Find player in the scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;
        
        Vector2 playerPos = player.transform.position;
        Vector2 point1 = points[0].position;
        Vector2 point2 = points[points.Count - 1].position;
        
        // Calculate the min and max bounds of the area between points
        float minX = Mathf.Min(point1.x, point2.x);
        float maxX = Mathf.Max(point1.x, point2.x);
        float minY = Mathf.Min(point1.y, point2.y);
        float maxY = Mathf.Max(point1.y, point2.y);
        
        // Add some margin to the detection area
        float margin = 1.0f;
        minX -= margin;
        maxX += margin;
        minY -= margin;
        maxY += margin;
        
        // Check if player is within these bounds
        return playerPos.x >= minX && playerPos.x <= maxX && 
               playerPos.y >= minY && playerPos.y <= maxY;
    }

    void MoveToNextPoint()
    {
        bool playerDetected = isScene2 ? IsPlayerInChaseRange() : IsPlayerBetweenPoints();

        if (playerDetected)
        {
            chasingPlayer = true;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            playerTransform = player.transform;

            if (playerTransform.position.x > transform.position.x)
                transform.localScale = new Vector3(-1, 1, 1);
            else
                transform.localScale = new Vector3(1, 1, 1);

            float moveSpeed = isScene2 ? chaseSpeed : speed;
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
            return;
        }
        else
        {
            chasingPlayer = false;
        }

        Transform goalPoint = points[nextID];
        if (goalPoint.transform.position.x > transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);

        transform.position = Vector2.MoveTowards(transform.position, goalPoint.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, goalPoint.position) < 0.2f)
        {
            if (nextID == points.Count - 1)
                idChangeValue = -1;
            if (nextID == 0)
                idChangeValue = 1;
            nextID += idChangeValue;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            FindObjectOfType<LifeCount>().LoseLife();
        }
    }
}