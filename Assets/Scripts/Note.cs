using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace soulful
{
    //public class Timer
    //{
    //    private double seconds;
    //    private double ticking;

    //    public Timer(double seconds, double ticking)
    //    {

    //    }
    //}

    public enum Direction
    {
        UP = 0,
        DOWN,
        LEFT,
        RIGHT,
    }

    public class Note : MonoBehaviour, IBeatSynced
    {
        public Direction direction;

        private Vector2 spawnPos;
        private Vector2 targetPos;

        private float targetBeat;

        private float beatsToSpawnInAdvance;

        private float selfDestructTimer;
        private float selfDestructTime = 0.075f;
        private bool willDestruct = false;

        private void Start()
        {
            beatsToSpawnInAdvance = SoulfulSettings.BeatsToSpawnInAdvance;
        }

        public void Init(float targetBeat, Vector2 spawnPos, Direction direction, Color color)
        {
            this.direction = direction;
            this.targetBeat = targetBeat;
            this.spawnPos = spawnPos;
            this.targetPos = Vector2.zero; // for now, with 1 player game, target is the origin
            GetComponent<SpriteRenderer>().color = color;

            transform.position = spawnPos;
        }

        public void OnCurrentBeat(object sender, float currentBeat)
        {
            transform.position = Vector2.Lerp(spawnPos, Vector2.zero, (beatsToSpawnInAdvance - (targetBeat - currentBeat)) / beatsToSpawnInAdvance);
        }

        private void Update()
        {
            if (willDestruct)
            {
                selfDestructTimer -= Time.deltaTime;
                if (selfDestructTimer <= 0)
                {
                    gameObject.SetActive(false);
                }
            }

            if ((Vector2)transform.position == targetPos && !willDestruct)
            {
                Debug.Log("should be a miss");
                selfDestructTimer = selfDestructTime;
                willDestruct = true;
            }
        }
    }
}
