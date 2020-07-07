//Author: Jake Aquilina
//Creation Date: 15/06/20 12:38 AM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

//using NetworkView = RealSoftGames.Network.NetworkView;

namespace RealSoftGames
{
    public class NetworkPreprocessor : UnityEditor.AssetModificationProcessor
    {
        [UnityEditor.Callbacks.DidReloadScripts]
        public static void OnCompileCompleted()
        {
            if (EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                //GetAllRPC();
                var networkViews = GameObject.FindObjectsOfType<RealSoftGames.Network.NetworkView>();
                int viewID = 0;
                Debug.LogError($"OnCompileCompleted {networkViews}");
                foreach (var view in networkViews)
                {
                    Debug.LogError($"Assigning {view.name} ID: {viewID}");
                    view.ViewID = viewID;
                    viewID++;
                }
            }
        }
    }
}

#endif