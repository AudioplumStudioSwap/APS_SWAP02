using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Aube
{
    //! @class FxCommand
    //!
    //! @brief  Tool to parse an animation event command.
    //!         e.g. "param1 -x1 param2 -x2 param3" gives (key, value) pairs : {(MAIN_PARAM, param1);(x2, param2);(x2 param3)}.
    public class FxCommand
    {
    #region Declarations
    #region Private
        //! @class Param
        //!
        //! @brief A command parameter.
        public class Param
        {
            private string m_key;
            private string m_value;

            public string Key { get { return m_key; } }
            public string Value
            {
                get 
                {
                    return m_value; 
                }
            }

            public Param(string key, string value)
            {
                m_key = key.Trim().ToLower();
                m_value = (value != null)? value.Trim() : null;
            }

            public bool HasKey(string key)
            {
                return m_key == key.ToLower();
            }
        }
    #endregion
    #endregion

    #region Attributes
    #region Public
        public const string MAIN_PARAM = "MAIN"; //! Default parameter key
    #endregion
    #region Private
        private const char PARAM_SEPARATOR = '-';
        private const char KEY_SEPARATOR = ' ';

        private List<Param> m_params = new List<Param>();
    #endregion
    #endregion

    #region Methods
    #region Public
        public FxCommand(string command)
        {
            Parse(command);
        }

        public void Parse(string command)
        {
            m_params = new List<Param>();

            command.Trim();

            string[] list = command.Split(PARAM_SEPARATOR);
            
            for (int i = 0; i < list.Length; ++i)
            {
                list[i].Trim();

                if (string.IsNullOrEmpty(list[i]))
                {
                    continue;
                }

                bool option = i > 0 || command[0] == PARAM_SEPARATOR;
                int sepIdx = list[i].IndexOf(KEY_SEPARATOR);

                string key = (option)? ((sepIdx >= 0)? list[i].Substring(0, sepIdx) : list[i]) : MAIN_PARAM; 
                string value = (option)? ((sepIdx >= 0)? list[i].Substring(sepIdx, list[i].Length - sepIdx) : null) : list[i];
                m_params.Add(new Param(key, value));
            }
        }

        public Param GetParam(string key)
        {
            for (int i = 0, count = m_params.Count; i < count; ++i)
            {
                if (m_params[i].HasKey(key))
                {
                    return m_params[i];
                }
            }
            return null;
        }
    #endregion
    #endregion
    }
}
