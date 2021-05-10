using System;
using System.Collections.Generic;

namespace Rocketeer
{
    public static class Context
    {
        public static int patchIDCounter = 0;

        public static RocketeerPatchTracker[] patches = new RocketeerPatchTracker[100];

        public static readonly Dictionary<string, RocketeerPatchTracker> patchByUniqueIdentifier = new Dictionary<string, RocketeerPatchTracker>();

        public static readonly HashSet<string> patchedMethods = new HashSet<string>();

        /* ----------------------------------------------------
         * DEBUGGIN:                  
         * This section contain special variables for debugging 
        */

        public static int __MARCO = 0; // Used to create a marko paulo ping
        public static int __NUKE = 0; // Used to create a nuke new calls
    }
}
