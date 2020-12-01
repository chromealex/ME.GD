using System.Collections.Generic;

namespace ME.GD {

    [UnityEngine.CreateAssetMenuAttribute(menuName = "ME.GD/Data")]
    public class GDData : UnityEngine.ScriptableObject {

        public string version;
        public List<Item> items = new List<Item>();
        
        internal void Clear() {
            
            this.items.Clear();
            
        }

        internal void Add(Item item) {
            
            this.items.Add(item);
            
        }

        new internal void SetDirty() {
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
            
        }
        
    }

}