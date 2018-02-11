using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreQuickSlots
{
    public class Controller : MonoBehaviour
    {
        private static readonly string GAME_OBJECT_NAME = "MoreQuickSlots.Controller";

        public static void Load()
        {
            Unload();
            new GameObject(GAME_OBJECT_NAME).AddComponent<MoreQuickSlots.Controller>();
            Logger.Log("Controller Loaded");
        }

        private static void Unload()
        {
            GameObject gameObject = GameObject.Find(GAME_OBJECT_NAME);
            if (gameObject)
            {
                DestroyImmediate(gameObject);
                Logger.Log("Controller Unloaded");
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Logger.Log("Controller Awake");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Logger.Log("Controller Destroyed");
        }
        
        private void Update()
        {
            if (!uGUI_SceneLoading.IsLoadingScreenFinished || uGUI.main == null || uGUI.main.loading.IsLoading)
            {
                return;
            }

            for (int i = Player.quickSlotButtonsCount; i < Config.SlotCount; ++i)
            {
                KeyCode key = GetKeyCodeForSlot(i);
                if (Input.GetKeyDown(key))
                {
                    SelectQuickSlot(i);
                }
            }
        }

        private KeyCode GetKeyCodeForSlot(int slotID)
        {
            if (slotID == 9) return KeyCode.Alpha0;
            if (slotID == 10) return KeyCode.Minus;
            if (slotID == 11) return KeyCode.Equals;
            else return KeyCode.Alpha1 + slotID;
        }

        private void OnEnable()
        {
            Logger.Log("Controller enabled");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.Log("Scene Loaded: " + scene.name);
            if (scene.name == "Main")
            {
                gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            Logger.Log("Controller disabled");
        }
        
        private void SelectQuickSlot(int slotID)
        {
            string message = string.Format("Quick Slot Selected ({0})", slotID);
            Logger.Log(message);

            if (Inventory.main != null)
            {
                Inventory.main.quickSlots.Select(slotID);
            }
            else
            {
                Logger.Log("Inventory is null");
            }
        }
    }
}
