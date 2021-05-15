using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rocketeer
{
    public static class Context
    {
        public static int patchIDCounter = 0;

        public static RocketeerMethodTracker[] trackers = new RocketeerMethodTracker[100];

        public static readonly Dictionary<string, RocketeerMethodTracker> trackerByUniqueIdentifier = new Dictionary<string, RocketeerMethodTracker>();
        public static readonly Dictionary<MethodBase, RocketeerMethodTracker> trackerByMethod = new Dictionary<MethodBase, RocketeerMethodTracker>();

        public static readonly HashSet<string> patchedMethods = new HashSet<string>();

        /* ----------------------------------------------------
         * DEBUGGIN:                  
         * This section contain special variables for debugging 
        */

        public static int __MARCO = 0; // Used to create a marko paulo ping
        public static int __NUKE = 0; // Used to create a nuke new calls
    }
}
