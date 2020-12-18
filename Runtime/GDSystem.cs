using System.Collections.Generic;

namespace ME.GD {

    using Parsers;
    
    public enum GDValueType {

        Unknown = 0,
        String,
        Float,
        Integer,
        Enum,

    }

    [System.Serializable]
    public struct Item {

        public string key;
        public GDValueType type;
        public string s;
        public float f;
        public int i;

    }

    public interface IGDEnum {}
    
    [System.Serializable]
    public struct GDEnum<T> : IGDEnum where T : struct, System.IConvertible {

        public string key;
        [System.NonSerialized]
        public T runtimeValue;
        [System.NonSerialized]
        public bool runtimeValueSet;

        public static TEnum Get<TEnum>(GDEnum<TEnum> gdEnum) where TEnum : struct, System.IConvertible {
            
            if (gdEnum.runtimeValueSet == true) return gdEnum.runtimeValue;

            var gdKey = new GDKey() { key = gdEnum.key };
            if (GDSystem.active.Get(gdKey, out string val) == true) {

                ulong valOut = default;
                var split = val.Split('|');
                //UnityEngine.Debug.Log(val + " :: " + typeof(TEnum));
                for (int i = 0; i < split.Length; ++i) {

                    var r = (TEnum)System.Enum.Parse(typeof(TEnum), split[i]);
                    var rVal = System.Runtime.CompilerServices.Unsafe.As<TEnum, ulong>(ref r);
                    //UnityEngine.Debug.Log(valOut + " :: " + rVal + " :: " + r);
                    valOut |= rVal;

                }
                
                //UnityEngine.Debug.Log(valOut);
                var res = System.Runtime.CompilerServices.Unsafe.As<ulong, TEnum>(ref valOut);
                gdEnum.Set(res);
                return res;

            }

            if (GDSystem.active.Get(gdKey, out int valInt) == true) {
                var res = System.Runtime.CompilerServices.Unsafe.As<int, TEnum>(ref valInt);
                gdEnum.Set(res);
                return res;
            }
            return default;

        }
        
        public string GetRuntimeValueStr() {

            if (this.runtimeValueSet == true) return this.runtimeValue.ToString();
            
            return null;

        }

        public T Get() {

            return GDEnum<T>.Get(this);

        }
        
        public void Set(T value) {

            this.runtimeValue = value;
            this.runtimeValueSet = true;

        }

        public static implicit operator T(GDEnum<T> key) {

            return key.Get();

        }

    }

    [System.Serializable]
    public struct GDInt {

        public string key;
        [System.NonSerialized]
        public int runtimeValue;
        [System.NonSerialized]
        public bool runtimeValueSet;

        public int Get() {

            if (this.runtimeValueSet == true) return this.runtimeValue;

            if (GDSystem.active.Get(this, out int val) == true) {
                this.Set(val);
                return val;
            }
            return default;

        }
        
        public string GetRuntimeValueStr() {

            if (this.runtimeValueSet == true) return this.runtimeValue.ToString();
            
            return null;

        }

        public void Set(int value) {

            this.runtimeValue = value;
            this.runtimeValueSet = true;

        }

        public static implicit operator float(GDInt key) {

            return key.Get();

        }

        public static implicit operator int(GDInt key) {

            return key.Get();

        }

    }

    [System.Serializable]
    public struct GDString {
        
        public string key;
        [System.NonSerialized]
        public string runtimeValue;
        [System.NonSerialized]
        public bool runtimeValueSet;

        public string Get() {

            if (this.runtimeValueSet == true) return this.runtimeValue;

            if (GDSystem.active.Get(this, out string val) == true) {
                this.Set(val);
                return val;
            }
            return default;

        }
        
        public string GetRuntimeValueStr() {

            if (this.runtimeValueSet == true) return this.runtimeValue.ToString();
            
            return null;

        }

        public void Set(string value) {

            this.runtimeValue = value;
            this.runtimeValueSet = true;

        }

        public static implicit operator string(GDString key) {

            return key.Get();

        }

    }
    
    [System.Serializable]
    public struct GDFloat {
        
        public string key;
        [System.NonSerialized]
        public float runtimeValue;
        [System.NonSerialized]
        public bool runtimeValueSet;

        public float Get() {

            if (this.runtimeValueSet == true) return this.runtimeValue;
            
            if (GDSystem.active.Get(this, out float val) == true) {
                this.Set(val);
                return val;
            }
            return default;

        }

        public string GetRuntimeValueStr() {

            if (this.runtimeValueSet == true) return this.runtimeValue.ToString();
            
            return null;

        }

        public void Set(float value) {

            this.runtimeValue = value;
            this.runtimeValueSet = true;

        }
        
        public static implicit operator float(GDFloat key) {

            return key.Get();
            
        }

    }
    
    [System.Serializable]
    public struct GDKey {

        public string key;
        [System.NonSerialized]
        public string runtimeValue;

        public float GetFloat() {

            if (GDSystem.active.Get(this, out float val) == true) return val;
            return default;

        }

        public int GetInt() {

            if (GDSystem.active.Get(this, out int val) == true) return val;
            return default;

        }

        public string GetString() {

            if (GDSystem.active.Get(this, out string val) == true) return val;
            return default;

        }

        public static implicit operator float(GDKey key) {

            return key.GetFloat();

        }

        public static implicit operator int(GDKey key) {

            return key.GetInt();

        }

        public static implicit operator string(GDKey key) {

            return key.GetString();

        }

    }

    public class GDSystem {

        public static GDSystem active;
        private Dictionary<string, Item> lines = new Dictionary<string, Item>();
        private GDData data;
        public bool showLogs;

