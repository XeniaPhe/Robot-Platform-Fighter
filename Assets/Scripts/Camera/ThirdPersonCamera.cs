using UnityEngine;

namespace RobotFighter.Cam
{
    internal class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] Transform player;
        [SerializeField] Vector3 relativeToPlayer;

        void Update()
        {
            //Follow player at a constant distance
            transform.position = player.transform.position + relativeToPlayer;
            transform.LookAt(player);
        }
    }
}