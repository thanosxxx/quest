﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public interface IASLDebug
    {
        event ObjectsUpdatedHandler ObjectsUpdated;
        List<string> GetObjects(string type);
        DebugData GetDebugData(string obj);
        List<string> DebuggerObjectTypes { get; }
    }

    public class DebugDataItem
    {
        public string Value
        {
            get;
            private set;
        }

        public bool IsInherited
        {
            get;
            set;
        }

        public string Source
        {
            get;
            set;
        }

        public DebugDataItem(string value)
            : this(value, false)
        {
        }

        public DebugDataItem(string value, bool isInherited)
            :this(value, isInherited, null)
        {
        }

        public DebugDataItem(string value, bool isInherited, string source)
        {
            Value = value;
            IsInherited = isInherited;
            Source = source;
        }
    }

    public class DebugData
    {
        private Dictionary<string, DebugDataItem> m_data = new Dictionary<string, DebugDataItem>();
        public Dictionary<string, DebugDataItem> Data
        {
            get { return m_data; }
            set { m_data = value; }
        }
    }
}