        public static void SetActive(GDSystem system) {

            GDSystem.active = system;

        }

        public GDData GetData() {

            return this.data;

        }

        public int GetKeysCount() {

            if (this.data == null) return 0;
            return this.data.items.Count;

        }

        public bool Get(GDInt key, out int value, bool forced = false) {

            return this.Get(new GDKey() { key = key.key }, out value, forced);

        }

        public bool Get(GDFloat key, out float value, bool forced = false) {

            return this.Get(new GDKey() { key = key.key }, out value, forced);

        }

        public bool Get(GDString key, out string value, bool forced = false) {

            return this.Get(new GDKey() { key = key.key }, out value, forced);

        }

        public bool Get(GDKey key, out float value, bool forced = false) {

            value = default;
            if (string.IsNullOrEmpty(key.key) == true) {

                return false;
                
            }
            
            if (this.lines.TryGetValue(key.key, out var item) == true && ((item.type == GDValueType.Float || item.type == GDValueType.Integer) || forced == true)) {

                value = item.f;
                return true;

            }

            return false;

        }

        public bool Get(GDKey key, out int value, bool forced = false) {
            
            value = default;
            if (string.IsNullOrEmpty(key.key) == true) {

                return false;
                
            }
            
            if (this.lines.TryGetValue(key.key, out var item) == true && (item.type == GDValueType.Integer || forced == true)) {

                value = item.i;
                return true;

            }

            if (this.Get(key, out float val, forced) == true) {

                value = (int)val;
                return true;

            }

            return false;

        }

        public bool Get(GDKey key, out string value, bool forced = false) {

            value = default;
            if (string.IsNullOrEmpty(key.key) == true) {

                return false;
                
            }
            
            if (this.lines.TryGetValue(key.key, out var item) == true && (item.type == GDValueType.String || forced == true)) {

                value = item.s;
                return true;

            }

            return false;

        }

        public void Use(GDData data) {

            this.data = data;

            this.lines.Clear();
            for (int i = 0; i < data.items.Count; ++i) {

                var item = data.items[i];
                this.lines.Add(item.key, item);

            }

        }
        
        public void Update(string data, string version, GDData output) {
            
            var reader = new CsvReader(new System.IO.StringReader(data), ",");
            var line = 0;
            var versionIndex = -1;
            while (reader.Read() == true) {

                ++line;
                if (line == 1) {

                    var fileVersionInt = 0;
                    var fileVersion = reader[0];
                    if (string.IsNullOrEmpty(fileVersion) == false) {

                        int.TryParse(fileVersion, out fileVersionInt);

                    }

                    if (fileVersionInt <= output.fileVersion && version == output.version) {

                        if (this.showLogs == true) UnityEngine.Debug.Log("File " + output + " is up to date already, version: " + fileVersionInt);
                        return;

                    }

                    output.Clear();
                    output.fileVersion = fileVersionInt;
                    
                    // check version
                    for (int i = 1; i < reader.FieldsCount; ++i) {

                        var ver = reader[i];
                        if (ver == version) {

                            versionIndex = i;
                            break;

                        }

                    }

                    if (versionIndex == -1) return;
                    
                    continue;
                    
                }
                
                var key = reader[0];
                var value = reader[versionIndex];

                var style = System.Globalization.NumberStyles.Number;
                var item = new Item() {
                    key = key,
                    type = GDValueType.String,
                    s = value,
                };
                if (float.TryParse(value, style, System.Globalization.CultureInfo.InvariantCulture, out item.f) == true) item.type = GDValueType.Float;
                if (int.TryParse(value, style, System.Globalization.CultureInfo.InvariantCulture, out item.i) == true) item.type = GDValueType.Integer;

                output.Add(item);

            }

            output.version = version;
            output.SetDirty();

            if (this.showLogs == true) UnityEngine.Debug.Log("[ME.GD] Updated version: " + output.fileVersion + ". Done");

        }

        public bool HasCache(out int bytesCount) {

            bytesCount = 0;
            var cachePath = this.GetCachePath();
            if (System.IO.File.Exists(cachePath) == true) {

                bytesCount = System.IO.File.ReadAllBytes(cachePath).Length;
                return true;
                
            }

            return false;

        }

        public void ClearCache() {

            if (this.HasCache(out _) == true) {
                
                var cachePath = this.GetCachePath();
                System.IO.File.Delete(cachePath);
                
            }
            
        }

        public bool ApplyCache(string version, GDData data) {
            
            var cachePath = this.GetCachePath();
            if (System.IO.File.Exists(cachePath) == true) {

                var fileData = System.IO.File.ReadAllText(cachePath);
                this.Update(fileData, version, data);
                return true;
                
            }

            return false;

        }
        
        private string GetCachePath() {
            
            return $"{UnityEngine.Application.persistentDataPath}/ME.GD/DataCache.mec";
            
        }
        
        public System.Collections.IEnumerator DownloadAndUpdate(string url, string version, GDData data, System.Action<bool> onComplete) {

            var request = UnityEngine.Networking.UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            while (request.isDone == false) yield return null;

            var hasError = (request.isNetworkError == true || request.isHttpError == true);
            if (hasError == false) {

                { // Write cache
                    var cachePath = this.GetCachePath();
                    var dir = System.IO.Path.GetDirectoryName(cachePath);
                    if (string.IsNullOrEmpty(dir) == false && System.IO.Directory.Exists(dir) == false) System.IO.Directory.CreateDirectory(dir);
                    System.IO.File.WriteAllText(cachePath, request.downloadHandler.text);
                }
                
                this.Update(request.downloadHandler.text, version, data);
                
            }
            onComplete.Invoke(hasError == false);

        }
        
    }

}
