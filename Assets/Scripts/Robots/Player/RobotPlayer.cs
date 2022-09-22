using UnityEngine;

namespace RobotFighter.Core
{
    internal class RobotPlayer : RobotFighter
    {
        internal delegate void GrowingEventHandler(int fuelEarned, float scale);
        internal event GrowingEventHandler Growing;

        [SerializeField] Camera cam;
        [SerializeField] float z;

        private int score;
        internal int Score => score;
        private float timer = 0;

        protected override void Update()
        {
            timer += Time.deltaTime;

            if(timer >= 1f)
            {
                timer -= 1f;
                score += 50;
            }

            base.Update();
        }

        protected override Quaternion FindDirection()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector3 touchPos = new Vector3(touch.position.x, touch.position.y, z);

                touchPos = cam.ScreenToWorldPoint(touchPos);
                touchPos.y = transform.position.y;

                return Quaternion.LookRotation(touchPos - transform.position, Vector3.up);
            }

            return transform.rotation;
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            if (!other.tag.Equals("Drop Detector"))
                return;

            GameManager.Instance.OpenResultScreen();
        }

        protected override void Grow(object sender, int fuel)
        {
            base.Grow(sender, fuel);
            score += fuel * 2;

            if(sender.GetType() == typeof(RobotFighter))
            {
                score += (int)(((RobotFighter)sender).rb.mass * 15);
            }

            OnGrowing(fuel);
        }

        //Calls the UI to update
        protected virtual void OnGrowing(int fuelEarned)
        {
            Growing.Invoke(fuelEarned, transform.localScale.x);
        }
    }
}