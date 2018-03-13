using Common.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CustomPings
{
	class PingEntryController : MonoBehaviour
	{
		private uGUI_Icon icon;
		private Text label;
		private PingColorButton colorButton;
		private PingColorButton iconButton;
		private PingInstance ping;
		private BeaconColorPicker colorPicker;
		private BeaconIconPicker iconPicker;

		private void Initialize()
		{
			if (icon == null)
			{
				var entry = GetComponent<uGUI_PingEntry>();
				icon = entry.icon;
				label = entry.label;
			}

			if (colorButton == null)
			{
				colorButton = PingColorButton.Create(transform, Color.white);
				colorButton.transform.localPosition = new Vector3(500, 0, 0);
				colorButton.onClick += OnColorButtonClick;
			}

			if (colorPicker == null)
			{
				colorPicker = BeaconColorPicker.Create(transform);
				colorPicker.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
				colorPicker.gameObject.SetActive(false);
				colorPicker.rectTransform.anchoredPosition = new Vector2(200, 0);
			}

			if (iconButton == null)
			{
				iconButton = PingColorButton.Create(transform, Color.clear);
				iconButton.transform.localPosition = new Vector3(-302, 0, 0);
				iconButton.onClick += OnIconButtonClick;
			}

			if (iconPicker == null)
			{
				iconPicker = BeaconIconPicker.Create(transform);
				iconPicker.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
				iconPicker.gameObject.SetActive(false);
				iconPicker.rectTransform.anchoredPosition = new Vector2(0, 0);
			}
		}

		public void OnInitialize(int id, PingType type, int colorIndex)
		{
			ping = PingManager.Get(id);
			Initialize();
		}

		private void OnEnable()
		{
			colorButton.Initialize(ping != null ? PingManager.colorOptions[ping.colorIndex] : Color.white);
			colorPicker.Initialize(ping);
			iconButton.Initialize(Color.clear);
			iconPicker.Initialize(ping);
		}

		private void OnDisable()
		{
			colorPicker.Close();
			iconPicker.Close();
		}

		private void Update()
		{
			if (ping != null && colorButton != null)
			{
				colorButton.image.color = PingManager.colorOptions[ping.colorIndex];
			}

			label.gameObject.SetActive(!iconPicker.gameObject.activeSelf);
		}

		private void ClosePickers()
		{
			colorPicker.Close();
		}

		private void CloseAllOtherPickers()
		{
			var otherControllers = GameObject.FindObjectsOfType<PingEntryController>();
			foreach (var c in otherControllers)
			{
				if (c != this)
				{
					c.ClosePickers();
				}
			}
		}

		private void OnColorButtonClick()
		{
			CloseAllOtherPickers();
			colorPicker.Toggle();
		}

		private void OnIconButtonClick()
		{
			CloseAllOtherPickers();
			iconPicker.Toggle();
		}
	}
}
