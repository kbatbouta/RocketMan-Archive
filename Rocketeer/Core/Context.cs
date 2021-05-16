using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rocketeer
{
    public static class Context
    {
        public static int patchIDCounter = 0;

        public static RocketeerPatchInfo[] trackers = new RocketeerPatchInfo[100];
        public static readonly HashSet<string> patchedMethods = new HashSet<string>();

        /* ----------------------------------------------------
         * DEBUGGIN:                  
         * This section contain special variables for debugging 
        */

        public static int __MARCO = 0; // Used to create a marko paulo ping
        public static int __NUKE = 0; // Used to create a nuke new calls
    }
}
