using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using RobotFighter.Core.AI;
using RobotFighter.UI.Management;

namespace RobotFighter.Core
{
    internal class GameManager : MonoBehaviour
    {
        enum GameState
        {
            Starting,
            Continuing,
            Ending
        }

        #region Singleton

        private static GameManager instance;
        internal static GameManager Instance => instance;

        #endregion

        internal delegate void FuelTanksUpdatedEventHandler(List<FuelTank> fuelTanks);
        internal delegate void RobotsUpdatedEventHandler(List<RobotFighter> fuelTanks);
        internal delegate void RestartEventHandler();

        internal event FuelTanksUpdatedEventHandler FuelTanksUpdated;
        internal event RobotsUpdatedEventHandler RobotsUpdated;
        internal event RestartEventHandler Restart;

        [Header("Robot parameters")]
        [SerializeField] RobotPlayer player;
        [SerializeField] Transform robotParent;
        [SerializeField] RobotFighterAI robotPrefab;
        [Range(0.1f, 0.9f)][SerializeField] float distanceFromCenter;

        [Header("Platform Parameters")]
        [SerializeField] Transform plane;
        [SerializeField] float planeSize;
        [SerializeField] float unitSquarePerRobot;

        [Header("Fuel Tank Parameters")]
        [SerializeField] Transform fuelTankParent;
        [SerializeField] FuelTank fuelTankPrefab;
        [SerializeField] float minDistanceBetweenTanks;
        [SerializeField] int maxFuelTankCount;
        [Range(0.1f, 3f)][SerializeField] float fuelTankSpawnTime;

        [Header("Time Parameters")]
        [SerializeField] float gameStartTime;
        [SerializeField] float roundTime;
        [SerializeField] float resultScreenTime;


        private List<RobotFighter> robots = new();
        private List<FuelTank> fuelTanks = new();
        private GameState state;
        private float timer = 0;
        private float spawnTimer = 0;
        private UIManager uiManager;


        internal float Timer => timer;
        internal float RoundTime => roundTime;
        internal float StartTime => gameStartTime;
        internal float PlaneSize => planeSize;
        internal int RobotCount => robots.Count;


        private void Awake()
        {
            if (instance is null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }

            float scale = planeSize / 10f;
            plane.transform.localScale = Vector3.one * scale;
            distanceFromCenter *= planeSize / 2f;

            robots = new List<RobotFighter>();
            InitializeRobots();
        }

        private void Start()
        {
            timer = 0;
            spawnTimer = 0;
            uiManager = UIManager.Instance;
            uiManager.EnableStartScreen();
            state = GameState.Starting;
            SpawnFuelTanks();
        }

        private void Update()
        {
            timer += Time.deltaTime;

            //Do actions depending on the game state
            switch (state)
            {
                case GameState.Starting:

                    if (timer >= gameStartTime)
                        StartGame();

                    break;
                case GameState.Continuing:

                    spawnTimer += Time.deltaTime;

                    if (spawnTimer >= fuelTankSpawnTime)
                    {
                        spawnTimer -= fuelTankSpawnTime;
                        SpawnFuelTanks();
                    }

                    if (timer >= roundTime)
                        OpenResultScreen(true);

                    break;
                case GameState.Ending:

                    if (timer >= resultScreenTime)
                        RestartGame();

                    break;
            }
        }

        internal void StartGame()
        {
            timer = 0;
            RobotsUpdated?.Invoke(robots);
            state = GameState.Continuing;
            uiManager.DisableStartScreen();
            robots.ForEach(r => r.enabled = true);
        }

        internal void OpenResultScreen(bool timeout = false)
        {
            timer = 0;
            state = GameState.Ending;

            int rank = 1;
            if (timeout)
            {

                foreach (var robot in robots)
                {
                    if (robot.rb.mass > player.rb.mass)
                        rank++;
                }
            }
            else
            {
                rank = robots.Count;
            }

            uiManager.EnableResultsScreen(rank, player.Score);
        }

