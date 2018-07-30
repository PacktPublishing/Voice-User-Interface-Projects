using System;
using System.Linq;
using System.Collections.Generic;

namespace MyPodCast
{
    public class PackageFinder
    {
        private List<string> _allowedAppNames;

        public PackageFinder()
        {
            _allowedAppNames = new List<string>() {
                "com.google.android.projection.gearhead",
                "com.henry.mypodcast"
            };
        }

        public bool Find(string clientPackageName)
        {
            return _allowedAppNames.Contains(clientPackageName);
        }
    }
}