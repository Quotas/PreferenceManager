using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Xml;

namespace SharedPreferenceManager
{
    [XmlType("Preferences")]
    public class PreferenceList : List<object>
    {

        public void Delete(string key)
        {

            foreach (IPreference<object> o in this)
            {

                if (o.Label == key)
                {

                    this.Remove(key);
                    return;
                }
            }

            //We didnt find the key so throw an exception
            throw new KeyNotFoundException("Key not found in preferences.");

        }

        public object GetPreference(string key)
        {
           

            foreach (object o in this)
            {
                Type type = o.GetType();
                PropertyInfo prop = type.GetProperty("Label");

                if ((string)prop.GetValue(o, null) == key)
                {

                    return o;
                }

            }

            throw new KeyNotFoundException("Key not found in preferences.");


        }

        public object GetPreferenceValue(string key)
        {


            
            foreach (object o in this)
            {
                Type type = o.GetType();
                PropertyInfo prop = type.GetProperty("Label");

                if ((string)prop.GetValue(o, null) == key)
                {

                    return o.GetType().GetProperty("Value").GetValue(o, null);
                }

            }

            throw new KeyNotFoundException("Key not found in preferences.");


        }
    }

    public class PreferenceManager
    {

        #region Enumerators

        public enum FileType { BINARY, XML }

        #endregion

        #region Member Variables
        [XmlElement, XmlArrayItem(Type = typeof(Preference<object>))]
        public PreferenceList m_SharedPreferences = new PreferenceList();

        #endregion

        #region Events
        //Have a general event here for any perefence changing so users can subscribe
        [field: XmlIgnoreAttribute()]
        public event Action<object, PreferenceChangedEventArgs> OnPrefenceChanged;
        
        #endregion

        #region Constructor

        public PreferenceManager()
        {



        }

        #endregion

        #region Properties

        #endregion

        #region Member Functions


        #endregion

        #region Public Functions

        public void OnPreferenceChangedHandler(object sender, PreferenceChangedEventArgs args) {

            Debug.Log("Something happened to " + sender);

            OnPrefenceChanged?.Invoke(sender, args);

        }

        public void AddPreference(string name, object value) 
        {

            try
            {
                //Because we dont know the type of the object we have to create a generic type to be evaluted at runtime
                var type = typeof(Preference<>).MakeGenericType(value.GetType());
                var a_PreferenceContext = Activator.CreateInstance(type);

                //Create our Delegate to bind to our event, this needs to be done before we set the value so our event is properly raised
                MethodInfo method = typeof(PreferenceManager).GetMethod("OnPreferenceChangedHandler");

                EventInfo eventInfo = type.GetEvent("OnPreferenceChanged");
                Type t = eventInfo.EventHandlerType;
                Delegate handler = Delegate.CreateDelegate(t, this, method);
                eventInfo.AddEventHandler(a_PreferenceContext, handler);

                //Set our name and value here
                type.GetMethod("SetValue").Invoke(a_PreferenceContext, new object[] { value } );
                type.GetMethod("SetName").Invoke(a_PreferenceContext, new object[] { name });


                //Add our preference to the shared perefence list
                m_SharedPreferences.Add(a_PreferenceContext);


            }
            catch (Exception e) {
                if (e.Source != null)
                    Console.WriteLine("Exception source: {0} /n Data: {1}", e.Source, value);
                throw;

            }

            
        }

        public object GetPreference(string key)
        {

            return m_SharedPreferences.GetPreference(key); 


        }

        public object GetPreferenceValue(string key)
        {
            return m_SharedPreferences.GetPreferenceValue(key);
        }
        
        public void DeleteAll()
        {

            //Clear our shared preferences,
            //TODO implement some kind of sanity check so that users dont call this by accident

            m_SharedPreferences.Clear();

        }

        public void Delete(string key)
        {
            //LUL
            m_SharedPreferences.Delete(key); 

        }

        public void Save(FileType type = FileType.BINARY, string path = null) {

            switch (type)
            {
                case FileType.BINARY:

                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new FileStream("Preferences.bin", FileMode.Create, FileAccess.Write, FileShare.None);

                    formatter.Serialize(stream, m_SharedPreferences);

                    stream.Close();
                    break;
                case FileType.XML:

                    //We need to get the types of all the elements in the array to pass to the serializer for it to work correctly
                    //This also makes it format it very strangely 
                    Type[] types = { typeof(Preference<System.Int32>)};
                    XmlSerializer xs = new XmlSerializer(typeof(PreferenceList), types);
                    //create an instance of the MemoryStream class since we intend to keep the XML string 
                    //in memory instead of saving it to a file.
                    Stream xmlstream = new FileStream("Preferences.xml", FileMode.Create, FileAccess.Write, FileShare.None);

                    //Serialize emp in the xmlTextWriter

                    List<object> xmlData = m_SharedPreferences as List<object>;
                    xs.Serialize(xmlstream, xmlData);

                    xmlstream.Close();
                    break;


            }



        }

        public void Load(string filename = "Preferences.bin", FileType type = FileType.BINARY)
        {


            switch (type)
            {
                case FileType.BINARY:


                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

                    PreferenceList obj = (PreferenceList)formatter.Deserialize(stream);

                    stream.Close();


                    m_SharedPreferences = obj;
                    break;


                case FileType.XML:

                    PreferenceList myObject;
                    // Construct an instance of the XmlSerializer with the type  
                    // of object that is being deserialized.  
                    XmlSerializer mySerializer = new XmlSerializer(typeof(PreferenceList));
                    // To read the file, create a FileStream.  
                    FileStream myFileStream = new FileStream("Preferences.xml", FileMode.Open, FileAccess.Read, FileShare.Read);
                    // Call the Deserialize method and cast to the object type.  
                    myObject = (PreferenceList)mySerializer.Deserialize(myFileStream);

                    m_SharedPreferences = myObject;

                    myFileStream.Close();
                    break;


            }


        }
        #endregion



    }
}
