
using System.Linq;

namespace ME.GD.Editor {
    
    using UnityEngine;
    using UnityEditor;

    [UnityEditor.CustomEditor(typeof(GDSystemMonoBehaviour))]
    public class GDSystemMonoBehaviourEditor : UnityEditor.Editor {

        private UnityEditorInternal.ReorderableList.Defaults styleDefaults;
        
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
            
            if (this.styleDefaults == null) {
                
                this.styleDefaults = new UnityEditorInternal.ReorderableList.Defaults();
                
            }

            var target = this.target as GDSystemMonoBehaviour;
            
            GUILayoutExt.DrawComponentHeader(this.serializedObject, "GD", () => {
                
                GUILayoutExt.DrawComponentHeaderItem("Is Ready", target.IsReady().ToString());
                GUILayoutExt.DrawComponentHeaderItem("Is Loading", target.IsLoading().ToString());
                //GUILayoutExt.DrawComponentHeaderItem("Has Cache", GDSystem.active.HasCache(out _).ToString());
                
            }, UnityEngine.Color.white);

            for (int i = 0; i < target.items.Length; ++i) {

                var item = target.items[i];
                {

                    var gdSystem = GDSystem.GetActive(item.index);
                    if (gdSystem == null) {
                
                        target.Init();
                        gdSystem = GDSystem.GetActive(item.index);
                        gdSystem.Use(item.data);
                
                    }

                    GUILayoutExt.Box(4f, 0f, () => {

                        GUILayout.BeginHorizontal();
                        {
                            var newName = UnityEditor.EditorGUILayout.TextField("Name", item.name, GUILayout.ExpandWidth(true));
                            if (newName != item.name) {
                    
                                item.name = newName;
                                this.SetDirty();

                            }
                            
                            if (GUILayout.Button(this.styleDefaults.iconToolbarMinus, this.styleDefaults.preButton, GUILayout.Width(50f), GUILayout.Height(16f)) == true) {

                                var list = target.items.ToList();
                                list.RemoveAt(i);
                                target.items = list.ToArray();
                                this.SetDirty();
                    
                            }
                        }
                        GUILayout.EndHorizontal();

                        UnityEngine.GUILayout.Label("Url");
                        var newVal = UnityEngine.GUILayout.TextArea(item.url);
                        if (newVal != item.url) {
                    
                            item.url = newVal;
                            this.SetDirty();

                        }

                        UnityEngine.GUILayout.Space(2f);
                
                        var newVer = UnityEditor.EditorGUILayout.TextField("Version", item.version);
                        if (newVer != item.version) {
                    
                            item.version = newVer;
                            this.SetDirty();

                        }

                        var newObj = (GDData)UnityEditor.EditorGUILayout.ObjectField("Output", item.data, typeof(GDData), allowSceneObjects: false);
                        if (newObj != item.data) {
                    
                            item.data = newObj;
                            this.SetDirty();

                        }

                        GUILayoutExt.DrawHeader("Current Data");
            
                        UnityEditor.EditorGUI.BeginDisabledGroup(target.IsLoading() == true);
                        {
                            UnityEngine.GUILayout.BeginHorizontal(UnityEditor.EditorStyles.helpBox);
                            {
                                UnityEngine.GUILayout.Label($"Loaded keys count: {gdSystem.GetKeysCount()}");
                                UnityEngine.GUILayout.Label($"Version: {gdSystem.GetVersion()}");
                                UnityEngine.GUILayout.FlexibleSpace();
                                if (UnityEngine.GUILayout.Button("Update", UnityEngine.GUILayout.Width(60f)) == true) {

                                    EditorCoroutines.StartCoroutine(target.UpdateData(target.items, i));

                                }
                            }
                            UnityEngine.GUILayout.EndHorizontal();

                            UnityEngine.GUILayout.Space(2f);

                            UnityEngine.GUILayout.BeginHorizontal(UnityEditor.EditorStyles.helpBox);
                            {
                                UnityEngine.GUILayout.Label($"Cache: {gdSystem.HasCache(out var bytes).ToString()} ({ME.GD.MathUtils.BytesCountToString(bytes)})");
                                UnityEngine.GUILayout.FlexibleSpace();
                                if (UnityEngine.GUILayout.Button("Clear", UnityEngine.GUILayout.Width(60f)) == true) {

                                    gdSystem.ClearCache();

                                }
                            }
                            UnityEngine.GUILayout.EndHorizontal();
                        }
                        UnityEditor.EditorGUI.EndDisabledGroup();

                    });

                }
                target.items[i] = item;

            }
            
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal(this.styleDefaults.footerBackground);
                var rect = GUILayoutUtility.GetRect(50f, 16f);
                if (GUI.Button(rect, this.styleDefaults.iconToolbarPlus, this.styleDefaults.preButton) == true) {

                    var list = target.items.ToList();
                    list.Add(new GDSystemMonoBehaviour.DataItem() {
                        index = target.nextId++,
                    });
                    target.items = list.ToArray();
                    this.SetDirty();
                    
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
            
            UnityEngine.GUILayout.Space(10f);

            var newShowLogs = UnityEditor.EditorGUILayout.Toggle("Show Logs", target.showLogs);
            if (newShowLogs != target.showLogs) {
                    
                target.showLogs = newShowLogs;
                target.SetLogs(newShowLogs);
                this.SetDirty();

            }

            var updateOnStart = UnityEditor.EditorGUILayout.Toggle("Update on Start", target.updateOnStart);
            if (updateOnStart != target.updateOnStart) {
                    
                target.updateOnStart = updateOnStart;
                this.SetDirty();

            }

        }

    }

}