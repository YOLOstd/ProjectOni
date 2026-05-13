using PurrNet;
using UnityEngine;

namespace ProjectOni.Networking
{
    public static class NetworkedInteraction
    {
        /// <summary>
        /// Requests ownership of a networked object if the caller is the owner of the hitter.
        /// Useful for combat where the hitter wants immediate authority over the target.
        /// </summary>
        public static void RequestOwnershipOnHit(NetworkIdentity target, NetworkIdentity hitter)
        {
            if (target == null || hitter == null) return;
            if (!hitter.isOwner) return;
            
            if (target.owner != hitter.owner)
            {
                target.GiveOwnership(hitter.localPlayer.Value);
            }
        }
    }
}
