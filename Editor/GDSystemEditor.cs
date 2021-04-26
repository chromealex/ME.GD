namespace ME.GD.Editor {

    [UnityEditor.InitializeOnLoadAttribute]
    public static class GDSystemEditor {

        static GDSystemEditor() {

            var data = GDSystemEditor.GetData();
            for (int i = 0; i < data.Length; ++i) {
                
                var gdSystem = new GDSystem();
                GDSystem.SetActive(gdSystem, data[i].index, data[i].name);
                gdSystem.Use(data[i].data);
                
            }

        }

        public static string GetKeyCaption(GDKey key, GDData data) {

            if (string.IsNullOrEmpty(key.key) == true) {
                
                if (string.IsNullOrEmpty(key.runtimeValue) == false) {

                    return $"<color=#008000ff><b>{key.runtimeValue}</b></color>";

                }
                
                return "None";
                
            }

            var sys = GDSystem.GetActive(data.index);
            if (sys == null) {

                sys = new GDSystem();
                sys.Use(data);
                GDSystem.SetActive(sys, data.index, data.name);

            }

            sys.Get(key, out string val, forced: true);
            if (string.IsNullOrEmpty(key.runtimeValue) == false) {

                return $"<color=#008000ff><b>{key.runtimeValue}</b></color> ({key.key} = {val})";

            }
            
            return $"{key.key} = {val}";

        }
        
        public static GDSystemMonoBehaviour.DataItem[] GetData() {

            var mono = UnityEngine.Object.FindObjectOfType<GDSystemMonoBehaviour>();
            if (mono != null) {

                return mono.items;

            }
            
            var assets = UnityEditor.AssetDatabase.FindAssets("t:GDData");
            if (assets.Length > 0) {

                var list = new System.Collections.Generic.List<GDSystemMonoBehaviour.DataItem>();
                for (int i = 0; i < assets.Length; ++i) {

                    var asset = assets[0];
                    var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GDData>(UnityEditor.AssetDatabase.GUIDToAssetPath(asset));
                    if (obj != null) {

                        list.Add(new GDSystemMonoBehaviour.DataItem() {
                            index = obj.index,
                            data = obj,
                        });

                    }

                }
                
            }

            return null;

        }

    }

}