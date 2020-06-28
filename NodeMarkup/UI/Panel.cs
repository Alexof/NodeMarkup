﻿using ColossalFramework.UI;
using NodeMarkup.Manager;
using NodeMarkup.UI.Editors;
using NodeMarkup.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NodeMarkup.UI
{
    public class NodeMarkupPanel : UIPanel
    {
        public static NodeMarkupPanel Instance { get; private set; }

        public Markup Markup { get; private set; }

        private UIDragHandle Handle { get; set; }
        private UILabel Caption { get; set; }
        private CustomUITabstrip TabStrip { get; set; }
        public List<Editor> Editors { get; } = new List<Editor>();
        public Editor CurrentEditor { get; set; }

        private Vector2 EditorSize => new Vector2(500, 400);
        private Vector2 EditorPosition => new Vector2(0, TabStrip.relativePosition.y + TabStrip.height);

        private static readonly string kTabstripButton = "RoadEditorTabstripButton";
        private static float TabStripHeight => 20;

        public static NodeMarkupPanel CreatePanel()
        {
            var uiView = UIView.GetAView();
            Instance = uiView.AddUIComponent(typeof(NodeMarkupPanel)) as NodeMarkupPanel;
            Instance.Init();
            return Instance;
        }
        public void Init()
        {
            atlas = TextureUtil.GetAtlas("Ingame");
            backgroundSprite = "MenuPanel2";
            absolutePosition = new Vector3(200, 200);
            name = "NodeMarkupPanel";

            CreateHandle();
            CreateTabStrip();
            CreateEditors();

            size = new Vector2(500, Handle.height + TabStrip.height + EditorSize.y);
        }
        private void CreateHandle()
        {
            Handle = AddUIComponent<UIDragHandle>();
            Handle.size = new Vector2(500, 42);
            Handle.relativePosition = new Vector2(0, 0);
            Handle.target = parent;
            Handle.eventSizeChanged += ((component, size) =>
            {
                Caption.size = size;
                Caption.CenterToParent();
            });

            Caption = Handle.AddUIComponent<UILabel>();
            Caption.text = nameof(NodeMarkupPanel);
            Caption.textAlignment = UIHorizontalAlignment.Center;
            Caption.anchor = UIAnchorStyle.Top;

            Caption.eventTextChanged += ((component, text) => Caption.CenterToParent());
        }

        private void CreateTabStrip()
        {
            TabStrip = AddUIComponent<CustomUITabstrip>();
            TabStrip.relativePosition = new Vector3(0, Handle.height);
            TabStrip.eventSelectedIndexChanged += TabStripSelectedIndexChanged;
            TabStrip.selectedIndex = -1;
        }

        private void CreateEditors()
        {
            CreateEditor<PointsEditor>();
            CreateEditor<LinesEditor>();
        }
        private void CreateEditor<EditorType>() where EditorType : Editor
        {
            var editor = AddUIComponent<EditorType>();
            editor.Init();
            TabStrip.AddTab<PointsEditor>(editor.Name);

            editor.isVisible = false;
            editor.size = EditorSize;
            editor.relativePosition = EditorPosition;
            editor.NodeMarkupPanel = this;

            Editors.Add(editor);
        }

        public void SetNode(ushort nodeId)
        {
            Show();
            Caption.text = $"Edit node #{nodeId} markup";

            Markup = Manager.Manager.Get(nodeId);
            foreach (var editor in Editors)
            {
                editor.UpdateEditor();
            }

            TabStrip.selectedIndex = 0;
        }
        private int GetEditor(Type editorType) => Editors.FindIndex((e) => e.GetType() == editorType);
        private void TabStripSelectedIndexChanged(UIComponent component, int index) => CurrentEditor = SelectEditor(index);
        private Editor SelectEditor(int index)
        {
            if (index >= 0 && Editors.Count > index)
            {
                foreach (var editor in Editors)
                {
                    editor.isVisible = false;
                }

                Editors[index].isVisible = true;
                return Editors[index];
            }
            else
                return null;
        }
        private EditorType SelectEditor<EditorType>() where EditorType : Editor
        {
            var editorIndex = GetEditor(typeof(EditorType));
            TabStrip.selectedIndex = editorIndex;
            return Editors[editorIndex] as EditorType;
        }

        public void EditPoint(MarkupPoint point)
        {
            var editor = SelectEditor<PointsEditor>();
            editor?.Select(point);
        }
        public void EditLine(MarkupLine line)
        {
            var editor = SelectEditor<LinesEditor>();
            editor?.UpdateEditor();
            editor?.Select(line);
        }
        public void Render(RenderManager.CameraInfo cameraInfo) => CurrentEditor?.Render(cameraInfo);
    }
}