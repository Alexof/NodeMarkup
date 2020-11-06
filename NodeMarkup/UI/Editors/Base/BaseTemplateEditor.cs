﻿using ColossalFramework.UI;
using NodeMarkup.Manager;
using NodeMarkup.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeMarkup.UI.Editors
{
    public interface ITemplateEditor
    {
        void Cancel();
    }
    public abstract class BaseTemplateEditor<Item, TemplateType, Icon, Group, GroupType, HeaderPanelType> : GroupedEditor<Item, TemplateType, Icon, Group, GroupType>, ITemplateEditor
        where Item : EditableItem<TemplateType, Icon>
        where Icon : UIComponent
        where TemplateType : Template<TemplateType>
        where Group : EditableGroup<GroupType, Item, TemplateType, Icon>
        where HeaderPanelType : TemplateHeaderPanel<TemplateType>
    {
        protected override bool UseGroupPanel => true;

        protected bool EditMode { get; private set; }
        protected bool HasChanges { get; set; }

        protected StringPropertyPanel NameProperty { get; set; }
        protected HeaderPanelType HeaderPanel { get; set; }
        protected abstract string RewriteCaption { get; }
        protected abstract string RewriteMessage { get; }
        protected abstract string SaveChangesMessage { get; }
        protected abstract string NameExistMessage { get; }

        private EditorItem[] Aditional { get; set; }

        public override bool Active
        {
            set
            {
                base.Active = value;
                EditMode = false;
                if (SelectItem is Item)
                    SetEditable();
            }
        }
        private EditTemplateMode ToolMode { get; }

        public BaseTemplateEditor()
        {
            ToolMode = Tool.CreateToolMode<EditTemplateMode>();
            ToolMode.Init(this);
        }

        protected abstract IEnumerable<TemplateType> GetTemplates();
        protected override void FillItems()
        {
            foreach (var templates in GetTemplates())
                AddItem(templates);
        }
        protected override void OnObjectSelect()
        {
            AddHeader();
            AddAuthor();
            AddTemplateName();

            ReloadAdditionalProperties();

            AddAdditional();

            SetEditable();
        }
        private void ReloadAdditionalProperties()
        {
            if (Aditional != null)
            {
                foreach (var aditional in Aditional)
                    ComponentPool.Free(aditional);
            }

            Aditional = AddAditionalProperties().ToArray();
        }
        protected virtual void AddAdditional() { }
        protected override void OnClear()
        {
            NameProperty = null;
            HeaderPanel = null;
            Aditional = null;
        }
        protected virtual IEnumerable<EditorItem> AddAditionalProperties() { yield break; }

        protected override void OnObjectDelete(TemplateType template) => (template.Manager as TemplateManager<TemplateType>).DeleteTemplate(template);

        protected virtual void AddHeader()
        {
            HeaderPanel = ComponentPool.Get<HeaderPanelType>(PropertiesPanel);
            HeaderPanel.Init(EditObject);
            HeaderPanel.OnSaveAsset += SaveAsset;
            HeaderPanel.OnEdit += StartEditTemplate;
            HeaderPanel.OnSave += SaveChanges;
            HeaderPanel.OnNotSave += NotSaveChanges;
        }

        private void AddAuthor()
        {
            if (EditObject.IsAsset)
            {
                var authorProperty = ComponentPool.Get<StringPropertyPanel>(PropertiesPanel);
                authorProperty.Text = NodeMarkup.Localize.TemplateEditor_Author;
                authorProperty.FieldWidth = 230;
                authorProperty.EnableControl = false;
                authorProperty.Init();
                authorProperty.Value = EditObject.Asset.Author;
            }
        }
        private void AddTemplateName()
        {
            NameProperty = ComponentPool.Get<StringPropertyPanel>(PropertiesPanel);
            NameProperty.Text = NodeMarkup.Localize.TemplateEditor_Name;
            NameProperty.FieldWidth = 230;
            NameProperty.SubmitOnFocusLost = true;
            NameProperty.Init();
            NameProperty.Value = EditObject.Name;
            NameProperty.OnValueChanged += (name) => OnChanged();
        }

        protected virtual void SetEditable()
        {
            Panel.Available = AvailableItems = !EditMode;
            HeaderPanel.EditMode = NameProperty.EnableControl = EditMode;

            foreach (var aditional in Aditional)
                aditional.EnableControl = EditMode;
        }

        private void SaveAsset()
        {
            if (TemplateManager<TemplateType>.Instance.MakeAsset(EditObject))
            {
                SelectItem.Init(EditObject);
                ItemClick(SelectItem);
            }
        }

        protected virtual void StartEditTemplate()
        {
            EditMode = true;
            HasChanges = false;
            SetEditable();
            Tool.SetMode(ToolMode);
        }
        protected void OnChanged() => HasChanges = true;
        protected virtual void EndEditTemplate()
        {
            EditMode = false;
            HasChanges = false;
            SetEditable();
            Tool.SetDefaultMode();
        }

        private void SaveChanges()
        {
            var name = NameProperty.Value;
            var messageBox = default(YesNoMessageBox);
            if (!string.IsNullOrEmpty(name) && name != EditObject.Name && (EditObject.Manager as TemplateManager<TemplateType>).ContainsName(name, EditObject))
            {
                messageBox = MessageBoxBase.ShowModal<YesNoMessageBox>();
                messageBox.CaprionText = NodeMarkup.Localize.TemplateEditor_NameExistCaption;
                messageBox.MessageText = string.Format(NameExistMessage, name);
                messageBox.OnButton1Click = AgreeExistName;
                messageBox.OnButton2Click = EditName;
            }
            else
                AgreeExistName();


            bool AgreeExistName()
            {
                if (EditObject.IsAsset)
                {
                    messageBox ??= MessageBoxBase.ShowModal<YesNoMessageBox>();
                    messageBox.CaprionText = RewriteCaption;
                    messageBox.MessageText = RewriteMessage;
                    messageBox.OnButton1Click = Save;
                    return false;
                }
                else
                    return Save();
            }

            bool EditName()
            {
                NameProperty.Edit();
                return true;
            }

            bool Save()
            {
                OnApplyChanges();
                (EditObject.Manager as TemplateManager<TemplateType>).TemplateChanged(EditObject);
                EndEditTemplate();
                SelectItem.Refresh();
                return true;
            }
        }
        protected virtual void OnApplyChanges() => EditObject.Name = NameProperty.Value;

        private void NotSaveChanges()
        {
            OnNotApplyChanges();
            ReloadAdditionalProperties();
            EndEditTemplate();
        }
        protected virtual void OnNotApplyChanges() => NameProperty.Value = EditObject.Name;

        public void Cancel()
        {
            if (HasChanges)
            {
                var messageBox = MessageBoxBase.ShowModal<ThreeButtonMessageBox>();
                messageBox.CaprionText = NodeMarkup.Localize.TemplateEditor_SaveChanges;
                messageBox.MessageText = SaveChangesMessage;
                messageBox.Button1Text = MessageBoxBase.Yes;
                messageBox.Button2Text = MessageBoxBase.No;
                messageBox.Button3Text = MessageBoxBase.Cancel;
                messageBox.OnButton1Click = OnSave;
                messageBox.OnButton2Click = OnNotSave;
            }
            else
                OnNotSave();

            bool OnSave()
            {
                SaveChanges();
                return true;
            }
            bool OnNotSave()
            {
                NotSaveChanges();
                return true;
            }
        }
    }

    public class EditTemplateMode : BaseToolMode
    {
        public override ToolModeType Type => ToolModeType.PanelAction;

        private ITemplateEditor Editor { get; set; }

        public void Init(ITemplateEditor editor) => Editor = editor;
        public override void OnSecondaryMouseClicked() => Editor?.Cancel();
    }
}
