using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterScannerBlips
{
    internal class BlipIdentifier : MonoBehaviour
    {
        // A component added to a GUI scanner blip that stores information about the actual TechType of a fragment, allowing the code to identify exactly what type of Fragment the object is

        internal string uniqueId;
        internal TechType actualTechType { get
            {
                return ResourceTrackerPatches.GetTechTypeForId(this.uniqueId);
            }
        }
    }
}
