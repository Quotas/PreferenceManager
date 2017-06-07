using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using SharedPreferenceManager;
using System.Reflection;


public class Entity : MonoBehaviour {

    // Use this for initialization

    PreferenceManager prefs = new PreferenceManager();
    private readonly string TEST_PREF = "Test";




    void Start ()
    {
        prefs.OnPrefenceChanged += Test;


        prefs.AddPreference(TEST_PREF, 1);
              
        
        Debug.Log(prefs.GetPreferenceValue(TEST_PREF));


        prefs.Save(PreferenceManager.FileType.XML);

        prefs.DeleteAll();

        prefs.Load("Preferences.xml", PreferenceManager.FileType.XML);


        Debug.Log(prefs.GetPreferenceValue(TEST_PREF));

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void Test(object sender, PreferenceChangedEventArgs e) {


        Type type = sender.GetType();
        PropertyInfo prop = type.GetProperty("TypeName");
        //check to see who sent the event
        Debug.Log("Test");
        Debug.Log(prop.GetValue(sender, null));

    }
    
}
