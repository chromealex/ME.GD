
namespace ME.GD.Editor {

    [UnityEditor.CustomEditor(typeof(GDSystemMonoBehaviour))]
    public class GDSystemMonoBehaviourEditor : UnityEditor.Editor {

        private void OnEnable() {

            UnityEditor.EditorApplication.update += this.Repaint;

        }

        private void OnDisable() {
            
            UnityEditor.EditorApplication.update -= this.Repaint;
            
        }

        new private void SetDirty() {
            
            UnityEditor.EditorUtility.SetDirty(this.target);
            
        }

        public override void OnInspectorGUI() {

            var target = this.target as GDSystemMonoBehaviour;
            GUILayoutExt.DrawComponentHeader(this.serializedObject, "GD", () => {
                
                GUILayoutExt.DrawComponentHeaderItem("Is Ready", target.IsReady().ToString());
                GUILayoutExt.DrawComponentHeaderItem("Is Loading", target.IsLoading().ToString());
                GUILayoutExt.DrawComponentHeaderItem("Has Cache", GDSystem.active.HasCache(out _).ToString());
                
            }, UnityEngine.Color.white);

            GUILayoutExt.Box(4f, 4f, () => {

                UnityEngine.GUILayout.Label("Url");
                var newVal = UnityEngine.GUILayout.TextArea(target.url);
                if (newVal != target.url) {
                    
                    target.url = newVal;
                    this.SetDirty();

                }

                UnityEngine.GUILayout.Space(2f);
                
                var newVer = UnityEditor.EditorGUILayout.TextField("Version", target.version);
                if (newVer != target.version) {
                    
                    target.version = newVer;
                    this.SetDirty();

                }

                UnityEngine.GUILayout.Space(10f);

                var newObj = (GDData)UnityEditor.EditorGUILayout.ObjectField("Output", target.data, typeof(GDData), allowSceneObjects: false);
                if (newObj != target.data) {
                    
                    target.data = newObj;
                    this.SetDirty();

                }

                UnityEngine.GUILayout.Space(10f);

                var newShowLogs = UnityEditor.EditorGUILayout.Toggle("Show Logs", target.showLogs);
                if (newShowLogs != target.showLogs) {
                    
                    target.showLogs = newShowLogs;
                    this.SetDirty();

                }

            });
            
            UnityEngine.GUILayout.Space(10f);
            
            if (GDSystem.active == null) {
                
                target.Init();
                GDSystem.active.Use(target.data);
                
            }

            GDSystem.active.showLogs = target.showLogs;

            GUILayoutExt.DrawHeader("Current Data");
            
            UnityEditor.EditorGUI.BeginDisabledGroup(target.IsLoading() == true);
            {
                UnityEngine.GUILayout.BeginHorizontal(UnityEditor.EditorStyles.helpBox);
                {
                    UnityEngine.GUILayout.Label("Loaded keys count: " + GDSystem.active.GetKeysCount());
                    UnityEngine.GUILayout.FlexibleSpace();
                    if (UnityEngine.GUILayout.Button("Update", UnityEngine.GUILayout.Width(60f)) == true) {

                        EditorCoroutines.StartCoroutine(target.UpdateData(GDSystem.active));

                    }
                }
                UnityEngine.GUILayout.EndHorizontal();

                UnityEngine.GUILayout.Space(2f);

                UnityEngine.GUILayout.BeginHorizontal(UnityEditor.EditorStyles.helpBox);
                {
                    UnityEngine.GUILayout.Label("Cache: " + GDSystem.active.HasCache(out var bytes) + " (" + MathUtils.BytesCountToString(bytes) + ")");
                    UnityEngine.GUILayout.FlexibleSpace();
                    if (UnityEngine.GUILayout.Button("Clear", UnityEngine.GUILayout.Width(60f)) == true) {

                        GDSystem.active.ClearCache();

                    }
                }
                UnityEngine.GUILayout.EndHorizontal();
            }
            UnityEditor.EditorGUI.EndDisabledGroup();

        }

    }

}