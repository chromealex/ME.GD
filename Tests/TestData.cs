using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.GD.Tests {

    public class TestData {

        [NUnit.Framework.TestAttribute]
        public void Update() {

            var url = "Assets/StreamingAssets/TestData.csv";
            var assets = UnityEditor.AssetDatabase.FindAssets("t:GDData");
            if (assets.Length > 0) {

                var asset = assets[0];
                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GDData>(UnityEditor.AssetDatabase.GUIDToAssetPath(asset));
                if (obj != null) {

                    var data = System.IO.File.ReadAllText(url);
                    
                    var sys = new GDSystem();
                    sys.Update(data, "0.1", obj);
                    sys.Use(obj);
                    
                    Debug.Log("Updated: " + data);
                    sys.Get(new GDKey() { key = "key1" }, out int f);
                    Debug.Log(f);

                }

            }

        }
        
    }

}