        //Instantiates the robots and places them on their places on the platform
        private void InitializeRobots()
        {
            Vector3 position = new Vector3(0, player.GetComponent<Rigidbody>().centerOfMass.y, 0);

            int robotCount = Mathf.RoundToInt(planeSize * planeSize / unitSquarePerRobot);

            float incrementAngle = 2 * Mathf.PI / (float)robotCount;
            float angle = 0;

            int playerPosition = Random.Range(0, robotCount);
            int firstGroup = robotCount - playerPosition - 1;
            int secondGroup = robotCount - firstGroup - 1;

            int robotNumber = 1;

            for (int i = 0; i < firstGroup; i++)
            {
                var robot = AdjustTransformAndAddToList(Instantiate<RobotFighterAI>(robotPrefab));
                SubscribeToEvents(robot);
            }

            AdjustTransformAndAddToList(player, true);

            for (int i = 0; i < secondGroup; i++)
            {
                var robot = AdjustTransformAndAddToList(Instantiate<RobotFighterAI>(robotPrefab));
                SubscribeToEvents(robot);
            }

            RobotFighter AdjustTransformAndAddToList(RobotFighter robot, bool player = false)
            {
                position.z = distanceFromCenter * Mathf.Cos(angle);
                position.x = distanceFromCenter * Mathf.Sin(angle);

                robot.transform.position = position;
                robot.transform.LookAt(Vector3.zero, Vector3.up);

                if(!player)
                {
                    robot.name = string.Concat("Robot #", robotNumber++);
                    robot.transform.SetParent(robotParent);
                }

                angle += incrementAngle;
                robots.Add(robot);

                return robot;
            }

            void SubscribeToEvents(RobotFighter robot)
            {
                robot.BeingDestroyed += OnRobotsUpdated;
                RobotsUpdated += ((RobotFighterAI)robot).UpdateRobots;
                FuelTanksUpdated += ((RobotFighterAI)robot).UpdateFuelTanks;
            }
        }

        //Spawns new fuel tanks on the platform
        private void SpawnFuelTanks()
        {
            FuelTank tank;
            Vector3 position = new Vector3(0, fuelTankPrefab.transform.position.y, 0);
            int count = maxFuelTankCount - fuelTanks.Count;

            float halfLength = planeSize / 2f;
            bool positionAvailable;

            for (int i = 0; i < count; i++)
            {
                do
                {
                    position.x = Random.Range(-halfLength, halfLength);
                    position.z = Random.Range(-halfLength, halfLength);

                    positionAvailable = true;

                    foreach (var t in fuelTanks)
                    {
                        if ((t.transform.position - position).magnitude < minDistanceBetweenTanks)
                        {
                            positionAvailable = false;
                            break;
                        }
                    }

                } while (!positionAvailable);

                tank = Instantiate<FuelTank>(fuelTankPrefab);
                tank.transform.position = position;
                tank.transform.SetParent(fuelTankParent);
                tank.BeingDestroyed += OnFuelTanksUpdated;
                fuelTanks.Add(tank);
            }

            FuelTanksUpdated?.Invoke(fuelTanks);
        }

        //Updates the robots list in each robot
        protected virtual void OnRobotsUpdated(object sender)
        {
            RobotFighterAI robot = (RobotFighterAI)sender;
            RobotsUpdated -= robot.UpdateRobots;
            FuelTanksUpdated -= robot.UpdateFuelTanks;
            robots.Remove(robot);

            if (robots.Count == 1)
            {
                OpenResultScreen();
            }
            else
            {
                RobotsUpdated?.Invoke(robots);
            }
        }

        //Updates the fuel tanks list in each robot
        protected virtual void OnFuelTanksUpdated(object sender)
        {
            fuelTanks.Remove((FuelTank)sender);
            FuelTanksUpdated?.Invoke(fuelTanks);
        }

        //Calls the singleton classes to make their instances null so the scene can reload successfully
        protected virtual void OnRestart()
        {
            Restart.Invoke();
        }

        //Restart scene
        internal void RestartGame()
        {
            instance = null;
            OnRestart();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}