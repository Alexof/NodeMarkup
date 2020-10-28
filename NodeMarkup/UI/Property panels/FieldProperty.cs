﻿using ColossalFramework.UI;
using NodeMarkup.Tools;
using NodeMarkup.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NodeMarkup.UI.Editors
{
    public abstract class FieldPropertyPanel<ValueType> : EditorPropertyPanel, IReusable
    {
        protected UITextField Field { get; set; }

        public event Action<ValueType> OnValueChanged;
        public event Action OnHover;
        public event Action OnLeave;

        protected abstract bool CanUseWheel { get; }
        public bool UseWheel { get; set; }
        public ValueType WheelStep { get; set; }
        public float FieldWidth
        {
            get => Field.width;
            set => Field.width = value;
        }

        private bool ValueProgress { get; set; } = false;
        public virtual ValueType Value
        {
            get
            {
                try
                {
                    return (ValueType)TypeDescriptor.GetConverter(typeof(ValueType)).ConvertFromString(Field.text);
                }
                catch
                {
                    return default;
                }
            }
            set
            {
                if (!ValueProgress)
                {
                    ValueProgress = true;
                    Field.text = GetString(value);
                    OnValueChanged?.Invoke(value);                   
                    ValueProgress = false;
                }
            }
        }

        public FieldPropertyPanel()
        {
            Field = AddTextField(Control);

            Field.tooltip = Settings.ShowToolTip && CanUseWheel ? NodeMarkup.Localize.FieldPanel_ScrollWheel : string.Empty;
            Field.eventMouseWheel += FieldMouseWheel;
            Field.eventTextSubmitted += FieldTextSubmitted;
            Field.eventMouseHover += FieldHover;
            Field.eventMouseLeave += FieldLeave;
        }
        public override void DeInit()
        {
            base.DeInit();

            UseWheel = false;
            WheelStep = default;

            OnValueChanged = null;
            OnHover = null;
            OnLeave = null;
        }

        protected virtual string GetString(ValueType value) => value.ToString();
        protected abstract ValueType Increment(ValueType value, ValueType step, WheelMode mode);
        protected abstract ValueType Decrement(ValueType value, ValueType step, WheelMode mode);

        protected virtual void FieldTextSubmitted(UIComponent component, string value) => Value = Value;
        private void FieldHover(UIComponent component, UIMouseEventParameter eventParam) => OnHover?.Invoke();
        private void FieldLeave(UIComponent component, UIMouseEventParameter eventParam) => OnLeave?.Invoke();
        protected virtual void FieldMouseWheel(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (CanUseWheel && UseWheel)
            {
                var mode = NodeMarkupTool.ShiftIsPressed ? WheelMode.High : NodeMarkupTool.CtrlIsPressed ? WheelMode.Low : WheelMode.Normal;
                if (eventParam.wheelDelta < 0)
                    Value = Decrement(Value, WheelStep, mode);
                else
                    Value = Increment(Value, WheelStep, mode);
            }
        }
        public void Edit() => Field.Focus();

        protected enum WheelMode
        {
            Normal,
            Low,
            High
        }
    }
    public abstract class ComparableFieldPropertyPanel<ValueType> : FieldPropertyPanel<ValueType>
        where ValueType : IComparable<ValueType>
    {
        public ValueType MinValue { get; set; }
        public ValueType MaxValue { get; set; }
        public bool CheckMax { get; set; }
        public bool CheckMin { get; set; }

        public override ValueType Value
        {
            get => base.Value;
            set
            {
                var newValue = value;

                if (CheckMin && newValue.CompareTo(MinValue) < 0)
                    newValue = MinValue;

                if (CheckMax && newValue.CompareTo(MaxValue) > 0)
                    newValue = MaxValue;

                base.Value = newValue;
            }
        }

        public ComparableFieldPropertyPanel() => SetDefault();
        public override void DeInit()
        {
            base.DeInit();
            SetDefault();
        }
        private void SetDefault()
        {
            MinValue = default;
            MaxValue = default;
            CheckMin = false;
            CheckMax = false;
        }
    }
    public class FloatPropertyPanel : ComparableFieldPropertyPanel<float>
    {
        protected override bool CanUseWheel => true;

        protected override float Decrement(float value, float step, WheelMode mode)
        {
            step = mode == WheelMode.Low ? step / 10 : mode == WheelMode.High ? step * 10 : step;
            return (value - step).RoundToNearest(step);
        }
        protected override float Increment(float value, float step, WheelMode mode)
        {
            step = mode == WheelMode.Low ? step / 10 : mode == WheelMode.High ? step * 10 : step;
            return (value + step).RoundToNearest(step);
        }
        protected override string GetString(float value) => value.ToString("0.###");
    }
    public class StringPropertyPanel : FieldPropertyPanel<string>
    {
        protected override bool CanUseWheel => false;

        protected override string Decrement(string value, string step, WheelMode mode) => throw new NotSupportedException();
        protected override string Increment(string value, string step, WheelMode mode) => throw new NotSupportedException();
    }
    public class IntPropertyPanel : ComparableFieldPropertyPanel<int>
    {
        protected override bool CanUseWheel => true;

        protected override int Decrement(int value, int step, WheelMode mode) => value - step;
        protected override int Increment(int value, int step, WheelMode mode) => value + step;
    }
}
