﻿using ColossalFramework.Math;
using ColossalFramework.UI;
using NodeMarkup.UI;
using NodeMarkup.UI.Editors;
using NodeMarkup.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace NodeMarkup.Manager
{
    public class SolidStopLineStyle : StopLineStyle, IStopLine
    {
        public override StyleType Type => StyleType.StopLineSolid;

        public SolidStopLineStyle(Color32 color, float width) : base(color, width) { }

        protected override IEnumerable<MarkupStyleDash> Calculate(MarkupStopLine stopLine, ILineTrajectory trajectory)
        {
            var offset = ((stopLine.Start.Direction + stopLine.End.Direction) / -2).normalized * (Width / 2);
            return StyleHelper.CalculateSolid(trajectory, CalculateDashes);

            IEnumerable<MarkupStyleDash> CalculateDashes(ILineTrajectory dashTrajectory)
            {
                yield return StyleHelper.CalculateSolidDash(dashTrajectory, offset, offset, Width, Color);
            }
        }

        public override StopLineStyle CopyStopLineStyle() => new SolidStopLineStyle(Color, Width);
    }
    public class DoubleSolidStopLineStyle : SolidStopLineStyle, IStopLine, IDoubleLine
    {
        public override StyleType Type => StyleType.StopLineDoubleSolid;

        float _offset;
        public float Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                StyleChanged();
            }
        }

        public DoubleSolidStopLineStyle(Color color, float width, float offset) : base(color, width)
        {
            Offset = offset;
        }
        protected override IEnumerable<MarkupStyleDash> Calculate(MarkupStopLine stopLine, ILineTrajectory trajectory)
        {
            var offsetNormal = ((stopLine.Start.Direction + stopLine.End.Direction) / -2).normalized;
            var offsetLeft = offsetNormal * (Width / 2);
            var offsetRight = offsetNormal * (Width / 2 + 2 * Offset);

            return StyleHelper.CalculateSolid(trajectory, CalculateDashes);

            IEnumerable<MarkupStyleDash> CalculateDashes(ILineTrajectory dashTrajectory)
            {
                yield return StyleHelper.CalculateSolidDash(dashTrajectory, offsetLeft, offsetLeft, Width, Color);
                yield return StyleHelper.CalculateSolidDash(dashTrajectory, offsetRight, offsetRight, Width, Color);
            }
        }

        public override StopLineStyle CopyStopLineStyle() => new DoubleSolidStopLineStyle(Color, Width, Offset);
        public override void CopyTo(Style target)
        {
            base.CopyTo(target);
            if (target is IDoubleLine doubleTarget)
            {
                doubleTarget.Offset = Offset;
            }
        }

        public override List<EditorItem> GetUIComponents(object editObject, UIComponent parent, Action onHover = null, Action onLeave = null, bool isTemplate = false)
        {
            var components = base.GetUIComponents(editObject, parent, onHover, onLeave, isTemplate);
            components.Add(AddOffsetProperty(this, parent, onHover, onLeave));
            return components;
        }
        public override XElement ToXml()
        {
            var config = base.ToXml();
            config.Add(new XAttribute("O", Offset));
            return config;
        }
        public override void FromXml(XElement config, ObjectsMap map, bool invert)
        {
            base.FromXml(config, map, invert);
            Offset = config.GetAttrValue("O", DefaultOffset);
        }
    }
    public class DashedStopLineStyle : StopLineStyle, IStopLine, IDashedLine
    {
        public override StyleType Type { get; } = StyleType.StopLineDashed;

        float _dashLength;
        float _spaceLength;

        public float DashLength
        {
            get => _dashLength;
            set
            {
                _dashLength = value;
                StyleChanged();
            }
        }
        public float SpaceLength
        {
            get => _spaceLength;
            set
            {
                _spaceLength = value;
                StyleChanged();
            }
        }

        public DashedStopLineStyle(Color color, float width, float dashLength, float spaceLength) : base(color, width)
        {
            DashLength = dashLength;
            SpaceLength = spaceLength;
        }

        protected override IEnumerable<MarkupStyleDash> Calculate(MarkupStopLine stopLine, ILineTrajectory trajectory)
        {
            var offset = ((stopLine.Start.Direction + stopLine.End.Direction) / -2).normalized * (Width / 2);
            return StyleHelper.CalculateDashed(trajectory, DashLength, SpaceLength, CalculateDashes);

            IEnumerable<MarkupStyleDash> CalculateDashes(ILineTrajectory dashTrajectory, float startT, float endT)
            {
                yield return StyleHelper.CalculateDashedDash(dashTrajectory, startT, endT, DashLength, offset, offset, Width, Color);
            }
        }

        public override StopLineStyle CopyStopLineStyle() => new DashedStopLineStyle(Color, Width, DashLength, SpaceLength);
        public override void CopyTo(Style target)
        {
            base.CopyTo(target);
            if (target is IDashedLine dashedTarget)
            {
                dashedTarget.DashLength = DashLength;
                dashedTarget.SpaceLength = SpaceLength;
            }
        }
        public override List<EditorItem> GetUIComponents(object editObject, UIComponent parent, Action onHover = null, Action onLeave = null, bool isTemplate = false)
        {
            var components = base.GetUIComponents(editObject, parent, onHover, onLeave, isTemplate);
            components.Add(AddDashLengthProperty(this, parent, onHover, onLeave));
            components.Add(AddSpaceLengthProperty(this, parent, onHover, onLeave));
            return components;
        }

        public override XElement ToXml()
        {
            var config = base.ToXml();
            config.Add(new XAttribute("DL", DashLength));
            config.Add(new XAttribute("SL", SpaceLength));
            return config;
        }
        public override void FromXml(XElement config, ObjectsMap map, bool invert)
        {
            base.FromXml(config, map, invert);
            DashLength = config.GetAttrValue("DL", DefaultDashLength);
            SpaceLength = config.GetAttrValue("SL", DefaultSpaceLength);
        }
    }
    public class DoubleDashedStopLineStyle : DashedStopLineStyle, IStopLine, IDoubleLine
    {
        public override StyleType Type { get; } = StyleType.StopLineDoubleDashed;

        float _offset;
        public float Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                StyleChanged();
            }
        }
        public DoubleDashedStopLineStyle(Color color, float width, float dashLength, float spaceLength, float offset) : base(color, width, dashLength, spaceLength)
        {
            Offset = offset;
        }
        public override StopLineStyle CopyStopLineStyle() => new DoubleDashedStopLineStyle(Color, Width, DashLength, SpaceLength, Offset);
        public override void CopyTo(Style target)
        {
            base.CopyTo(target);
            if (target is IDoubleLine doubleTarget)
            {
                doubleTarget.Offset = Offset;
            }
        }

        protected override IEnumerable<MarkupStyleDash> Calculate(MarkupStopLine stopLine, ILineTrajectory trajectory)
        {
            var offsetNormal = ((stopLine.Start.Direction + stopLine.End.Direction) / -2).normalized;
            var offsetLeft = offsetNormal * (Width / 2);
            var offsetRight = offsetNormal * (Width / 2 + 2 * Offset);

            return StyleHelper.CalculateDashed(trajectory, DashLength, SpaceLength, CalculateDashes);

            IEnumerable<MarkupStyleDash> CalculateDashes(ILineTrajectory dashTrajectory, float startT, float endT)
            {
                yield return StyleHelper.CalculateDashedDash(dashTrajectory, startT, endT, DashLength, offsetLeft, offsetLeft, Width, Color);
                yield return StyleHelper.CalculateDashedDash(dashTrajectory, startT, endT, DashLength, offsetRight, offsetRight, Width, Color);
            }
        }

        public override List<EditorItem> GetUIComponents(object editObject, UIComponent parent, Action onHover = null, Action onLeave = null, bool isTemplate = false)
        {
            var components = base.GetUIComponents(editObject, parent, onHover, onLeave, isTemplate);
            components.Add(AddOffsetProperty(this, parent, onHover, onLeave));
            return components;
        }
        public override XElement ToXml()
        {
            var config = base.ToXml();
            config.Add(new XAttribute("O", Offset));
            return config;
        }
        public override void FromXml(XElement config, ObjectsMap map, bool invert)
        {
            base.FromXml(config, map, invert);
            Offset = config.GetAttrValue("O", DefaultOffset);
        }
    }
    public class SolidAndDashedStopLineStyle : StopLineStyle, IStopLine, IDoubleLine, IDashedLine
    {
        public override StyleType Type => StyleType.StopLineSolidAndDashed;

        float _offset;
        float _dashLength;
        float _spaceLength;
        public float Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                StyleChanged();
            }
        }
        public float DashLength
        {
            get => _dashLength;
            set
            {
                _dashLength = value;
                StyleChanged();
            }
        }
        public float SpaceLength
        {
            get => _spaceLength;
            set
            {
                _spaceLength = value;
                StyleChanged();
            }
        }

        public SolidAndDashedStopLineStyle(Color color, float width, float dashLength, float spaceLength, float offset) : base(color, width)
        {
            Offset = offset;
            DashLength = dashLength;
            SpaceLength = spaceLength;
        }


        protected override IEnumerable<MarkupStyleDash> Calculate(MarkupStopLine stopLine, ILineTrajectory trajectory)
        {
            var offsetNormal = ((stopLine.Start.Direction + stopLine.End.Direction) / -2).normalized;
            var solidOffset = offsetNormal * (Width / 2);
            var dashedOffset = offsetNormal * (Width / 2 + 2 * Offset);

            foreach (var dash in StyleHelper.CalculateSolid(trajectory, CalculateSolidDash))
                yield return dash;

            foreach (var dash in StyleHelper.CalculateDashed(trajectory, DashLength, SpaceLength, CalculateDashedDash))
                yield return dash;

            IEnumerable<MarkupStyleDash> CalculateSolidDash(ILineTrajectory lineTrajectory)
            {
                yield return StyleHelper.CalculateSolidDash(lineTrajectory, solidOffset, solidOffset, Width, Color);
            }

            IEnumerable<MarkupStyleDash> CalculateDashedDash(ILineTrajectory lineTrajectory, float startT, float endT)
            {
                yield return StyleHelper.CalculateDashedDash(lineTrajectory, startT, endT, DashLength, dashedOffset, dashedOffset, Width, Color);
            }
        }

        public override StopLineStyle CopyStopLineStyle() => new SolidAndDashedStopLineStyle(Color, Width, DashLength, SpaceLength, Offset);
        public override void CopyTo(Style target)
        {
            base.CopyTo(target);
            if (target is IDashedLine dashedTarget)
            {
                dashedTarget.DashLength = DashLength;
                dashedTarget.SpaceLength = SpaceLength;
            }
            if (target is IDoubleLine doubleTarget)
            {
                doubleTarget.Offset = Offset;
            }
        }
        public override List<EditorItem> GetUIComponents(object editObject, UIComponent parent, Action onHover = null, Action onLeave = null, bool isTemplate = false)
        {
            var components = base.GetUIComponents(editObject, parent, onHover, onLeave, isTemplate);
            components.Add(AddDashLengthProperty(this, parent, onHover, onLeave));
            components.Add(AddSpaceLengthProperty(this, parent, onHover, onLeave));
            components.Add(AddOffsetProperty(this, parent, onHover, onLeave));

            return components;
        }

        public override XElement ToXml()
        {
            var config = base.ToXml();
            config.Add(new XAttribute("O", Offset));
            config.Add(new XAttribute("DL", DashLength));
            config.Add(new XAttribute("SL", SpaceLength));
            return config;
        }
        public override void FromXml(XElement config, ObjectsMap map, bool invert)
        {
            base.FromXml(config, map, invert);
            Offset = config.GetAttrValue("O", DefaultOffset);
            DashLength = config.GetAttrValue("DL", DefaultDashLength);
            SpaceLength = config.GetAttrValue("SL", DefaultSpaceLength);
        }
    }
    public class SharkTeethStopLineStyle : StopLineStyle, IColorStyle, ISharkLIne
    {
        public override StyleType Type { get; } = StyleType.StopLineSharkTeeth;

        float _base;
        float _height;
        float _space;
        public float Base
        {
            get => _base;
            set
            {
                _base = value;
                StyleChanged();
            }
        }
        public float Height
        {
            get => _height;
            set
            {
                _height = value;
                StyleChanged();
            }
        }
        public float Space
        {
            get => _space;
            set
            {
                _space = value;
                StyleChanged();
            }
        }
        public SharkTeethStopLineStyle(Color color, float baseValue, float height, float space) : base(color, 0)
        {
            Base = baseValue;
            Height = height;
            Space = space;
        }
        protected override IEnumerable<MarkupStyleDash> Calculate(MarkupStopLine stopLine, ILineTrajectory trajectory)
        {
            foreach (var dash in StyleHelper.CalculateDashed(trajectory, Base, Space, CalculateDashes))
            {
                dash.MaterialType = MaterialType.Triangle;
                yield return dash;
            }
        }
        IEnumerable<MarkupStyleDash> CalculateDashes(ILineTrajectory lineTrajectory, float startT, float endT)
        {
            yield return StyleHelper.CalculateDashedDash(lineTrajectory, startT, endT, Base, Height / -2, Height, Color);
        }

        public override StopLineStyle CopyStopLineStyle() => new SharkTeethStopLineStyle(Color, Base, Height, Space);
        public override void CopyTo(Style target)
        {
            base.CopyTo(target);
            if (target is SharkTeethStopLineStyle sharkTeethTarget)
            {
                sharkTeethTarget.Base = Base;
                sharkTeethTarget.Height = Height;
                sharkTeethTarget.Space = Space;
            }
        }
        public override List<EditorItem> GetUIComponents(object editObject, UIComponent parent, Action onHover = null, Action onLeave = null, bool isTemplate = false)
        {
            var components = base.GetUIComponents(editObject, parent, onHover, onLeave, isTemplate);
            components.Add(AddBaseProperty(this, parent, onHover, onLeave));
            components.Add(AddHeightProperty(this, parent, onHover, onLeave));
            components.Add(AddSpaceProperty(this, parent, onHover, onLeave));

            return components;
        }

        public override XElement ToXml()
        {
            var config = base.ToXml();
            config.Add(new XAttribute("B", Base));
            config.Add(new XAttribute("H", Height));
            config.Add(new XAttribute("S", Space));
            return config;
        }
        public override void FromXml(XElement config, ObjectsMap map, bool invert)
        {
            base.FromXml(config, map, invert);
            Base = config.GetAttrValue("B", DefaultSharkBaseLength);
            Height = config.GetAttrValue("H", DefaultSharkHeight);
            Space = config.GetAttrValue("S", DefaultSharkSpaceLength);
        }
    }
}
