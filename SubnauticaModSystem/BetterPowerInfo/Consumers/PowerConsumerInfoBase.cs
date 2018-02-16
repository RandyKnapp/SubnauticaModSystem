using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterPowerInfo.Consumers
{
	public class PowerConsumerInfoBase
	{
		public virtual float PowerConsumedPerMinute { get { return GetPowerConsumedPerMinute(); } }
		public virtual string DisplayText { get { return GetDisplayText(); } }

		private string name;
		private float consumption;

		public PowerConsumerInfoBase(string name, float consumption)
		{
			this.name = name;
			this.consumption = consumption;
		}

		protected virtual float GetPowerConsumedPerMinute()
		{
			return consumption;
		}

		protected virtual string GetDisplayText()
		{
			string s = name.Replace("(Clone)", "");
			s = System.Text.RegularExpressions.Regex.Replace(s, "[A-Z]", " $0").Trim();
			return s;
		}
	}
}
