using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SharedPreferenceManager
{

    
    public class PreferenceChangedEventArgs : EventArgs
    {
        //Empty for now but we want to be able to pass the old values and new values
        //There is an issue here with Generics and not knowing the type without having this class be a Generic

    }


    interface IPreference<T>
    {

        string Label { get; }

        T Value { get; set; }

        object GetValue();
        event Action<object, PreferenceChangedEventArgs> OnPreferenceChanged;

    }



    public class Preference<T> : IPreference<T>
    {


        #region Member Variables

        private T m_value;
        private string m_name;

        #endregion

        #region Events
        //public delegate void PreferenceChangedHandler(PreferenceChangedEventArgs<T> e);
        [field: XmlIgnoreAttribute()]
        public event Action<object, PreferenceChangedEventArgs> OnPreferenceChanged;
        #endregion

        #region Constructor

        public Preference()
        {

        }

        #endregion  

        #region Properties
        [XmlElement("Label")]
        public string Label  {
            get
            {

                return m_name;
            }

        }

        [XmlElement("TypeName")]
        public string TypeName 
        {
            get
            {

                string friendlyName = this.GetType().Name;

                if (this.GetType().IsGenericType)
                {
                    int iBacktick = friendlyName.IndexOf('`');
                    if (iBacktick > 0)
                    {
                        friendlyName = friendlyName.Remove(iBacktick);
                    }
                    friendlyName += "<";
                    Type[] typeParameters = this.GetType().GetGenericArguments();
                    for (int i = 0; i < typeParameters.Length; ++i)
                    {
                        string typeParamName = typeParameters[i].Name;
                        friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                    }
                    friendlyName += ">";
                }

                return friendlyName;


            }
            set
            {


            }
        }

        [XmlElement()]
        public T Value {
            get {

                return m_value;
            }
            // Check to see if the typed of the stored prefernce match, otherwise throw an expection 
            set {

                if (value.GetType() == typeof(T))
                {
                    //If the types match then set our value and call our OnPreferenceChanged event

                    //Call our event for preference changing

                    OnPreferenceChanged?.Invoke(this, new PreferenceChangedEventArgs());

                    m_value = value;

                }
                else {

                    throw new ArgumentException(String.Format("{0} does not match the stored preference value type", value), "value.GetType()"); 
                }

            }

        }


        #endregion

        #region Member Functions


        #endregion

        #region Public Functions

        public void SetValue(T value)
        {

            if (value.GetType() == typeof(T))
            {

                OnPreferenceChanged?.Invoke(this, new PreferenceChangedEventArgs());

                m_value = value;
                               
            }
            else
            {

                throw new ArgumentException(String.Format("{0} does not match the stored preference value type", value), "value.GetType()");
            }


        }

        public void SetName(string name)
        {

            m_name = name;

        }

        public object GetValue()
        {
            return m_value;

        }

        #endregion


    }
}
