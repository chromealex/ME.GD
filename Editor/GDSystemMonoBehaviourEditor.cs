
namespace ME.GD.Editor {

    [UnityEditor.CustomEditor(typeof(GDSystemMonoBehaviour))]
    public class GDSystemMonoBehaviourEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {

            this.DrawDefaultInspector();

            var target = this.target as GDSystemMonoBehaviour;

            if (GDSystem.active == null) {
                
                target.Init();
                GDSystem.active.Use(target.data);
                
            }
            
            UnityEngine.GUILayout.Label("Loaded keys count: " + GDSystem.active.GetKeysCount());
            if (UnityEngine.GUILayout.Button("Update") == true) {

                EditorCoroutines.StartCoroutine(target.UpdateData(GDSystem.active));

            }

        }

    }

}