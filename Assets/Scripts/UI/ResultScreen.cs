using UnityEngine;
using TMPro;

namespace RobotFighter.UI
{
    internal class ResultScreen : ClosableMenu
    {
        [SerializeField] TMP_Text rankText;
        [SerializeField] TMP_Text scoreText;

        internal void Instantiate(int rank, int score)
        {
            rankText.text = "You are #" + rank.ToString() + " !!!";
            scoreText.text = score.ToString();
        }
    }
}