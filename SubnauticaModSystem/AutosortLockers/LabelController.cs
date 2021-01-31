using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutosortLockers
{
	class LabelController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		private bool hover;

		public RectTransform rectTransform;
		public Action<string> onModified = delegate { };
		public Text text;

		[SerializeField]
		private SaveDataEntry target;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

		private void Initialize(SaveDataEntry data, Text textPrefab)
		{
			target = data;

			text = GameObject.Instantiate(textPrefab);
			text.fontSize = 16;
			text.gameObject.name = "Text";
			text.rectTransform.SetParent(transform, false);
			RectTransformExtensions.SetSize(text.rectTransform, 140, 50);

			text.text = target.Label;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			uGUI.main.userInput.RequestString("Label", "Submit", target.Label, 25, new uGUI_UserInput.UserInputCallback(SetLabel));
		}

		public void SetLabel(string newLabel)
		{
			target.Label = newLabel;
			text.text = newLabel;

			onModified(newLabel);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			hover = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			hover = false;
		}

		private void Update()
		{
			if (hover)
			{
				HandReticle.main.SetIcon(HandReticle.IconType.Rename);
#if SUBNAUTICA
				HandReticle.main.SetInteractTextRaw("Set Locker Label", "");
#elif BELOWZERO
				HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Set Locker Label");
#endif
			}
		}



		///////////////////////////////////////////////////////////////////////////////////////////
		public static LabelController Create(SaveDataEntry data, Transform parent)
		{
			var lockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			var textPrefab = Instantiate(lockerPrefab.GetComponentInChildren<Text>());
			textPrefab.fontSize = 12;
			textPrefab.color = CustomizeScreen.ScreenContentColor;

			var habitatNameController = new GameObject("LabelController", typeof(RectTransform)).AddComponent<LabelController>();
			var rt = habitatNameController.gameObject.transform as RectTransform;
			RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
			habitatNameController.Initialize(data, textPrefab);

			return habitatNameController;
		}
	}
}
