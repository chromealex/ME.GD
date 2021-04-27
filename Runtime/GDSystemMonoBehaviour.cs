
namespace ME.GD {

    public class GDSystemMonoBehaviour : UnityEngine.MonoBehaviour {

        [System.Serializable]
        public struct DataItem {

            public string name;
            public int index;
            
            public string url;
            public string version;
            public GDData data;

            internal bool isReady;
            internal bool isLoading;
            
        }

        public DataItem[] items = new DataItem[0];
        public int nextId;

        public bool showLogs;
        public bool updateOnStart;

        public void Awake() {

            UnityEngine.GameObject.DontDestroyOnLoad(this.gameObject);

            this.SetReady(false);
            this.Init();
            if (this.updateOnStart == true) {

                for (int i = 0; i < this.items.Length; ++i) {

                    this.StartCoroutine(this.UpdateData(this.items, i));

                }

            } else {

                this.Use();
                this.SetReady(true);
                
            }
            
        }

        public void SetLogs(bool state) {

            for (int i = 0; i < this.items.Length; ++i) {
                
                GDSystem.GetActive(this.items[i].index).showLogs = state;
                
            }

        }

        public void Init() {

            for (int i = 0; i < this.items.Length; ++i) {

                var gdSystem = new GDSystem() {
                    showLogs = this.showLogs,
                };
                GDSystem.SetActive(gdSystem, this.items[i].index, this.items[i].name);

            }

        }

        internal void Use() {
            
            for (int i = 0; i < this.items.Length; ++i) {

                GDSystem.GetActive(this.items[i].index).Use(this.items[i].data);

            }
            
        }

        internal void SetReady(bool state) {

            for (int i = 0; i < this.items.Length; ++i) {

                this.items[i].isReady = state;

            }
            
        }

        internal void SetLoading(bool state) {

            for (int i = 0; i < this.items.Length; ++i) {

                this.items[i].isLoading = state;

            }
            
        }

        public bool IsReady() {

            for (int i = 0; i < this.items.Length; ++i) {

                if (this.items[i].isReady == false) return false;

            }
            
            return true;

        }

        public bool IsLoading() {

            for (int i = 0; i < this.items.Length; ++i) {

                if (this.items[i].isLoading == false) return false;

            }
            
            return true;

        }

        public System.Collections.IEnumerator UpdateData(DataItem[] items, int index) {

            if (items[index].data == null) {
                
                if (this.showLogs == true) UnityEngine.Debug.LogWarning("[ME.GD] Data output is null. Please check `data` link on GD GameObject.");
                yield break;
                
            }

            var gdSystem = GDSystem.GetActive(items[index].index);
            if (gdSystem.ApplyCache(items[index].version, items[index].data) == true) {
                
                if (this.showLogs == true) UnityEngine.Debug.Log($"[ME.GD] Cache read successfully from {gdSystem.GetCachePath()}");
                try {

                    gdSystem.Use(items[index].data);
                    items[index].isReady = true;

                } catch (System.Exception ex) {
                    
                    UnityEngine.Debug.LogException(ex);
                    
                }
                
            }
            
            items[index].isLoading = true;

            var url = items[index].url.Replace("{streaming_assets}", UnityEngine.Application.streamingAssetsPath);
            yield return gdSystem.DownloadAndUpdate(url, items[index].version, items[index].data, (result) => {

                try {

                    gdSystem.Use(items[index].data);
                    items[index].isReady = true;

                } catch (System.Exception ex) {
                    
                    UnityEngine.Debug.LogException(ex);
                    
                }

                if (result == false) {
                    
                    if (this.showLogs == true) UnityEngine.Debug.LogError($"Failed to load {items[index].url}, version: {items[index].version}");
                    
                } else {
                    
                    if (this.showLogs == true) UnityEngine.Debug.Log($"[ME.GD] Loaded {items[index].name} by index {items[index].index}");
                    
                }
                
                items[index].isLoading = false;
                
            });

        }

    }

}