using UnityEngine;

namespace RobotFighter.UI
{
    //Can be turned on and off
    internal class ClosableMenu : MonoBehaviour
    {
        Canvas canvasComponent;

        protected virtual void Start()
        {
            canvasComponent = GetComponent<Canvas>();
            Close();
        }

        public virtual void Close()
        {
            canvasComponent.enabled = false;
        }

        public virtual void Open()
        {
            canvasComponent.enabled = true;
        }
    }
}