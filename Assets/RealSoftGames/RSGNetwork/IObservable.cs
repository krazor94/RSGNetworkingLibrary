//Author: Jake Aquilina
//Creation Date: 07/07/20 10:14 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RealSoftGames.Network
{
    public interface IObservable
    {
        void OnSerializeView(RealSoftGames.Network.Stream stream);
    }
}