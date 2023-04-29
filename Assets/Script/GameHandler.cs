using System;
using TMPro;
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
        public float rotationSpeed = 10f;

        public float cameraDistanceFromEarth = 10f; // The distance from the Earth sphere to spawn the camera

        public GameObject treasurePrefab;
        public GameObject playerPrefab;
        public GameObject playerShip;


        // This method is called when the game starts
        void Start()
        {
            SpawnCamera();
            SpawnPlayerShip();
            
            scoreText = GameObject.Find("ScoreText").GetComponent<TMP_Text>();
            UpdateScoreText();
            SpawnTreasures();
        }

        private void Update()
        {
            // Get the current rotation of the world and ship
            Quaternion earthRotation = transform.rotation;
            Quaternion shipRotation = transform.rotation;

            // Calculate the new rotation of the world
            float right = 0f; // Rotate around the Y-axis based on 'A' or 'D' input
            float up = -Input.GetAxis("Vertical"); // Rotate around the X-axis based on 'W' input

            if (Input.GetKey(KeyCode.W))
            {
                right = Input.GetAxis("Horizontal");
            }

            // Rotate the Earth sphere around the X- and Y-axes
            earthRotation = Quaternion.Euler(up,right,0f) * earthRotation;
            
            shipRotation = Quaternion.Euler(0f,0f,-right*10);
            
            // Set the new rotation of the world
            transform.rotation = earthRotation;
            playerShip.transform.rotation = shipRotation;
        }

        public void SpawnCamera()
        {
            // Calculate the initial position of the camera
            Vector3 position = -transform.forward * cameraDistanceFromEarth;

            // Spawn the camera at the calculated position
            GameObject cameraObject = new GameObject("Main Camera");
            Camera cameraComponent = cameraObject.AddComponent<Camera>();
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
            ProduceNewItem();
        }

        // This method updates the score text
        void UpdateScoreText()
        {
            scoreText.text = "Score: " + score.ToString();
        }

        // This method produces a new item at the spawn point
        void ProduceNewItem()
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float height = Random.Range(-1f, 1f);

            Vector3 pos = new Vector3(Mathf.Cos(angle) * Mathf.Sqrt(1 - height * height),
                height,
                Mathf.Sin(angle) * Mathf.Sqrt(1 - height * height));
            pos *= radius;

            GameObject treasure = Instantiate(treasurePrefab, pos, Quaternion.identity);
            treasure.transform.parent = transform; // Make the Earth sphere the parent of the Treasure object
            
            // Calculate the direction from the Treasure to the center of the Earth
            Vector3 centerDirection = transform.position.normalized;

            // Calculate the normal vector to the Earth's surface at the Treasure's position
            Vector3 surfaceNormal = treasure.transform.position - centerDirection * radius;

            if (surfaceNormal.magnitude > 0f) {
                // Rotate the Treasure to face the surface normal
                treasure.transform.rotation = Quaternion.LookRotation(-surfaceNormal, centerDirection);
            }
            Debug.Log("Surface normal: " + surfaceNormal);
            Debug.Log("Center direction: " + centerDirection);
        }

        void SpawnTreasures()
        {
            int numTreasures = 10; // Change this to the number of treasures you want to spawn
             // Change this to the radius of the Earth sphere

            for (int i = 0; i < numTreasures; i++)
            {
                ProduceNewItem();
            }
        }
    }
}




//rotation = Quaternion.Euler(0f, rotationSpeed * Time.deltaTime, 0f) * rotation;
