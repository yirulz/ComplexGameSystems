using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class Piece : MonoBehaviour
    {
        public bool isWhite, isKing;
        public Vector2Int cell, oldCell;

        private Animator anim;

        // Use this for initialization
        void Awake()
        {
            //Get reference to animator component
            anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void King()
        {
            //This peice is now king
            isKing = true;
            //Trigger King animation
            anim.SetTrigger("King");
        }
    }
}