namespace ME.GD.Editor {

    [UnityEditor.InitializeOnLoadAttribute]
    public static class GDSystemEditor {

        static GDSystemEditor() {

            var gdSystem = new GDSystem();
            GDSystem.SetActive(gdSystem);
            gdSystem.Use(GDSystemEditor.GetMainData());

        }

        public static string GetKeyCaption(GDKey key, GDData data) {

            if (string.IsNullOrEmpty(key.key) == true) return "None";
            
            var sys = new GDSystem();
            sys.Use(data);
            sys.Get(key, out string val, forced: true);
            return key.key + " = " + val;

        }
        
        public static GDData GetMainData() {

            var mono = UnityEngine.Object.FindObjectOfType<GDSystemMonoBehaviour>();
            if (mono != null) {

                return mono.data;

            }
            
            var assets = UnityEditor.AssetDatabase.FindAssets("t:GDData");
            if (assets.Length > 0) {

                var asset = assets[0];
                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GDData>(UnityEditor.AssetDatabase.GUIDToAssetPath(asset));
                if (obj != null) {

                    return obj;

                }

            }

            return null;

        }

    }

}