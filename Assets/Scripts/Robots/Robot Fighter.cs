using UnityEngine;
using System.Collections.Generic;

namespace RobotFighter.Core
{
    internal abstract class RobotFighter : MonoBehaviour, IFuelSource
    {
        public event IFuelSource.ConsumedEventHandler Consumed;
        public event IFuelSource.EliminatedEventHandler BeingDestroyed;

        [Header("Physics Parameters")]
        [SerializeField] protected float speedMultiplier;
        [SerializeField] protected float rotationSpeed;
        [SerializeField] protected float maxDeltaRotation;
        [SerializeField] protected float collisionBounciness;

        [Header("Fuel Parameters")]
        [SerializeField] protected int initialFuel;
        [SerializeField] protected float massPerUnitFuel;
        [SerializeField] protected float scalePerUnitFuel;


        internal Rigidbody rb;
        protected Animator animator;

        protected int fuel;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            speedMultiplier *= Time.fixedDeltaTime;
            fuel = initialFuel;
        }

        protected virtual void Update()
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, FindDirection(), rotationSpeed);
            Vector3 rot = transform.rotation.eulerAngles;

            rot.x = 0;
            rot.z = 0;

            transform.rotation = Quaternion.Euler(rot);
        }

        //Adds a force each physics frame for the robot to move with constant speed (when it balances with drag force)
        protected virtual void FixedUpdate()
        {
            rb.AddForce(transform.forward * speedMultiplier * rb.mass, ForceMode.Force);
        }

        //Collision physics
        protected virtual void OnCollisionEnter(Collision collision)
        {
            RobotFighter other;

            if (!collision.gameObject.TryGetComponent<RobotFighter>(out other))
                return;

            if(Consumed != null)
            {
                foreach (var del in Consumed.GetInvocationList())
                {
                    Consumed -= (IFuelSource.ConsumedEventHandler)del;
                }
            }

            Consumed += other.Grow;

            Vector3 forward = transform.forward;
            float collisionAngle = Vector3.Angle(other.transform.position - transform.position, forward);

            if (collisionAngle < 90f)
            {
                float cosAngle = Mathf.Cos(collisionAngle * Mathf.Deg2Rad);
                Vector3 impulse = rb.mass * forward * collisionBounciness * cosAngle;

                other.rb.AddForce(impulse,ForceMode.Impulse);
                rb.AddForce(-impulse, ForceMode.Impulse);
            }
        }

        //Checks for fuel tanks and picks them if possible
        protected virtual void OnTriggerEnter(Collider other)
        {
            FuelTank tank;

            if (!other.TryGetComponent<FuelTank>(out tank))
                return;

            tank.Consumed += Grow;
        }

        //Returns the rotation the robot should go
        protected abstract Quaternion FindDirection();

        //Grows the robot when a fuel tank is picked or when a robot is pushed down
        protected virtual void Grow(object sender, int fuel)
        {
            this.fuel += fuel;

            float scale = transform.localScale.x + fuel * scalePerUnitFuel;

            transform.localScale = Vector3.one * scale;
            rb.mass += fuel * massPerUnitFuel;
        }

        //Give fuel to the robot who pushed this one
        protected virtual void OnConsumed()
        {
            Consumed?.Invoke(this, fuel);
        }

        //Calls a method in GameManager to update the robot list
        protected virtual void OnEliminated()
        {
            BeingDestroyed?.Invoke(this);
        }

        //Stars the running animation
        private void OnEnable()
        {
            animator.SetBool("GameStarted", true);
        }
    }
}