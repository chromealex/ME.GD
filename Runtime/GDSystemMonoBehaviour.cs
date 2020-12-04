
namespace ME.GD {

    public class GDSystemMonoBehaviour : UnityEngine.MonoBehaviour {

        public string url;
        public string version;
        public GDData data;
        private bool isReady;

        public void Awake() {

            UnityEngine.GameObject.DontDestroyOnLoad(this.gameObject);

            this.isReady = false;
            this.Init();
            this.StartCoroutine(this.UpdateData(GDSystem.active));
            
        }

        public bool IsReady() {

            return this.isReady;

        }

        public void Init() {
            
            var gdSystem = new GDSystem();
            GDSystem.SetActive(gdSystem);

        }
        
        public System.Collections.IEnumerator UpdateData(GDSystem gdSystem) {

            if (this.data == null) {
                
                UnityEngine.Debug.LogWarning("[ME.GD] Data output is null. Please check `data` link on GD GameObject.");
                yield break;
                
            }
            
            var url = this.url.Replace("{streaming_assets}", UnityEngine.Application.streamingAssetsPath);
            yield return gdSystem.DownloadAndUpdate(url, this.version, this.data, (result) => {

                if (result == true) {
                    
                    gdSystem.Use(this.data);
                    this.isReady = true;
                    
                } else {
                    
                    UnityEngine.Debug.LogError("Failed to load " + this.url + ", version: " + this.version);
                    
                }
                
            });

        }

    }

}