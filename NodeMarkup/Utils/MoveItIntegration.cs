﻿using MoveItIntegration;
using System;
using System.Collections.Generic;
using NodeMarkup.Manager;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Linq;

namespace NodeMarkup.Utils
{
    public class MoveItIntegrationFactory : IMoveItIntegrationFactory
    {
        public string Name => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public MoveItIntegrationBase GetInstance() => new MoveItIntegration();
    }

    public class MoveItIntegration : MoveItIntegrationBase
    {
        public override string ID => "CS.macsergey.NodeMarkup";

        public override string Name => Mod.StaticName;

        public override string Description => Localize.Mod_Description;

        public override Version DataVersion => new Version(1, 0);

        public override object Copy(InstanceID sourceInstanceID)
        {
            if (sourceInstanceID.Type == InstanceType.NetNode)
            {
                ushort nodeID = sourceInstanceID.NetNode;
                if (MarkupManager.TryGetMarkup(nodeID, out Markup markup))
                {
                    var data = markup.ToXml();
                    return data;
                }
                else
                    return null;
            }
            return null;
        }

        public override void Paste(InstanceID targetInstanceID, object record, Dictionary<InstanceID, InstanceID> sourceMap)
            => Paste(targetInstanceID, record, false, sourceMap, PasteMapFiller);
        private void PasteMapFiller(Markup markup, ObjectsMap map, Dictionary<InstanceID, InstanceID> sourceMap)
        {
            foreach (var source in sourceMap.Where(p => IsCorrect(p)))
                map.AddSegment(source.Key.NetSegment, source.Value.NetSegment);
        }

        public override void Mirror(InstanceID targetInstanceID, object record, Dictionary<InstanceID, InstanceID> sourceMap, float instanceRotation, float mirrorRotation)
            => Paste(targetInstanceID, record, true, sourceMap, MirrorMapFiller);
        private void MirrorMapFiller(Markup markup, ObjectsMap map, Dictionary<InstanceID, InstanceID> sourceMap)
        {
            foreach (var source in sourceMap.Where(p => IsCorrect(p)))
            {
                if (!markup.TryGetEnter(source.Value.NetSegment, out Enter enter))
                    continue;

                var sourceSegment = source.Key.NetSegment;
                var targetSetment = source.Value.NetSegment;
                map.AddSegment(sourceSegment, targetSetment);
                map.AddMirrorEnter(enter);
            }
        }

        private void Paste(InstanceID targetInstanceID, object record, bool isMirror, Dictionary<InstanceID, InstanceID> sourceMap, Action<Markup, ObjectsMap, Dictionary<InstanceID, InstanceID>> mapFiller)
        {
            if (targetInstanceID.Type != InstanceType.NetNode || !(record is XElement config))
                return;

            ushort nodeID = targetInstanceID.NetNode;
            var map = new ObjectsMap(isMirror);
            var markup = MarkupManager.Get(nodeID);
            mapFiller(markup, map, sourceMap);
            markup.FromXml(Mod.Version, config, map);
        }
        private bool IsCorrect(KeyValuePair<InstanceID, InstanceID> pair) => pair.Key.Type == InstanceType.NetSegment && pair.Value.Type == InstanceType.NetSegment;


        public override string Encode64(object record)
        {
            if (record == null) return null;
            return EncodeUtil.BinaryEncode64(record.ToString());
        }

        public override object Decode64(string record, Version dataVersion)
        {
            if (record == null || record.Length == 0) return null;

            XElement xml;
            using (StringReader input = new StringReader((string)EncodeUtil.BinaryDecode64(record)))
            {
                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings
                {
                    IgnoreWhitespace = true,
                    ProhibitDtd = false,
                    XmlResolver = null
                };
                using (XmlReader reader = XmlReader.Create(input, xmlReaderSettings))
                {
                    xml = XElement.Load(reader, LoadOptions.None);
                }
            }
            return xml;
        }
    }
}
