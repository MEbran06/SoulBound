using System.Collections.Generic;
using UnityEngine;

namespace GhostStates
{
    // Update this whenever we create a new state
    public enum GhostStateID
    {
        Patrol,
        Chase
    }

    public static class CachedStates
    {
        // create an empty dictionary to store that cached state of a ghost
        public static Dictionary<GhostStateID, IGhostState> states = new Dictionary<GhostStateID, IGhostState>();
    }

}