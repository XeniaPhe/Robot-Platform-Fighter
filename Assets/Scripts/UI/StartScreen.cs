using System;
using UnityEngine;
using TMPro;
using RobotFighter.Core;

namespace RobotFighter.UI
{
    internal class StartScreen : ClosableMenu
    {
        [SerializeField] TMP_Text startTimer;

        GameManager manager;

        protected override void Start()
        {
            manager = GameManager.Instance;
            base.Start();
        }

        private void Update()
        {
            startTimer.text = Utility.RoundUp(manager.StartTime - manager.Timer).ToString();
        }
    }
}