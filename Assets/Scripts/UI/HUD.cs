using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotFighter.Core;
using System.Collections;
using System.Text;

namespace RobotFighter.UI
{
    internal class HUD : MonoBehaviour
    {
        [SerializeField] RobotPlayer player;

        [Header("Overlay Elements")]
        [SerializeField] TMP_Text totalScoreText;
        [SerializeField] TMP_Text robotCounterText;
        [SerializeField] TMP_Text timerText;

        [Header("WorldSpace Elements")]
        [SerializeField] Canvas worldSpaceCanvas;
        [SerializeField] TMP_Text scorePopText;

        private GameManager manager;
        private void Awake()
        {
            scorePopText.enabled = false;
        }

        private void Start()
        {
            manager = GameManager.Instance;
            player.Growing += ScaleUIAndShowScorePopup;
        }

        //Update HUD texts
        private void Update()
        {
            float time = manager.RoundTime - manager.Timer;
            int minutes = (int)(time / 60f);
            int seconds = Utility.RoundUp(time % 60f);

            StringBuilder builder = new StringBuilder();

            if(minutes > 0)
            {
                builder.Append(minutes);
                builder.Append(" : ");

                if(seconds < 10)
                    builder.Append("0");
            }

            builder.Append(seconds);

            timerText.text = builder.ToString();
            totalScoreText.text = player.Score.ToString();
            robotCounterText.text = manager.RobotCount.ToString();

            worldSpaceCanvas.transform.position = player.transform.position;
        }

        //Scale the popup text so that it will remain on top of the player
        private void ScaleUIAndShowScorePopup(int fuelEarned, float newScale)
        {
            StopAllCoroutines();
            worldSpaceCanvas.transform.localScale = Vector3.one * newScale;
            scorePopText.transform.localScale = Vector3.one / newScale;

            scorePopText.enabled = true;
            scorePopText.text = "+ " + fuelEarned.ToString();

            StartCoroutine(DisableScoreText());
        }

        IEnumerator DisableScoreText()
        {
            yield return new WaitForSeconds(1.5f);
            scorePopText.enabled = false;
        }
    }
}