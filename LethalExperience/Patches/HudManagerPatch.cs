using HarmonyLib;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;
using TMPro;
using System;
using System.Security.Cryptography;

namespace LethalExperience.Patches
{
    [HarmonyPatch]
    internal class HUDManagerPatch
    {
        private static GameObject _tempBar;
        private static TextMeshProUGUI _tempText;
        private static float _tempBarTime;

        private static TextMeshProUGUI _LevelText;
        private static float _LevelTextTime;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDManager), "DisplayNewScrapFound")]
        private static void GiveXPForScrap(HUDManager __instance)
        {
            // Get the scrap you just added
            var lootList = __instance.itemsToBeDisplayed;
            if (lootList == null)
                return;

            // Now we got the loot list that's about to be displayed, we add XP for each one that gets shown.
            int scrapCost = lootList[0].scrapValue;

            // Give XP for the amount of money this scrap costs.
            LethalExperience.LethalXP.AddXP(scrapCost);
        }

        public static void ShowXPUpdate(int oldXP, int newXP, int xp)
        {
            // Makes one if it doesn't exist on screen yet.
            if (!_tempBar)
                MakeBar();

            GameObject _tempprogress = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/XPUpdate/XPBarProgress");

            _tempprogress.GetComponent<Image>().fillAmount = newXP / (float)LethalExperience.LethalXP.GetXPRequirement();
            _tempText.text = newXP + " / " + (float)LethalExperience.LethalXP.GetXPRequirement();

            _tempBarTime = 2f;

            if (!_tempBar.activeSelf)
            {
                GameNetworkManager.Instance.StartCoroutine(XPBarCoroutine());
            }
        }

        private static IEnumerator XPBarCoroutine()
        {
            _tempBar.SetActive(true);
            while (_tempBarTime > 0f)
            {
                float time = _tempBarTime;
                _tempBarTime = 0f;
                yield return new WaitForSeconds(time);
            }
            _tempBar.SetActive(false);
        }

        public static void ShowLevelUp()
        {
            if (!_LevelText)
                MakeLevelUp();

            _LevelTextTime = 2f;

            if (!_LevelText.gameObject.activeSelf)
                GameNetworkManager.Instance.StartCoroutine(LevelUpCoroutine());
        }

        public static void MakeLevelUp()
        {
            // Make new text entirely. 
            _LevelText = new GameObject().AddComponent<TextMeshProUGUI>();

            _LevelText.name = "LevelUp";

            GameObject _igHud = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle");
            _LevelText.transform.SetParent(_igHud.transform, false);

            _LevelText.transform.Translate(1.5f, 0.4f, 0f);

            _LevelText.GetComponent<TextMeshProUGUI>().text = "Level Up!";
            // Color black
            _LevelText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 1f, 1f);

            // Search for font!
            TMP_FontAsset font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(t => t.name == "3270-HUDIngame");

            // Set font
            _LevelText.GetComponent<TextMeshProUGUI>().font = font;

            // Set font size
            _LevelText.GetComponent<TextMeshProUGUI>().fontSize = 50;

            _LevelText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Make this not active
            _LevelText.gameObject.SetActive(false);
        }

        private static IEnumerator LevelUpCoroutine()
        {
            _LevelText.gameObject.SetActive(true);
            while (_LevelTextTime > 0f)
            {
                float time = _LevelTextTime;
                _LevelTextTime = 0f;
                yield return new WaitForSeconds(time);
            }
            _LevelText.gameObject.SetActive(false);
        }

        private static void MakeBar()
        {
            GameObject _xpBar = GameObject.Find("/Systems/UI/Canvas/QuickMenu/XPBar");
            QuickMenuManagerPatch.MakeNewXPBar();
            _xpBar = GameObject.Find("/Systems/UI/Canvas/QuickMenu/XPBar");
            _tempBar = GameObject.Instantiate(_xpBar);
            _tempBar.name = "XPUpdate";

            _tempText = _tempBar.GetComponentInChildren<TextMeshProUGUI>();

            GameObject _igHud = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle");
            _tempBar.transform.SetParent(_igHud.transform, false);
            _tempBar.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            GameObject _xpBarLevel = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/XPUpdate/XPLevel");
            GameObject.Destroy(_xpBarLevel);

            GameObject _xpBarProfit = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/XPUpdate/XPProfit");
            GameObject.Destroy(_xpBarProfit);

            _tempBar.transform.Translate(3.1f, -2.1f, 0f);
            Vector3 localPos = _tempBar.transform.localPosition;

            _tempBar.transform.localPosition = new Vector3(localPos.x, localPos.y + 5f, localPos.z);

            _tempBar.SetActive(false);
        }
    }

}
