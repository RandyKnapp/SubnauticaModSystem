using System;
using UnityEngine;
using UnityEngine.EventSystems;
#if SN
using UnityEngine.UI;
#elif BZ
using TMPro;
#endif

namespace AutosortLockers
{
	class LabelController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		private bool hover;

		public RectTransform rectTransform;
		public Action<string> onModified = delegate { };
#if SN
		public Text text;
#elif BZ
		public TextMeshProUGUI text;
#endif
		[SerializeField]
		private SaveDataEntry target;

		private void Awake()
		{
			rectTransform = transform as RectTransform;
		}

#if SN
		private void Initialize(SaveDataEntry data, Text textPrefab)
#elif BZ
		private void Initialize(SaveDataEntry data, TextMeshProUGUI textPrefab)
#endif
		{
			target = data;

			text = GameObject.Instantiate(textPrefab);
			text.fontSize = 12;
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
#if SN
				HandReticle.main.SetInteractTextRaw("Set Locker Label", "");
#elif BZ
				HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Set Locker Label");
#endif
			}
		}

		/*__________________________________________________________________________________________________________*/

		public static LabelController Create(SaveDataEntry data, Transform parent, GameObject lockerPrefab = null)
		{
#if SN
			lockerPrefab = Resources.Load<GameObject>("Submarine/Build/SmallLocker");
			var textPrefab = Instantiate(lockerPrefab.GetComponentInChildren<Text>());
#elif BZ
			var textPrefab = Instantiate(lockerPrefab.GetComponentInChildren<TextMeshProUGUI>());
#endif

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