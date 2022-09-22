using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotFighter.Core;
using RobotFighter.UI;

namespace RobotFighter.UI.Management
{
    internal class UIManager : MonoBehaviour
    {
        #region Singleton

        private static UIManager instance;

        internal static UIManager Instance => instance;

        #endregion

        [SerializeField] HUD hud;

        [Header("Closebale Menus")]
        [SerializeField] PauseMenu pauseMenu;
        [SerializeField] StartScreen startScreen;
        [SerializeField] ResultScreen resultsScreen;

        private void Awake()
        {
            if(instance is null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            if(Input.GetKey(KeyCode.Escape))
            {
                pauseMenu.Open();
            }
        }

        //For nullifying the static instance before restart
        private void Start()
        {
            GameManager.Instance.Restart += OnRestart;
        }

        internal void EnableStartScreen()
        {
            startScreen.Open();
        }

        internal void DisableStartScreen()
        {
            startScreen.Close();
        }

        internal void EnableResultsScreen(int rank, int score)
        {
            resultsScreen.Open();
            resultsScreen.Instantiate(rank, score);
        }

        internal void OnRestart()
        {
            instance = null;
        }
    }
}