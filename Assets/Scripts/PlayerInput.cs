using System.Collections.Generic;
using UnityEngine;

namespace soulful
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField]
        private GameObject arrow;
        [SerializeField]
        private Sprite upSprite, downSprite, leftSprite, rightSprite;
        private GameObject activeUpArrow, activeDownArrow, activeLeftArrow, activeRightArrow;
    
        // this is a demo- this functionality should go in a pseudo UI Manager, event or called directly
        // todo: wrap input with enum for relevant keys for the game

        public static bool HitUp()
        {
            return Input.GetButtonDown("Up");
        }
        public static bool HitDown()
        {
            return Input.GetButtonDown("Down");
        }
        public static bool HitLeft()
        {
            return Input.GetButtonDown("Left");
        }
        public static bool HitRight()
        {
            return Input.GetButtonDown("Right");
        }

        public void Update()
        {
            List<int> inputDirections = new List<int>();

            if (Input.GetButtonDown("Up"))
            {
                inputDirections.Add(0);

                if (activeUpArrow)
                {
                    Destroy(activeUpArrow);
                }
                arrow.GetComponent<SpriteRenderer>().sprite = upSprite;
                activeUpArrow = Instantiate(arrow, Vector3.up * 3, Quaternion.identity);
            }
            if (Input.GetButtonDown("Down"))
            {
                inputDirections.Add(1);

                if (activeDownArrow)
                {
                    Destroy(activeDownArrow);
                }
                arrow.GetComponent<SpriteRenderer>().sprite = downSprite;
                activeDownArrow = Instantiate(arrow, Vector3.down * 3, Quaternion.identity);
            }
            if (Input.GetButtonDown("Left"))
            {
                inputDirections.Add(2);

                if (activeLeftArrow)
                {
                    Destroy(activeLeftArrow);
                }
                arrow.GetComponent<SpriteRenderer>().sprite = leftSprite;
                activeLeftArrow = Instantiate(arrow, Vector3.left * 3, Quaternion.identity);
            }
            if (Input.GetButtonDown("Right"))
            {
                inputDirections.Add(3);

                if (activeRightArrow)
                {
                    Destroy(activeRightArrow);
                }
                arrow.GetComponent<SpriteRenderer>().sprite = rightSprite;
                activeRightArrow = Instantiate(arrow, Vector3.right * 3, Quaternion.identity);
            }
        }
    }
}