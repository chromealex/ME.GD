
namespace ME.GD {

    public class GDSystemMonoBehaviour : UnityEngine.MonoBehaviour {

        public string url;
        public string version;
        public GDData data;
        private bool isReady;
        private bool isLoading;
        public bool showLogs;

        public void Awake() {

            UnityEngine.GameObject.DontDestroyOnLoad(this.gameObject);

            this.isReady = false;
            this.Init();
            this.StartCoroutine(this.UpdateData(GDSystem.active));
            
        }

        public bool IsReady() {

            return this.isReady;

        }

        public bool IsLoading() {

            return this.isLoading;

        }

        public void Init() {
            
            var gdSystem = new GDSystem() {
                showLogs = this.showLogs
            };
            GDSystem.SetActive(gdSystem);

        }
        
        public System.Collections.IEnumerator UpdateData(GDSystem gdSystem) {

            if (this.data == null) {
                
                if (this.showLogs == true) UnityEngine.Debug.LogWarning("[ME.GD] Data output is null. Please check `data` link on GD GameObject.");
                yield break;
                
            }

            if (gdSystem.ApplyCache(this.version, this.data) == true) {
                
                if (this.showLogs == true) UnityEngine.Debug.Log("[ME.GD] Cache read successfully");
                try {

                    gdSystem.Use(this.data);
                    this.isReady = true;

                } catch (System.Exception ex) {
                    
                    UnityEngine.Debug.LogException(ex);
                    
                }
                
            }
            
            this.isLoading = true;

            var url = this.url.Replace("{streaming_assets}", UnityEngine.Application.streamingAssetsPath);
            yield return gdSystem.DownloadAndUpdate(url, this.version, this.data, (result) => {

                try {

                    gdSystem.Use(this.data);
                    this.isReady = true;

                } catch (System.Exception ex) {
                    
                    UnityEngine.Debug.LogException(ex);
                    
                }

                if (result == false) {
                    
                    if (this.showLogs == true) UnityEngine.Debug.LogError("Failed to load " + this.url + ", version: " + this.version);
                    
                }
                
                this.isLoading = false;
                
            });

        }

    }

}