//Author: Jake Aquilina
//Creation Date: 07/07/20 10:13 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RealSoftGames.Network
{
    public class HashTable : MonoBehaviour
    {
        public static Dictionary<string, MethodInfo> HashSet = new Dictionary<string, MethodInfo>();
    }
}