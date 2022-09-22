using UnityEngine;
using System.Collections;

namespace RobotFighter.Core
{
    internal class FuelTank : MonoBehaviour, IFuelSource
    {

        public event IFuelSource.EliminatedEventHandler BeingDestroyed;

        private event IFuelSource.ConsumedEventHandler _Consumed;

        public event IFuelSource.ConsumedEventHandler Consumed
        {
            add
            {
                //To make sure only one robot can subscribe to the event
                if (_Consumed is null)
                    _Consumed += value;
            }
            remove
            {
                _Consumed -= value;
            }
        }


        [SerializeField] int capacity;

        [Header("Elevation Parameters")]

        [Range(0, 1f)][SerializeField] float minAltitude;
        [Range(0, 2f)][SerializeField] float maxAltitude;
        [Range(0.1f, 1f)][SerializeField] float elevationSpeed;

        [Header("Rotation Parameters")]

        [Range(0, 360f)][SerializeField] float peakRotation;
        [Range(0.1f, 1f)][SerializeField] float rotationSpeed;

        [Header("Animation Parameters")]

        [SerializeField] int frameCount;
        [SerializeField] int fps;
        [Range(0.01f, 1f)][SerializeField] float minScale;


        private float elevationMultiplier;
        private float time;
        private bool beingConsumed;

        private void Awake()
        {
            elevationMultiplier = 1f / (maxAltitude - minAltitude);
        }

        private void Update()
        {
            if (beingConsumed)
                return;

            time += Time.deltaTime;

            transform.position = new Vector3(transform.position.x, minAltitude + elevationMultiplier * Mathf.Abs(Mathf.Sin(time * elevationSpeed)), transform.position.z);
            transform.rotation = Quaternion.Euler(0, peakRotation * Mathf.Cos(time * rotationSpeed), 0);
        }

        //Start shrink animation when collided with a robot
        private void OnTriggerEnter(Collider other)
        {
            RobotFighter robot;

            if (!other.TryGetComponent<RobotFighter>(out robot))
                return;

            if (beingConsumed)
                return;

            beingConsumed = true;
            StartCoroutine(Shrink());
        }

        //Shrink animation
        IEnumerator Shrink()
        {
            float animationLength = (float)frameCount / (float)fps;
            WaitForSeconds frameLength = new WaitForSeconds(animationLength / frameCount);

            float currentScale = transform.localScale.x;

            float stepSize = (currentScale - minScale) / frameCount;

            for (int i = 0; i < frameCount; i++)
            {
                currentScale -= stepSize;

                transform.localScale = Vector3.one * currentScale;

                yield return frameLength;
            }

            Destroy(this);
        }

        protected virtual void OnConsumed()
        {
            _Consumed?.Invoke(this, capacity);
        }

        protected virtual void OnEliminated()
        {
            BeingDestroyed.Invoke(this);
        }

        //Randomize the vertical position so fuel tanks won't sync with each other
        private void OnEnable()
        {
            time = Random.Range(1f, 100f);
        }

        private void OnDestroy()
        {
            OnConsumed();
            OnEliminated();
        }
    }
}