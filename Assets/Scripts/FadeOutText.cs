using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace soulful
{
    public class FadeOutText : MonoBehaviour
    {
        [SerializeField]
        private float moveSpeed;

        [SerializeField]
        private float fadeTime;
        private float fadeTimer;


        void Start()
        {
            fadeTimer = fadeTime;
        }

        void Update()
        {
            transform.position += Vector3.up * moveSpeed;
            
            fadeTimer -= Time.deltaTime;
            if (fadeTimer <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
