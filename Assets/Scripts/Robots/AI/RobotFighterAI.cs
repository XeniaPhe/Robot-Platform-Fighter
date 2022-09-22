using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RobotFighter.Core.AI
{
    internal class RobotFighterAI : RobotFighter
    {
        [SerializeField] float maxFollowDistortion;
        [SerializeField] float minSafeDistanceFromEdge;
        [SerializeField] float safeZoneRatio;
        [SerializeField] float escapeConstant;
        [SerializeField] float attackConstant;
        [SerializeField] float scavengeConstant;

        private Vector3 targetPos;
        private float decisionTime = 0;
        private float edgeDistanceLimit;
        private float safeArea;
        private List<RobotFighter> threats = new();
        private List<RobotFighter> preys = new();
        private List<FuelTank> fuelTanks;

        private void Start()
        {
            edgeDistanceLimit = GameManager.Instance.PlaneSize / 2f - minSafeDistanceFromEdge;
            safeArea = GameManager.Instance.PlaneSize * safeZoneRatio;
        }

        protected override Quaternion FindDirection()
        {
            //To make sure the AI doesn't change its mind each frame
            Quaternion desiredRot;

            if (Mathf.Abs(transform.position.z) >= edgeDistanceLimit || Mathf.Abs(transform.position.z) >= edgeDistanceLimit)
            {
                //If the robot is on the edge, redirect the rotation to a more central area
                targetPos = new Vector3(Random.Range(-safeArea, safeArea), rb.centerOfMass.y, Random.Range(-safeArea, safeArea));
            }
            else if(GetDistanceFrom(targetPos) > 0.05f && Time.time - decisionTime < 0.8f)
            {
                return transform.rotation;
            }
            else
            {
                int count = threats.Count;
                int threatIndex = 0;
                float biggestThreat = 0;
                float temp;

                //Calculates the closest and strongest opponent and calculates a value that indicates how sensible to escape from it it would be
                for (int i = 0; i < count; i++)
                {
                    if ((temp = CalculateEscapeValue(threats[i])) > biggestThreat)
                    {
                        biggestThreat = temp;
                        threatIndex = i;
                    }
                }

                count = preys.Count;
                int preyIndex = 0;
                float easiestPrey = 0;

                //Finds the closest and easiest possible opponent and calculates a value that indicates how worthy of a target it would be
                for (int i = 0; i < count; i++)
                {
                    if ((temp = CalculateAttackValue(preys[i])) > easiestPrey)
                    {
                        easiestPrey = temp;
                        preyIndex = i;
                    }
                }

                count = fuelTanks.Count;
                int fuelTankIndex = 0;
                float easiestTank = 0;

                //Calculates the closest fuel tank and calculates a value that indicates how worthy of a target it would be
                for (int i = 0; i < count; i++)
                {
                    if ((temp = CalculateScavengeValue(fuelTanks[i])) > easiestTank)
                    {
                        easiestTank = temp;
                        fuelTankIndex = i;
                    }
                }

                float max = Mathf.Max(easiestTank, easiestPrey, biggestThreat);

                //Makes a decision based on the three values calculated
                if (max == easiestTank)
                {
                    targetPos = fuelTanks[fuelTankIndex].transform.position;
                }
                else if (max == easiestPrey)
                {
                    targetPos = preys[preyIndex].transform.position;
                }
                else
                {
                    targetPos = transform.position - 2.5f * threats[threatIndex].transform.position.normalized;
                }
            }

            desiredRot = Quaternion.LookRotation(targetPos - transform.position, Vector3.up);
            decisionTime = Time.time;
            return desiredRot;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            //Checks if the robot has fallen
            if (!other.tag.Equals("Drop Detector"))
                return;

            Destroy(this);
        }

        private float CalculateScavengeValue(FuelTank tank)
        {
            return scavengeConstant / GetDistanceFrom(tank.transform);
        }

        private float CalculateEscapeValue(RobotFighter robot)
        {
            return escapeConstant * CalculateHardness(robot) / GetDistanceFrom(robot.transform);
        }

        private float CalculateAttackValue(RobotFighter robot)
        {
            return attackConstant / (CalculateHardness(robot) * GetDistanceFrom(robot.transform));
        }

        private float CalculateHardness(RobotFighter robot)
        {
            return robot.rb.mass / rb.mass;
        }

        private float GetDistanceFrom(Transform transform)
        {
            return (this.transform.position - transform.position).magnitude;
        }

        private float GetDistanceFrom(Vector3 pos)
        {
            return (this.transform.position - pos).magnitude;
        }

        //This method subcribes to a method in GameManager and is called when the fuel tanks are updated
        internal void UpdateFuelTanks(List<FuelTank> fuelTanks)
        {
            this.fuelTanks = fuelTanks;
        }

        //This method subcribes to a method in GameManager and is called when the robots are updated
        internal void UpdateRobots(List<RobotFighter> robots)
        {
            preys = robots.Where(r => !r.Equals(this)).ToList();

            threats.Clear();

            RobotFighter temp;

            for (int i = 0; i < preys.Count; i++)
            {
                if ((temp = preys[i]).rb.mass > rb.mass)
                {
                    threats.Add(temp);
                    preys.RemoveAt(i);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            OnConsumed();
            OnEliminated();
        }
    }
}