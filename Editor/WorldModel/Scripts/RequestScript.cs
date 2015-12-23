using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    // Any changes here should also be reflected in CoreEditorScriptsOutput.aslx (validvalues for "request" command)
    // and also in the documentation https://github.com/textadventures/quest/blob/gh-pages/scripts/request.md
    internal enum Request
    {
        Quit,
        UpdateLocation,
        GameName,
        FontName,
        FontSize,
        Background,
        Foreground,
        LinkForeground,
        RunScript,
        SetStatus,
        ClearScreen,
        PanesVisible,
        ShowPicture,
        Show,
        Hide,
        SetCompassDirections,
        Pause,
        Wait,
        SetInterfaceString,
        RequestSave,
        SetPanelContents,
        Log,
        Speak
    }

    public class RequestScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "request"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new RequestScript(scriptContext, parameters[0], new Expression<string>(parameters[1], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
    }

    public class RequestScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private Request m_request;
        private IFunction<string> m_data;

        public RequestScript(ScriptContext scriptContext, string request, IFunction<string> data)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_data = data;
            m_request = (Request)(Enum.Parse(typeof(Request), request));
        }

        protected override ScriptBase CloneScript()
        {
            return new RequestScript(m_scriptContext, m_request.ToString(), m_data.Clone());
        }

        public override string Save()
        {
            return SaveScript("request", m_request.ToString(), m_data.Save());
        }

        public override string Keyword
        {
            get
            {
                return "request";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_request.ToString();
                case 1:
                    return m_data.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_request = (Request)(Enum.Parse(typeof(Request), (string)value));
                    break;
                case 1:
                    m_data = new Expression<string>((string)value, m_scriptContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
