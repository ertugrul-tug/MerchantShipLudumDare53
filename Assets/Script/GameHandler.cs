using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Script
{
    public class GameHandler : MonoBehaviour
    {

        public int score = 0;
        public int inventory = 0;
        public TMP_Text scoreText;
        public float radius = 5f;
        public float moveSpeed = 1f;
        public float rotationSpeed = 1f;

        public float right = 0f;
        public float up = 0f;
        public float turn = 0f;

        private bool _isIsland = false;
        private bool _isStopping = false; // Flag to indicate if the ship is stopping
        private float _currentRotationSpeed = 0f; // Current rotation speed of the ship
        private float _currentMoveSpeed = 0f; // Current movement speed of the ship
        private const float StopSpeed = 2f; // Speed at which to stop the ship

        public float cameraDistanceFromEarth = 10f; // The distance from the Earth sphere to spawn the camera

        public GameObject cloudPrefab;
        public GameObject playerPrefab;
        public GameObject playerShip;
        public GameObject islandPrefab;
        public GameObject cameraPrefab;
        public GameObject directionalLight;
        public GameObject cloud1;
        
        public Collider playerShipCollider;
        public float islandCollisionDistance = 1f;

        public List<Vector3> existingIslandPositions = new List<Vector3>();
        List<Vector3> islandPositions = new List<Vector3>(); // Keep track of the positions of the islands


        // This method is called when the game starts
        void Start()
        {
            SpawnCamera();
            SpawnPlayerShip();
            SpawnIslands();
            scoreText = GameObject.Find("ScoreText").GetComponent<TMP_Text>();
            UpdateScoreText();
            SpawnClouds();
            // Get the directional light object from the scene
            directionalLight = GameObject.Find("Sky");
            directionalLight.transform.parent = transform; // Make the Earth sphere the parent of the Directional Light object
            
            // Assign the playerShip's collider to the playerShipCollider field
            playerShipCollider = playerShip.GetComponent<Collider>();
        }



        private void Update()
        {
            Vector3 direction1 = new Vector3(0.5f, 5f, 6f);
            direction1 = (direction1 + playerShip.transform.position);
            float distance1 = 2f;
            Vector3 direction2 = new Vector3(-0.5f, 5f, 6f);
            direction2 = (direction2 + playerShip.transform.position);
            float distance2 = 2f;
            
            RaycastHit hitInfo1, hitInfo2;
            bool hit1 = Physics.Raycast(playerShip.transform.position, direction1, out hitInfo1, distance1);
            bool hit2 = Physics.Raycast(playerShip.transform.position, direction2, out hitInfo2, distance2);

            if (hit1 || hit2)
            {
                if (hit1) Debug.DrawRay(playerShip.transform.position, direction1 * hitInfo1.distance, Color.yellow);
                if (hit2) Debug.DrawRay(playerShip.transform.position, direction2 * hitInfo2.distance, Color.yellow);
                Debug.Log("Player collided with island");
                _isIsland = true;
                Debug.Log("isIsland"+_isIsland);
            }
            else
            {
                Debug.DrawRay(playerShip.transform.position, direction1 * 1000, Color.white);
                Debug.DrawRay(playerShip.transform.position, direction2 * 1000, Color.white);
                _isIsland = false;
                Debug.Log("isIsland"+_isIsland);
            }
            
            
            
            // Get the current rotation of the world and ship
            Quaternion earthRotation = transform.rotation;
            Quaternion shipRotation = playerShip.transform.rotation;
            
            turn = Input.GetAxis("Horizontal"); // Rotate around the Y-axis based on 'A' or 'D' input

            if (!_isIsland)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    // Calculate the new rotation of the world
                    right = Input.GetAxis("Horizontal"); // Rotate around the Y-axis based on 'A' or 'D' input
                    up = -Input.GetAxis("Vertical"); // Rotate around the X-axis based on 'W' input
                    // Reset the rotation and movement speed to their original values
                    _currentRotationSpeed = rotationSpeed;
                    _currentMoveSpeed = moveSpeed;
                    _isStopping = false;
                }
                else if (!_isStopping)
                {
                    // Gradually reduce the rotation and movement speed to zero over time
                    _currentRotationSpeed = Mathf.Lerp(_currentRotationSpeed, 0f, Time.deltaTime * StopSpeed);
                    _currentMoveSpeed = Mathf.Lerp(_currentMoveSpeed, 0f, Time.deltaTime * StopSpeed);

                    // If the rotation and movement speed are close enough to zero, set them to zero
                    if (Mathf.Abs(_currentRotationSpeed) < 0.01f)
                    {
                        _currentRotationSpeed = 0f;
                    }

                    if (Mathf.Abs(_currentMoveSpeed) < 0.01f)
                    {
                        _currentMoveSpeed = 0f;
                    }

                    // If the speed is zero, set the stopping flag to true
                    if (_currentRotationSpeed == 0f && _currentMoveSpeed == 0f)
                    {
                        _isStopping = true;
                    }
                }
                else if (_isStopping)
                {
                    right = 0f;
                    up = 0f;
                }
            }else
            {
                right = 0f;
                up = 0f;
            }

            if (_currentMoveSpeed > 1f)
            {
                // Rotate the Earth sphere around the X- and Y-axes
                earthRotation = Quaternion.Euler(up * _currentMoveSpeed, right * _currentRotationSpeed, 0f) * earthRotation;
                // Rotate ship accordingly
                shipRotation = Quaternion.Euler(0f, 0f, -right * 10 * _currentRotationSpeed);
            }
            else
            {
                // Rotate the Earth sphere around the X- and Y-axes
                earthRotation = Quaternion.Euler(up * _currentMoveSpeed, right * _currentRotationSpeed, turn) * earthRotation;
                // Rotate ship accordingly
                shipRotation = Quaternion.Euler(0f, 0f, -right * 10 * _currentRotationSpeed);
            }
            
            // Set the new rotation of the world
            transform.rotation = earthRotation;
            playerShip.transform.rotation = shipRotation;
            directionalLight.transform.rotation = Quaternion.Euler(0f, rotationSpeed * 10f * Time.deltaTime, 0f) * directionalLight.transform.rotation;
        }

        public void SpawnCamera()
        {
            // Calculate the initial position of the camera
            Vector3 position = -transform.forward * cameraDistanceFromEarth;

            // Spawn the camera at the calculated position
            GameObject cameraObject = Instantiate(cameraPrefab, position, Quaternion.identity);
            cameraObject.transform.position = position;
            cameraObject.transform.LookAt(transform.position);
        }

        public void SpawnPlayerShip()
        {
            // Calculate the initial position of the player
            Vector3 position = -transform.forward * radius;

            // Spawn the player at the calculated position
            playerShip = Instantiate(playerPrefab, position, Quaternion.identity);
            playerShip.transform.position = position;
            playerShip.transform.LookAt(transform.position);
        }

        // This method is called when the player collects an item
        public void CollectItem()
        {
            inventory++;
            UpdateScoreText();
        }

        // This method is called when the player delivers an item
        public void DeliverItem()
        {
            score++;
            UpdateScoreText();
        }

        // This method updates the score text
        void UpdateScoreText()
        {
            scoreText.text = "Score: " + score.ToString();
        }

        void SpawnClouds()
        {
            int numClouds = 480; // Change this to the number of clouds you want to spawn
            float radius = 8f; // Change this to the radius of the sphere where you want to spawn the clouds

            for (int i = 0; i < numClouds; i+=12)
            {
                float x, y, z;

                // Generate a random point on a sphere
                float u = Random.Range(0f, 1f);
                float v = Random.Range(0f, 1f);
                float theta = 2 * Mathf.PI * u;
                float phi = Mathf.Acos(2 * v - 1);

                x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
                y = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
                z = radius * Mathf.Cos(phi);

                // Spawn the first cloud
                Vector3 pos = new Vector3(x, y, z);
                cloud1 = Instantiate(cloudPrefab, pos, Quaternion.identity);
                cloud1.transform.parent = transform;

                // Spawn the next 11 clouds in a cluster around the first cloud
                for (int j = 1; j < 12; j++) 
                {
                    float offsetX = Random.Range(-0.5f, 0.5f);
                    float offsetY = Random.Range(-0.5f, 0.5f);
                    float offsetZ = Random.Range(-0.5f, 0.5f);

                    Vector3 offset = new Vector3(offsetX, offsetY, offsetZ);
                    Vector3 clusterPos = pos + offset;

                    GameObject cloud = Instantiate(cloudPrefab, clusterPos, Quaternion.identity);
                    cloud.transform.parent = cloud1.transform; // Make the first cloud the parent of the other clouds in the cluster
                }
            }
        }
                
        void SpawnIslands()
        {
            int numIslands = 5; // Change this to the number of islands you want to spawn
            float minDistance = 7f; // Change this to the minimum distance between islands
            float radius = transform.localScale.x / 2f; // Change this to the radius of the Earth sphere
            int maxAttempts = 50; // Maximum number of attempts to find a suitable position for an island
            float minDistanceToPlayer = 2f; // Minimum distance between the player ship and a new island

            for (int i = 0; i < numIslands; i++)
            {
                int attempts = 0; // Keep track of the number of attempts to find a suitable position for an island

                while (true) // Keep searching for a suitable position for the island
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    float height = Random.Range(-1f, 1f);

                    Vector3 pos = new Vector3(Mathf.Cos(angle) * Mathf.Sqrt(1 - height * height),
                        height,
                        Mathf.Sin(angle) * Mathf.Sqrt(1 - height * height));
                    pos *= radius;

                    // Check if the new island position is too close to the player ship
                    if (Vector3.Distance(pos, playerShip.transform.position) >= minDistanceToPlayer && 
                        !IslandsTooClose(pos, radius, islandPositions, minDistance))
                    {
                        islandPositions.Add(pos);
                        GameObject island = Instantiate(islandPrefab, pos, Quaternion.identity);
                        island.transform.parent = transform; // Make the Earth sphere the parent of the island object

                        // Calculate the direction from the island to the center of the Earth
                        Vector3 centerDirection = transform.position.normalized;

                        // Calculate the normal vector to the Earth's surface at the island's position
                        Vector3 surfaceNormal = island.transform.position - centerDirection * radius;
                        island.tag = "Island";

                        if (surfaceNormal.magnitude > 0f)
                        {
                            // Rotate the island to face the surface normal
                            island.transform.rotation = Quaternion.LookRotation(-surfaceNormal, centerDirection);
                        }

                        break; // Exit the loop and move to the next island
                    }

                    attempts++; // Increase the number of attempts to find a suitable position

                    if (attempts >= maxAttempts) // Failed to find a suitable position after the maximum number of attempts
                    {
                        Debug.LogWarning("Failed to find a suitable position for island #" + i);
                        break; // Exit the loop and move to the next island
                    }
                }
            }
        }
        private bool IslandsTooClose(Vector3 pos, float radius, List<Vector3> islandPositions, float minDistance)
        {
            foreach (var islandPos in islandPositions)
            {
                float lat1 = Mathf.Asin(pos.y / radius) * Mathf.Rad2Deg;
                float lon1 = Mathf.Atan2(pos.z, pos.x) * Mathf.Rad2Deg;
                float lat2 = Mathf.Asin(islandPos.y / radius) * Mathf.Rad2Deg;
                float lon2 = Mathf.Atan2(islandPos.z, islandPos.x) * Mathf.Rad2Deg;

                float dLat = Mathf.Deg2Rad * (lat2 - lat1);
                float dLon = Mathf.Deg2Rad * (lon2 - lon1);

                float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                          Mathf.Cos(Mathf.Deg2Rad * lat1) * Mathf.Cos(Mathf.Deg2Rad * lat2) *
                          Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

                float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

                float distance = radius * c;

                if (distance < minDistance)
                {
                    return true;
                }
            }

            return false;
        }
    }
}




//rotation = Quaternion.Euler(0f, rotationSpeed * Time.deltaTime, 0f) * rotation;
