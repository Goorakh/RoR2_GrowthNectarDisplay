using BepInEx.Configuration;
using GrowthNectarDisplay.Utilities.Extensions;
using RoR2;
using RoR2.UI;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace GrowthNectarDisplay.BuffIconIndicator
{
    public class GrowthNectarBuffIndicatorController : MonoBehaviour
    {
        public static ConfigEntry<Color> IndicatorColorConfig { get; private set; }

        public static ConfigEntry<bool> RequireItemInInventoryConfig { get; private set; }

        public static ConfigEntry<bool> AlwaysShowOnScoreboardOpenConfig { get; private set; }

        public static void StaticInit(ConfigFile config)
        {
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/BuffIcon.prefab").CallOnSuccess(buffIcon =>
            {
                buffIcon.AddComponent<GrowthNectarBuffIndicatorController>();
            });

            On.RoR2.UI.BuffDisplay.GetNewBuffIcon += BuffDisplay_GetNewBuffIcon;

            IndicatorColorConfig = config.Bind(new ConfigDefinition("Indicator", "Indicator Color"), new Color(0.9692f, 0.7857f, 0.0969f), new ConfigDescription("The color of the indicator"));

            RequireItemInInventoryConfig = config.Bind(new ConfigDefinition("Indicator", "Require Item"), true, new ConfigDescription("If enabled, the Growth Nectar buff indicator will not show unless you have the Growth Nectar item"));

            AlwaysShowOnScoreboardOpenConfig = config.Bind(new ConfigDefinition("Indicator", "Always Visible in Scoreboard"), false, new ConfigDescription("If enabled, the Growth Nectar buff indicator will always be visible when opening the scoreboard (TAB)"));
        }

        static BuffIcon BuffDisplay_GetNewBuffIcon(On.RoR2.UI.BuffDisplay.orig_GetNewBuffIcon orig, BuffDisplay self)
        {
            BuffIcon buffIcon = orig(self);

            if (buffIcon.TryGetComponent(out GrowthNectarBuffIndicatorController indicatorController))
            {
                indicatorController.OwnerBuffDisplay = self;
            }

            return buffIcon;
        }

        [NonSerialized]
        public BuffDisplay OwnerBuffDisplay;

        GameObject _indicatorRoot;
        Image _indicatorImage;

        BuffIcon _buffIcon;

        CharacterBody _buffSource;

        void Awake()
        {
            _buffIcon = GetComponent<BuffIcon>();

            _indicatorRoot = new GameObject("GrowthNectarIndicator");
            RectTransform indicatorTransform = _indicatorRoot.AddComponent<RectTransform>();
            indicatorTransform.SetParent(transform);
            indicatorTransform.anchorMin = new Vector2(0f, 0f);
            indicatorTransform.anchorMax = new Vector2(0f, 0f);
            indicatorTransform.sizeDelta = new Vector2(12f, 12f);
            indicatorTransform.anchoredPosition = new Vector2(6f, 6f);

            _indicatorImage = _indicatorRoot.AddComponent<Image>();
            _indicatorImage.raycastTarget = false;
        }

        void OnEnable()
        {
            _buffSource = null;

            refreshIndicator();
        }

        void FixedUpdate()
        {
            refreshIndicator();
        }

        void refreshIndicator()
        {
            CharacterBody buffSource = null;
            if (OwnerBuffDisplay)
            {
                buffSource = OwnerBuffDisplay.source;
            }

            _buffSource = buffSource;

            bool shouldShow = shouldShowIndicator();

            if (_indicatorRoot)
            {
                _indicatorRoot.SetActive(shouldShow);
            }

            if (shouldShow)
            {
                if (_indicatorImage)
                {
                    Sprite indicatorSprite = Assets.GrowthNectarBuffIndicatorSprite;
                    Color indicatorColor = IndicatorColorConfig.Value;

                    _indicatorImage.sprite = indicatorSprite;
                    _indicatorImage.color = indicatorColor;
                }
            }
        }

        bool shouldShowIndicator()
        {
            BuffDef displayingBuff = _buffIcon ? _buffIcon.buffDef : null;
            if (!displayingBuff || displayingBuff.isHidden || displayingBuff.ignoreGrowthNectar)
                return false;

            if (!_buffSource || !_buffSource.isPlayerControlled)
                return false;

            bool forceShow = false;

            if (AlwaysShowOnScoreboardOpenConfig.Value)
            {
                foreach (HUD hud in HUD.readOnlyInstanceList)
                {
                    if (hud.scoreboardPanel && hud.scoreboardPanel.activeSelf)
                    {
                        forceShow = true;
                        break;
                    }
                }
            }

            if (!forceShow)
            {
                if (RequireItemInInventoryConfig.Value)
                {
                    Inventory buffSourceInventory = _buffSource.inventory;
                    if (!buffSourceInventory || buffSourceInventory.GetItemCount(DLC2Content.Items.BoostAllStats) <= 0)
                        return false;
                }
            }

            return true;
        }
    }
}
