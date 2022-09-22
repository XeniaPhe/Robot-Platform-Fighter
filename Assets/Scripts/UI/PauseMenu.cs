using UnityEngine;
using UnityEngine.UI;
using RobotFighter.Core;

namespace RobotFighter.UI
{
    internal class PauseMenu : ClosableMenu
    {
        public override void Open()
        {
            Time.timeScale = 0;
            base.Open();
        }

        public override void Close()
        {
            Time.timeScale = 1f;
            base.Close();
        }

        public void Restart()
        {
            GameManager.Instance.RestartGame();
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}