using System;
using System.Collections.Generic;
using System.IO;
using TextAdventures.Quest.Scripts;
using System.Linq;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest
{
    public enum GameState
    {
        NotStarted,
        Loading,
        Running,
        Finished
    }

    public enum UpdateSource
    {
        System,
        User
    }

    public enum WorldModelVersion
    {
        v500,
        v510,
        v520,
        v530,
        v540,
        v550
    }

    public class ObjectsUpdatedEventArgs : EventArgs
    {
        public string Added { get; set; }
        public string Removed { get; set; }
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

        public bool IsDefaultType
        {
            get;
            set;
        }

        public DebugDataItem(string value)
            : this(value, false)
        {
        }

        public DebugDataItem(string value, bool isInherited)
            : this(value, isInherited, null)
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

    public class WorldModel
    {
        private Element m_game;
        private Elements m_elements;
        private Dictionary<string, int> m_nextUniqueID = new Dictionary<string, int>();
        private Template m_template;
        private UndoLogger m_undoLogger;
        private string m_filename;
        private string m_libFolder = null;
        private List<string> m_errors;
        private GameState m_state = GameState.NotStarted;
        private Dictionary<ElementType, IElementFactory> m_elementFactories = new Dictionary<ElementType, IElementFactory>();
        private ObjectFactory m_objectFactory;
        private GameSaver m_saver;
        private string m_saveFilename = string.Empty;
        private Functions.ExpressionOwner m_expressionOwner;
        private object m_threadLock = new object();
        private List<string> m_attributeNames = new List<string>();
        private RegexCache m_regexCache = new RegexCache();

        private static Dictionary<ObjectType, string> s_defaultTypeNames = new Dictionary<ObjectType, string>();
        private static Dictionary<string, Type> s_typeNamesToTypes = new Dictionary<string, Type>();
        private static Dictionary<Type, string> s_typesToTypeNames = new Dictionary<Type, string>();

        public event EventHandler<ObjectsUpdatedEventArgs> ObjectsUpdated;
        public event EventHandler<ElementFieldUpdatedEventArgs> ElementFieldUpdated;
        public event EventHandler<ElementRefreshEventArgs> ElementRefreshed;
        public event EventHandler<ElementFieldUpdatedEventArgs> ElementMetaFieldUpdated;
        public event EventHandler<LoadStatusEventArgs> LoadStatus;

        public class ElementFieldUpdatedEventArgs : EventArgs
        {
            internal ElementFieldUpdatedEventArgs(Element element, string attribute, object newValue, bool isUndo)
            {
                Element = element;
                Attribute = attribute;
                NewValue = newValue;
                IsUndo = isUndo;
            }

            public Element Element { get; private set; }
            public string Attribute { get; private set; }
            public object NewValue { get; private set; }
            public bool IsUndo { get; private set; }
            public bool Refresh { get; private set; }
        }

        public class ElementRefreshEventArgs : EventArgs
        {
            internal ElementRefreshEventArgs(Element element)
            {
                Element = element;
            }

            public Element Element { get; private set; }
        }

        public class LoadStatusEventArgs : EventArgs
        {
            public LoadStatusEventArgs(string status)
            {
                Status = status;
            }

            public string Status { get; private set; }
        }

        static WorldModel()
        {
            s_defaultTypeNames.Add(ObjectType.Object, "defaultobject");
            s_defaultTypeNames.Add(ObjectType.Exit, "defaultexit");
            s_defaultTypeNames.Add(ObjectType.Command, "defaultcommand");
            s_defaultTypeNames.Add(ObjectType.Game, "defaultgame");
            s_defaultTypeNames.Add(ObjectType.TurnScript, "defaultturnscript");

            s_typeNamesToTypes.Add("string", typeof(string));
            s_typeNamesToTypes.Add("script", typeof(IScript));
            s_typeNamesToTypes.Add("boolean", typeof(bool));
            s_typeNamesToTypes.Add("int", typeof(int));
            s_typeNamesToTypes.Add("double", typeof(double));
            s_typeNamesToTypes.Add("object", typeof(Element));
            s_typeNamesToTypes.Add("stringlist", typeof(QuestList<string>));
            s_typeNamesToTypes.Add("objectlist", typeof(QuestList<Element>));
            s_typeNamesToTypes.Add("stringdictionary", typeof(QuestDictionary<string>));
            s_typeNamesToTypes.Add("objectdictionary", typeof(QuestDictionary<Element>));
            s_typeNamesToTypes.Add("scriptdictionary", typeof(QuestDictionary<IScript>));
            s_typeNamesToTypes.Add("dictionary", typeof(QuestDictionary<object>));
            s_typeNamesToTypes.Add("list", typeof (QuestList<object>));

            foreach (KeyValuePair<string, Type> kvp in s_typeNamesToTypes)
            {
                s_typesToTypeNames.Add(kvp.Value, kvp.Key);
            }
        }

        public WorldModel()
            : this(null, null)
        {
        }

        public WorldModel(string filename, string originalFilename)
        {
            m_expressionOwner = new Functions.ExpressionOwner(this);
            m_template = new Template(this);
            InitialiseElementFactories();
            m_objectFactory = (ObjectFactory)m_elementFactories[ElementType.Object];

            m_filename = filename;
            m_elements = new Elements();
            m_undoLogger = new UndoLogger(this);
            m_game = ObjectFactory.CreateObject("game", ObjectType.Game);
        }

        public WorldModel(string filename, string libFolder, string originalFilename)
            : this(filename, originalFilename)
        {
            m_libFolder = libFolder;
        }

        private void InitialiseElementFactories()
        {
            foreach (Type t in TextAdventures.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(IElementFactory)))
            {
                AddElementFactory((IElementFactory)Activator.CreateInstance(t));
            }
        }

        private void AddElementFactory(IElementFactory factory)
        {
            m_elementFactories.Add(factory.CreateElementType, factory);
            factory.WorldModel = this;
            factory.ObjectsUpdated += ElementsUpdated;
        }

        void ElementsUpdated(object sender, ObjectsUpdatedEventArgs args)
        {
            if (ObjectsUpdated != null) ObjectsUpdated(this, args);
        }

        internal static Dictionary<ObjectType, string> DefaultTypeNames
        {
            get { return s_defaultTypeNames; }
        }

        internal string GetUniqueID()
        {
            return GetUniqueID(null);
        }

        internal string GetUniqueID(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) prefix = "k";
            if (!m_nextUniqueID.ContainsKey(prefix))
            {
                m_nextUniqueID.Add(prefix, 0);
            }

            string newid;
            do
            {
                m_nextUniqueID[prefix]++;
                newid = prefix + m_nextUniqueID[prefix].ToString();
            } while (m_elements.ContainsKey(newid));
            
            return newid;
        }

        public Element Game
        {
            get { return m_game; }
        }

        public Element Object(string name)
        {
            return m_elements.Get(ElementType.Object, name);
        }

        public ObjectFactory ObjectFactory
        {
            get { return m_objectFactory; }
        }

        public IElementFactory GetElementFactory(ElementType t)
        {
            return m_elementFactories[t];
        }

        internal QuestList<Element> GetAllObjects()
        {
            return new QuestList<Element>(m_elements.Objects);
        }

        public bool ObjectContains(Element parent, Element searchObj)
        {
            if (searchObj.Parent == null) return false;
            if (searchObj.Parent == parent) return true;
            return ObjectContains(parent, searchObj.Parent);
        }

        public IEnumerable<Element> Objects
        {
            get
            {
                foreach (Element o in m_elements.Objects)
                    yield return o;
            }
        }

        public bool ObjectExists(string name)
        {
            return m_elements.ContainsKey(ElementType.Object, name);
        }

        /// <summary>
        /// Attempt to resolve an element name from elements which are eligible for expression,
        /// i.e. objects and timers
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool TryResolveExpressionElement(string name, out Element element)
        {
            element = null;
            if (!m_elements.ContainsKey(name)) return false;

            Element result = m_elements.Get(name);
            if (result.ElemType == ElementType.Object || result.ElemType == ElementType.Timer)
            {
                element = result;
                return true;
            }

            return false;
        }

        internal void RemoveElement(ElementType type, string name)
        {
            m_elements.Remove(type, name);
        }

        public bool InitialiseEdit()
        {
            GameLoader loader = new GameLoader(this, GameLoader.LoadMode.Edit);
            return InitialiseInternal(loader);
        }

        private bool InitialiseInternal(GameLoader loader)
        {
            if (m_state != GameState.NotStarted)
            {
                throw new Exception("Game already initialised");
            }
            loader.FilenameUpdated += loader_FilenameUpdated;
            loader.LoadStatus += loader_LoadStatus;
            m_state = GameState.Loading;
            
            var success = m_filename == null || loader.Load(m_filename);
            m_state = success ? GameState.Running : GameState.Finished;
            m_errors = loader.Errors;
            m_saver = new GameSaver(this);
            return success;
        }

        void loader_LoadStatus(object sender, GameLoader.LoadStatusEventArgs e)
        {
            if (LoadStatus != null)
            {
                LoadStatus(this, new LoadStatusEventArgs(e.Status));
            }
        }

        void loader_FilenameUpdated(string filename)
        {
            // Update base ASLX filename to original filename if we're loading a saved game
            m_saveFilename = m_filename;
            m_filename = filename;
        }

        public List<string> Errors
        {
            get { return m_errors; }
        }

        public string Filename
        {
            get { return m_filename; }
        }

        internal Template Template
        {
            get { return m_template; }
        }

        public UndoLogger UndoLogger
        {
            get { return m_undoLogger; }
        }

        public DebugData GetDebugData(string el)
        {
            return m_elements.Get(el).GetDebugData();
        }

        public DebugData GetInheritedTypesDebugData(string el)
        {
            return m_elements.Get(el).Fields.GetInheritedTypesDebugData();
        }

        public DebugDataItem GetDebugDataItem(string el, string attribute)
        {
            return m_elements.Get(el).Fields.GetDebugDataItem(attribute);
        }

        public Element AddProcedure(string name)
        {
            Element proc = GetElementFactory(ElementType.Function).Create(name);
            return proc;
        }

        public Element AddDelegate(string name)
        {
            Element del = GetElementFactory(ElementType.Delegate).Create(name);
            return del;
        }

        public Element Procedure(string name)
        {
            if (!m_elements.ContainsKey(ElementType.Function, name)) return null;
            return m_elements.Get(ElementType.Function, name);
        }

        internal Element GetObjectType(string name)
        {
            return m_elements.Get(ElementType.ObjectType, name);
        }

        public GameState State
        {
            get { return m_state; }
        }

        public Elements Elements
        {
            get { return m_elements; }
        }

        public string Save(SaveMode mode, bool? includeWalkthrough = null, string html = null)
        {
            return m_saver.Save(mode, includeWalkthrough, html);
        }

        public static Type ConvertTypeNameToType(string name)
        {
            Type type;
            if (s_typeNamesToTypes.TryGetValue(name, out type))
            {
                return type;
            }

            if (name == "null") return null;

            // TO DO: type name could also be a DelegateImplementation
            //if (value is DelegateImplementation) return ((DelegateImplementation)value).TypeName;

            throw new ArgumentOutOfRangeException(string.Format("Unrecognised type name '{0}'", name));
        }

        public static string ConvertTypeToTypeName(Type type)
        {
            string name;
            if (s_typesToTypeNames.TryGetValue(type, out name))
            {
                return name;
            }

            foreach (KeyValuePair<Type, string> kvp in s_typesToTypeNames)
            {
                if (kvp.Key.IsAssignableFrom(type))
                {
                    return kvp.Value;
                }
            }

            throw new ArgumentOutOfRangeException(string.Format("Unrecognised type '{0}'", type.ToString()));
        }

        public string GetExternalPath(string file)
        {
            return GetExternalPath(file, true);
        }

        private string GetExternalPath(string file, bool throwException)
        {
            string resourcesFolder = Path.GetDirectoryName(Filename);
            return GetExternalPath(resourcesFolder, file, throwException);
        }

        private string GetExternalPath(string current, string file, bool throwException)
        {
            string path;

            if (TryPath(current, file, out path, false)) return path;
            // Only try other folders if we're not using a resource folder (i.e. a .quest file)
            // Because if we do have a resource folder, all required external files should be there.

            if (TryPath(Environment.CurrentDirectory, file, out path, false)) return path;
            if (!string.IsNullOrEmpty(m_libFolder) && TryPath(m_libFolder, file, out path, true)) return path;
            if (System.Reflection.Assembly.GetEntryAssembly() != null)
            {
                if (TryPath(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().CodeBase), file, out path, true)) return path;
            }
            if (throwException)
            {
                throw new Exception(
                    string.Format("Cannot find a file called '{0}' in current path or application/resource path", file));
            }
            return null;
        }

        public IEnumerable<string> GetAvailableLibraries()
        {
            List<string> result = new List<string>();
            AddFilesInPathToList(result, System.IO.Path.GetDirectoryName(Filename), false);
            AddFilesInPathToList(result, Environment.CurrentDirectory, false);
            if (m_libFolder != null) AddFilesInPathToList(result, m_libFolder, false);
            if (System.Reflection.Assembly.GetEntryAssembly() != null)
            {
                AddFilesInPathToList(result, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().CodeBase), true);
            }
            return result;
        }

        private void AddFilesInPathToList(List<string> list, string path, bool recurse, string searchPattern = "*.aslx")
        {
            path = TextAdventures.Utility.Utility.RemoveFileColonPrefix(path);
            System.IO.SearchOption option = recurse ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
            foreach (var result in System.IO.Directory.GetFiles(path, searchPattern, option))
            {
                if (result == Filename) continue;
                string filename = System.IO.Path.GetFileName(result);
                if (!list.Contains(filename)) list.Add(filename);
            }
        }

        public IEnumerable<string> GetAvailableExternalFiles(string searchPatterns)
        {
            List<string> result = new List<string>();
            string[] patterns = searchPatterns.Split(';');
            foreach (string searchPattern in patterns)
            {
                AddFilesInPathToList(result, System.IO.Path.GetDirectoryName(Filename), false, searchPattern);
            }
            return result;
        }

        private bool TryPath(string path, string file, out string fullPath, bool recurse)
        {
            path = TextAdventures.Utility.Utility.RemoveFileColonPrefix(path);
            fullPath = System.IO.Path.Combine(path, file);
            if (System.IO.File.Exists(fullPath))
            {
                return true;
            }
            else
            {
                if (recurse && !file.Contains("\\") && !file.Contains("/"))
                {
                    var results = System.IO.Directory.GetFiles(path, file, System.IO.SearchOption.AllDirectories);
                    if (results.Length > 0)
                    {
                        fullPath = results[0];
                        return true;
                    }
                }
                return false;
            }
        }

        internal void NotifyElementFieldUpdate(Element element, string attribute, object newValue, bool isUndo)
        {
            if (!element.Initialised) return;
            if (ElementFieldUpdated != null) ElementFieldUpdated(this, new ElementFieldUpdatedEventArgs(element, attribute, newValue, isUndo));
        }

        internal void NotifyElementMetaFieldUpdate(Element element, string attribute, object newValue, bool isUndo)
        {
            if (!element.Initialised) return;
            if (ElementMetaFieldUpdated != null) ElementMetaFieldUpdated(this, new ElementFieldUpdatedEventArgs(element, attribute, newValue, isUndo));
        }

        internal void NotifyElementRefreshed(Element element)
        {
            if (ElementRefreshed != null) ElementRefreshed(this, new ElementRefreshEventArgs(element));
        }

        internal Functions.ExpressionOwner ExpressionOwner
        {
            get { return m_expressionOwner; }
        }

        public ElementType GetElementTypeForTypeString(string typeString)
        {
            return Element.GetElementTypeForTypeString(typeString);
        }

        public ObjectType GetObjectTypeForTypeString(string typeString)
        {
            return Element.GetObjectTypeForTypeString(typeString);
        }

        public string GetTypeStringForElementType(ElementType type)
        {
            return Element.GetTypeStringForElementType(type);
        }

        public string GetTypeStringForObjectType(ObjectType type)
        {
            return Element.GetTypeStringForObjectType(type);
        }

        public bool IsDefaultTypeName(string typeName)
        {
            return DefaultTypeNames.ContainsValue(typeName);
        }

        public Element AddNewTemplate(string templateName)
        {
            return m_template.AddTemplate(templateName, string.Empty, false);
        }

        public Element TryGetTemplateElement(string templateName)
        {
            if (!m_template.TemplateExists(templateName)) return null;
            return m_template.GetTemplateElement(templateName);
        }

        private static System.Text.RegularExpressions.Regex s_removeTrailingDigits = new System.Text.RegularExpressions.Regex(@"\d*$");

        public string GetUniqueElementName(string elementName)
        {
            // If element name doesn't exist (element might have been Cut in the editor),
            // then just return the original name
            if (!Elements.ContainsKey(elementName))
            {
                return elementName;
            }

            // Otherwise get a uniquely numbered element
            string root = s_removeTrailingDigits.Replace(elementName, "");
            bool elementAlreadyExists = true;
            int number = 0;
            string result = null;

            while (elementAlreadyExists)
            {
                number++;
                result = root + number.ToString();
                elementAlreadyExists = Elements.ContainsKey(result);
            }

            return result;
        }

        internal void AddAttributeName(string name)
        {
            if (!m_attributeNames.Contains(name)) m_attributeNames.Add(name);
        }

        public IEnumerable<string> GetAllAttributeNames
        {
            get { return m_attributeNames.AsReadOnly(); }
        }

        public class PackageIncludeFile
        {
            public string Filename { get; set; }
            public Stream Content { get; set; }
        }

        public bool CreatePackage(string filename, bool includeWalkthrough, out string error, IEnumerable<PackageIncludeFile> includeFiles, Stream outputStream)
        {
            Packager packager = new Packager(this);
            return packager.CreatePackage(filename, includeWalkthrough, out error, includeFiles, outputStream);
        }

        private static List<string> s_functionNames = null;

        public IEnumerable<string> GetBuiltInFunctionNames()
        {
            if (s_functionNames == null)
            {
                System.Reflection.MethodInfo[] methods = typeof(ExpressionOwner).GetMethods();
                System.Reflection.MethodInfo[] stringMethods = typeof(StringFunctions).GetMethods();

                IEnumerable<System.Reflection.MethodInfo> allMethods = methods.Union(stringMethods);

                s_functionNames = new List<string>(allMethods.Select(m => m.Name));
            }

            return s_functionNames.AsReadOnly();
        }

        internal void UpdateElementSortOrder(Element movedElement)
        {
            // This function is called when an element is moved to a new parent.
            // When this happens, its SortIndex MetaField must be updated so that it
            // is at the end of the list of children.

            int maxIndex = -1;

            foreach (Element sibling in m_elements.GetDirectChildren(movedElement.Parent))
            {
                int thisSortIndex = sibling.MetaFields[MetaFieldDefinitions.SortIndex];
                if (thisSortIndex > maxIndex) maxIndex = thisSortIndex;
            }

            movedElement.MetaFields[MetaFieldDefinitions.SortIndex] = maxIndex + 1;
        }

        internal RegexCache RegexCache { get { return m_regexCache; } }

        public WorldModelVersion Version { get; internal set; }

        internal string VersionString { get; set; }

        public string TempFolder { get; set; }

        public int ASLVersion { get { return int.Parse(VersionString); } }

        public string GameID
        {
            get
            {
                string gameId = m_game.Fields[FieldDefinitions.GameID];
                if (gameId != null) return gameId;
                if (Config.ReadGameFileFromAzureBlob)
                {
                    var parts = m_filename.Split('/');
                    return parts[parts.Length - 2];
                }
                return TextAdventures.Utility.Utility.FileMD5Hash(m_filename);
            }
        }
        public string Category { get { return m_game.Fields[FieldDefinitions.Category]; } }
        public string Description { get { return m_game.Fields[FieldDefinitions.Description]; } }
        public string Cover { get { return m_game.Fields[FieldDefinitions.Cover]; } }
    }
}
