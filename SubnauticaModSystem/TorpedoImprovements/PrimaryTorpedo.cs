using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TorpedoImprovements
{
	class PrimaryTorpedo : MonoBehaviour
	{
		public List<TechType> Types { get; set; }
		public TechType PrimaryTorpedoType { get => Types[index]; }

		private int index;

		public void Next()
		{
			index = (index + 1) % Types.Count;

			var seamoth = GetComponent<SeaMoth>();
			List<TorpedoType> torpedoTypes = seamoth.torpedoTypes.ToList();
			torpedoTypes.Sort((a, b) => {
				return a.techType == b.techType ? 0 : (a.techType == PrimaryTorpedoType ? -1 : 1);
			});
			seamoth.torpedoTypes = torpedoTypes.ToArray();
		}
	}
}
