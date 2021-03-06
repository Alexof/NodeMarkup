﻿using ColossalFramework.UI;
using NodeMarkup.Manager;
using NodeMarkup.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NodeMarkup.UI.Editors
{
    public abstract class HeaderPanel : EditorItem, IReusable
    {
        public event Action OnDelete;
        protected override float DefaultHeight => HeaderButton.Size + 10;

        protected HeaderContent Content { get; set; }
        protected UIButton DeleteButton { get; set; }

        public HeaderPanel()
        {
            AddDeleteButton();
            AddContent();
        }

        public virtual void Init(float? height = null, bool isDeletable = true)
        {
            base.Init(height);
            DeleteButton.isVisible = isDeletable;
            SetSize();
        }
        public override void DeInit()
        {
            base.DeInit();
            OnDelete = null;
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetSize();
        }
        private void SetSize()
        {
            Content.size = new Vector2(DeleteButton.isVisible ? width - DeleteButton.width - 10 : width, height);
            DeleteButton.relativePosition = new Vector2(width - DeleteButton.width - 5, (height - DeleteButton.height) / 2);
        }

        private void AddContent()
        {
            Content = AddUIComponent<HeaderContent>();
            Content.relativePosition = new Vector2(0, 0);
        }

        private void AddDeleteButton()
        {
            DeleteButton = AddUIComponent<UIButton>();
            DeleteButton.atlas = TextureUtil.Atlas;
            DeleteButton.normalBgSprite = TextureUtil.DeleteNormal;
            DeleteButton.hoveredBgSprite = TextureUtil.DeleteHover;
            DeleteButton.pressedBgSprite = TextureUtil.DeletePressed;
            DeleteButton.size = new Vector2(20, 20);
            DeleteButton.eventClick += DeleteClick;
        }
        private void DeleteClick(UIComponent component, UIMouseEventParameter eventParam) => OnDelete?.Invoke();

    }
    public class HeaderContent : UIPanel
    {
        public HeaderContent()
        {
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutPadding = new RectOffset(0, Math.Max(5 - 2 * HeaderButton.IconPadding, 0), 0, 0);
        }
        public SimpleHeaderButton AddButton(string sprite, string text, bool showText = false, MouseEventHandler onClick = null)
            => AddButton<SimpleHeaderButton>(this, sprite, text, showText, onClick);
        public SimpleHeaderButton AddButton(UIComponent parent, string sprite, string text, bool showText = false, MouseEventHandler onClick = null)
            => AddButton<SimpleHeaderButton>(parent, sprite, text, showText, onClick);

        public ButtonType AddButton<ButtonType>(string sprite, string text, bool showText = false, MouseEventHandler onClick = null) where ButtonType : HeaderButton
            => AddButton<ButtonType>(this, sprite, text, showText, onClick);
        public ButtonType AddButton<ButtonType>(UIComponent parent, string sprite, string text, bool showText = false, MouseEventHandler onClick = null)
            where ButtonType : HeaderButton
        {
            var button = parent.AddUIComponent<ButtonType>();
            if(showText)
                button.text = text ?? string.Empty;
            else
                button.tooltip = text;
            button.SetIconSprite(sprite);

            if (onClick != null)
                button.eventClick += onClick;
            return button;
        }

        protected override void OnComponentAdded(UIComponent child)
        {
            base.OnComponentAdded(child);
            child.eventVisibilityChanged += ChildVisibilityChanged;
            child.eventSizeChanged += ChildSizeChanged;
        }
        protected override void OnComponentRemoved(UIComponent child)
        {
            base.OnComponentRemoved(child);
            child.eventVisibilityChanged -= ChildVisibilityChanged;
            child.eventSizeChanged -= ChildSizeChanged;
        }

        private void ChildVisibilityChanged(UIComponent component, bool value) => PlaceChildren();
        private void ChildSizeChanged(UIComponent component, Vector2 value) => PlaceChildren();

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            PlaceChildren();
        }

        public void PlaceChildren()
        {
            autoLayout = true;
            autoLayout = false;

            foreach (var item in components)
                item.relativePosition = new Vector2(item.relativePosition.x, (height - item.height) / 2);
        }
    }

    public class StyleHeaderPanel : HeaderPanel
    {
        public event Action OnSaveTemplate;
        public event Action OnCopy;
        public event Action OnPaste;

        ApplyTemplateHeaderButton ApplyTemplate { get; }

        public StyleHeaderPanel()
        {
            Content.AddButton(TextureUtil.AddTemplate, NodeMarkup.Localize.HeaderPanel_SaveAsTemplate, onClick: SaveTemplateClick);
            ApplyTemplate = Content.AddButton<ApplyTemplateHeaderButton>(TextureUtil.ApplyTemplate, NodeMarkup.Localize.HeaderPanel_ApplyTemplate);
            Content.AddButton(TextureUtil.Copy, NodeMarkup.Localize.HeaderPanel_StyleCopy, onClick: CopyClick);
            Content.AddButton(TextureUtil.Paste, NodeMarkup.Localize.HeaderPanel_StylePaste, onClick: PasteClick);
        }

        public void Init(Style.StyleType styleGroup, Action<StyleTemplate> onSelectTemplate, bool isDeletable = true)
        {
            base.Init(isDeletable: isDeletable);
            ApplyTemplate.Init(styleGroup, onSelectTemplate);
        }
        public override void DeInit()
        {
            base.DeInit();

            OnSaveTemplate = null;
            OnCopy = null;
            OnPaste = null;

            ApplyTemplate.DeInit();
        }
        private void SaveTemplateClick(UIComponent component, UIMouseEventParameter eventParam) => OnSaveTemplate?.Invoke();
        private void CopyClick(UIComponent component, UIMouseEventParameter eventParam) => OnCopy?.Invoke();
        private void PasteClick(UIComponent component, UIMouseEventParameter eventParam) => OnPaste?.Invoke();
    }
    public class CrosswalkHeaderPanel : StyleHeaderPanel
    {
        public event Action OnCut;

        public CrosswalkHeaderPanel()
        {
            Content.AddButton(TextureUtil.Cut, NodeMarkup.Localize.HeaderPanel_CutLinesByCrosswalk, onClick: CutClick);
        }
        public override void DeInit()
        {
            base.DeInit();

            OnCut = null;
        }

        private void CutClick(UIComponent component, UIMouseEventParameter eventParam) => OnCut?.Invoke();
    }

    public abstract class TemplateHeaderPanel<TemplateType> : HeaderPanel
        where TemplateType : Template
    {
        public event Action OnSaveAsset;
        public event Action OnEdit;
        public event Action OnSave;
        public event Action OnNotSave;
        HeaderButton SaveAsAsset { get; set; }
        HeaderButton Edit { get; set; }
        HeaderButton Save { get; set; }
        HeaderButton NotSave { get; set; }

        private bool IsAsset { get; set; }
        private bool CanEdit { get; set; }

        public virtual bool EditMode
        {
            set
            {
                SaveAsAsset.isVisible = !IsAsset && !value;
                Edit.isVisible = (!IsAsset || CanEdit) && !value;
                Save.isVisible = NotSave.isVisible = value;
            }
        }

        public TemplateHeaderPanel() => AddButtons();
        protected virtual void AddButtons()
        {
            Edit = Content.AddButton(TextureUtil.Edit, NodeMarkup.Localize.HeaderPanel_Edit, onClick: EditClick);
            SaveAsAsset = Content.AddButton(TextureUtil.Package, NodeMarkup.Localize.HeaderPanel_SaveAsAsset, onClick: SaveAssetClick);
            Save = Content.AddButton(TextureUtil.Save, NodeMarkup.Localize.HeaderPanel_Save, onClick: SaveClick);
            NotSave = Content.AddButton(TextureUtil.NotSave, NodeMarkup.Localize.HeaderPanel_NotSave, onClick: NotSaveClick);
        }

        public virtual void Init(TemplateType template)
        {
            base.Init(isDeletable: false);

            IsAsset = template.IsAsset;
            CanEdit = !IsAsset || template.Asset.CanEdit;

            EditMode = false;
        }
        public override void DeInit()
        {
            base.DeInit();
            OnSaveAsset = null;
            OnEdit = null;
            OnSave = null;
            OnNotSave = null;
        }
        private void SaveAssetClick(UIComponent component, UIMouseEventParameter eventParam) => OnSaveAsset?.Invoke();
        private void EditClick(UIComponent component, UIMouseEventParameter eventParam) => OnEdit?.Invoke();
        private void SaveClick(UIComponent component, UIMouseEventParameter eventParam) => OnSave?.Invoke();
        private void NotSaveClick(UIComponent component, UIMouseEventParameter eventParam) => OnNotSave?.Invoke();
    }

    public class StyleTemplateHeaderPanel : TemplateHeaderPanel<StyleTemplate>
    {
        public event Action OnSetAsDefault;
        public event Action OnDuplicate;

        HeaderButton SetAsDefaultButton { get; set; }
        HeaderButton Duplicate { get; set; }

        public override bool EditMode 
        {
            set
            {
                base.EditMode = value;
                SetAsDefaultButton.isVisible = Duplicate.isVisible = !value;
            }
        }

        protected override void AddButtons()
        {
            SetAsDefaultButton = Content.AddButton(string.Empty, null, onClick: SetAsDefaultClick);
            Duplicate = Content.AddButton(TextureUtil.Duplicate, NodeMarkup.Localize.HeaderPanel_Duplicate, onClick: DuplicateClick);

            base.AddButtons();
        }
        public override void Init(StyleTemplate template)
        {
            base.Init(template);

            SetAsDefaultButton.SetIconSprite(template.IsDefault ? TextureUtil.UnsetDefault : TextureUtil.SetDefault);
            SetAsDefaultButton.tooltip = template.IsDefault ? NodeMarkup.Localize.HeaderPanel_UnsetAsDefault : NodeMarkup.Localize.HeaderPanel_SetAsDefault;
        }
        public override void DeInit()
        {
            base.DeInit();

            OnSetAsDefault = null;
            OnDuplicate = null;
        }

        private void SetAsDefaultClick(UIComponent component, UIMouseEventParameter eventParam) => OnSetAsDefault?.Invoke();
        private void DuplicateClick(UIComponent component, UIMouseEventParameter eventParam) => OnDuplicate?.Invoke();
    }
    public class IntersectionTemplateHeaderPanel : TemplateHeaderPanel<IntersectionTemplate> 
    {
        public event Action OnApply;

        HeaderButton Apply { get; set; }
        public override bool EditMode
        {
            set
            {
                base.EditMode = value;
                Apply.isVisible = !value;
            }
        }

        protected override void AddButtons()
        {
            Apply = Content.AddButton(TextureUtil.Apply, NodeMarkup.Localize.PresetEditor_ApplyPreset, onClick: ApplyClick);
            base.AddButtons();
        }
        public override void DeInit()
        {
            base.DeInit();
            OnApply = null;
        }
        private void ApplyClick(UIComponent component, UIMouseEventParameter eventParam) => OnApply?.Invoke();
    }
}
