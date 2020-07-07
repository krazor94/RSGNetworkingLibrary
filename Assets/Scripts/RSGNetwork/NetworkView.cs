//Author: Jake Aquilina
//Creation Date: 27/06/20 10:57 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System.Collections.Generic;
using UnityEngine;

namespace RealSoftGames.Network
{
    public class NetworkView : NetworkBehaviour
    {
        #region Fields

        public static List<NetworkView> Views = new List<NetworkView>();
        private int viewId;
        [SerializeField, SerializeReference] private MonoBehaviour[] behaviours;

        #endregion Fields

        #region Properties

        public int ViewID { get => viewId; set => viewId = value; }

        public void SetViewID(int id)
        {
            ViewID = id;
        }

        //public bool IsOwner { get => isOwner; set => isOwner = value; }
        public MonoBehaviour[] Behaviours { get => behaviours; }

        #endregion Properties

        #region Methods

        private void Awake()
        {
            if (!Views.Contains(this))
                Views.Add(this);

            foreach (var behaviour in behaviours)
            {
                //Debug.Log(behaviour.name);
                foreach (var method in behaviour.GetType().GetMethodInfo())
                {
                    //Debug.Log($"Found {method.Name} in {behaviour.name}");
                    if (!HashTable.HashSet.ContainsKey(method.Name))
                    {
                        //HashTable.HashSet.Add(method.Name, method.CreateDeleage(behaviour));
                        HashTable.HashSet.Add(method.Name, method);
                        Debug.Log($"Adding {method.Name} with {method.GetParameters().Length} parameters");
                    }
                    else
                        Debug.LogError($"HashTable already contains {method.Name}");
                }
            }
        }

        protected override void OnDestroy()
        {
            Views.Remove(this);
            base.OnDestroy();
        }

        #endregion Methods
    }
}