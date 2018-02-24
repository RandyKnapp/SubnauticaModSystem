using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AutosortLockers
{
	public class AutosortTypePicker : MonoBehaviour
	{
		[SerializeField]
		private AutosortTarget locker;
		[SerializeField]
		private VerticalLayoutGroup currentList;
		[SerializeField]
		private VerticalLayoutGroup availableList;
		[SerializeField]
		private Image background;

		public void Initialize(AutosortTarget locker)
		{
			this.locker = locker;
			background.sprite = ImageUtils.Load9SliceSprite(Mod.GetAssetPath("BindingBackground.png"), new RectOffset(20, 20, 20, 20));
		}

		public void OnCurrentListItemClick(TechType techType)
		{
			var allowedTypes = locker.GetTechTypes();
			allowedTypes.Remove(techType);
			locker.SetTechTypes(allowedTypes);
		}

		public void OnAvailableListItemClick(TechType techType)
		{
			var allowedTypes = locker.GetTechTypes();
			if (allowedTypes.Count >= AutosortTarget.MaxTypes)
			{
				return;
			}
			if (allowedTypes.Contains(techType))
			{
				return;
			}

			allowedTypes.Add(techType);
			locker.SetTechTypes(allowedTypes);
		}



		public static AutosortTypePicker Create(Transform parent)
		{
			var picker = LockerPrefabShared.CreateCanvas(parent).gameObject.AddComponent<AutosortTypePicker>();

			var t = picker.transform;
			t.localEulerAngles = new Vector3(0, 180, 0);
			t.localPosition = new Vector3(0, 0, 0.5f);

			picker.background = LockerPrefabShared.CreateBackground(picker.transform);
			RectTransformExtensions.SetSize(picker.background.rectTransform, 240, 200);
			picker.background.color = new Color(1, 1, 1);

			// Create current list

			// Create Available list

			// Create available list pagination

			return picker;
		}
	}
}
