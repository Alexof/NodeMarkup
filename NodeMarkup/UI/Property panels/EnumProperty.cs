﻿using ColossalFramework.UI;
using NodeMarkup.Manager;
using NodeMarkup.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace NodeMarkup.UI.Editors
{
    public abstract class EnumPropertyPanel<EnumType, UISelector> : ListPropertyPanel<EnumType, UISelector>
        where EnumType : Enum
        where UISelector : UIComponent, IUISelector<EnumType>
    {
        protected override bool AllowNull => false;
        public override void Init()
        {
            base.Init();
            FillItems();
        }
        protected virtual void FillItems()
        {
            foreach (var value in Utilities.GetEnumValues<EnumType>())
                Selector.AddItem(value, value.Description());
        }
    }
    public abstract class StylePropertyPanel : EnumPropertyPanel<Style.StyleType, StylePropertyPanel.StyleDropDown>
    {
        protected override bool IsEqual(Style.StyleType first, Style.StyleType second) => first == second;
        public class StyleDropDown : UIDropDown<Style.StyleType> { }
    }
    public abstract class StylePropertyPanel<StyleType> : StylePropertyPanel
        where StyleType : Enum
    {
        protected override void FillItems()
        {
            foreach (var value in Enum.GetValues(typeof(StyleType)).Cast<object>().Cast<Style.StyleType>())
            {
                if (value.IsVisible())
                    Selector.AddItem(value, value.Description());
            }
        }
    }
    public class RegularStylePropertyPanel : StylePropertyPanel<RegularLineStyle.RegularLineType> { }
    public class StopStylePropertyPanel : StylePropertyPanel<StopLineStyle.StopLineType> { }
    public class CrosswalkPropertyPanel : StylePropertyPanel<CrosswalkStyle.CrosswalkType> { }
    public class FillerStylePropertyPanel : StylePropertyPanel<FillerStyle.FillerType> { }



    public class MarkupLineListPropertyPanel : ListPropertyPanel<MarkupLine, MarkupLineListPropertyPanel.MarkupLineDropDown>
    {
        protected override bool IsEqual(MarkupLine first, MarkupLine second) => ReferenceEquals(first, second);
        public class MarkupLineDropDown : UIDropDown<MarkupLine> { }
    }
    public class ChevronFromPropertyPanel : EnumPropertyPanel<ChevronFillerStyle.From, ChevronFromPropertyPanel.ChevronFromSegmented>
    {
        protected override bool IsEqual(ChevronFillerStyle.From first, ChevronFillerStyle.From second) => first == second;
        public class ChevronFromSegmented : UISegmented<ChevronFillerStyle.From> { }
    }
    public class LineAlignmentPropertyPanel : EnumPropertyPanel<LineStyle.StyleAlignment, LineAlignmentPropertyPanel.AlignmentSegmented>
    {
        protected override bool IsEqual(LineStyle.StyleAlignment first, LineStyle.StyleAlignment second) => first == second;
        public class AlignmentSegmented : UISegmented<LineStyle.StyleAlignment> { }
    }
    public class BoolListPropertyPanel : ListPropertyPanel<bool, BoolListPropertyPanel.BoolSegmented>
    {
        protected override bool AllowNull => false;
        protected override bool IsEqual(bool first, bool second) => first == second;
        public void Init(string falseLabel, string trueLabel)
        {
            base.Init();
            Selector.AddItem(true, trueLabel);
            Selector.AddItem(false, falseLabel);
        }

        public class BoolSegmented : UISegmented<bool> { }
    }
}
