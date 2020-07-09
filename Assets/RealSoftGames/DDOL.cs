//Author: Jake Aquilina
//Creation Date: 21/06/20 09:41 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealSoftGames
{
    public class DDOL : MonoBehaviour
    {
        public static DDOL Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(this);
        }
    }
}