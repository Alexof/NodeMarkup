﻿using ColossalFramework.UI;
using NodeMarkup.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NodeMarkup.UI
{
    public class Button : UIButton
    {
        const string CONTAINING_PANEL_NAME = "RoadsOptionPanel";
        const string nodeMarkupButtonBg = "NodeControllerButtonBg";
        const string nodeMarkupButtonBgActive = "NodeControllerButtonBgFocused";
        const string nodeMarkupButtonBgHovered = "NodeControllerButtonBgHovered";
        const string nodeMarkupIcon = "NodeControllerIcon";
        const string nodeMarkupIconActive = "NodeControllerIconPressed";
        const int buttonSize = 31;
        readonly static Vector2 buttonPosition = new Vector3(64, 38);
        public static string AtlasName = nameof(Button);
        public static Button Instace { get; private set; }

        static UIComponent GetContainingPanel()
        {
            var ret = UIUtils.Instance.FindComponent<UIComponent>(CONTAINING_PANEL_NAME, null, UIUtils.FindOptions.NameContains);
            return ret ?? throw new Exception($"Could not find {CONTAINING_PANEL_NAME}");
        }

        public override void Start()
        {
            Logger.LogDebug($"{nameof(Button)}.{nameof(Start)}");

            base.Start();
            name = nameof(Button);
            playAudioEvents = true;
            tooltip = "Node Markup";

            var builtinTabstrip = UIUtils.Instance.FindComponent<UITabstrip>("ToolMode", GetContainingPanel(), UIUtils.FindOptions.None);
            if (builtinTabstrip == null)
                return;

            string[] spriteNames = new string[]
            {
                nodeMarkupButtonBg,
                nodeMarkupButtonBgActive,
                nodeMarkupButtonBgHovered,
                nodeMarkupIcon,
                nodeMarkupIconActive
            };

            atlas = TextureUtil.GetAtlas(AtlasName);
            if (atlas == UIView.GetAView().defaultAtlas)
            {
                atlas = TextureUtil.CreateTextureAtlas("sprites.png", AtlasName, buttonSize, buttonSize, spriteNames);
            }

            Deactivate();
            hoveredBgSprite = nodeMarkupButtonBgHovered;

            relativePosition = buttonPosition;
            size = new Vector2(buttonSize, buttonSize);
            Show();
            Unfocus();
            Invalidate();
            Instace = this;
        }

        public void Activate()
        {
            Logger.LogDebug($"{nameof(Button)}.{nameof(Activate)}");

            focusedFgSprite = normalBgSprite = pressedBgSprite = disabledBgSprite = nodeMarkupButtonBgActive;
            normalFgSprite = focusedFgSprite = nodeMarkupIconActive;
            Invalidate();
        }
        public void Deactivate()
        {
            Logger.LogDebug($"{nameof(Button)}.{nameof(Deactivate)}");

            focusedFgSprite = normalBgSprite = pressedBgSprite = disabledBgSprite = nodeMarkupButtonBg;
            normalFgSprite = focusedFgSprite = nodeMarkupIcon;
            Invalidate();
        }

        public static Button CreateButton()
        {
            Logger.LogDebug($"{nameof(Button)}.{nameof(CreateButton)}");

            var button = GetContainingPanel().AddUIComponent<Button>();
            return button;
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            Logger.LogDebug($"{nameof(Button)}.{nameof(OnClick)}");

            base.OnClick(p);
            NodeMarkupTool.Instance.ToggleTool();
        }
    }
}