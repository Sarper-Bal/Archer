using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ArcadeBridge.ArcadeIdleEngine.Pools; // ObjectPool'un olduğu namespace

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IndianOceanAssets.Utils
{
    /// <summary>
    /// This script automatically clears all ObjectPools when the game starts or scene reloads.
    /// Use Reflection to invoke the "Clear" method on the private Queue<T> since it doesn't implement a non-generic Clear interface.
    /// 
    /// Bu script oyun başladığında veya sahne yüklendiğinde tüm ObjectPool'ları otomatik olarak temizler.
    /// Queue<T> generic olmayan bir Clear arayüzü sunmadığı için, private Queue üzerinde "Clear" metodunu Reflection ile çağırır.
    /// </summary>
    public static class ObjectPoolCleaner
    {
        // [RuntimeInitializeOnLoadMethod] ensures this runs automatically at the very start of the game.
        // [RuntimeInitializeOnLoadMethod] bu metodun oyunun en başında otomatik çalışmasını sağlar.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetAllPools()
        {
            // Find all ScriptableObjects loaded in memory / Hafızadaki tüm ScriptableObject'leri bul
            ScriptableObject[] allScriptableObjects = Resources.FindObjectsOfTypeAll<ScriptableObject>();

            foreach (var so in allScriptableObjects)
            {
                if (so == null) continue;

                System.Type type = so.GetType();
                
                // Check inheritance chain for ObjectPool<> / ObjectPool<> miras zincirini kontrol et
                while (type != null && type != typeof(object))
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObjectPool<>))
                    {
                        ClearPoolQueue(so, type);
                        break;
                    }
                    type = type.BaseType;
                }
            }
            
            Debug.Log("🧹 [ObjectPoolCleaner] All pools have been cleaned using Reflection. / Tüm havuzlar Reflection kullanılarak temizlendi.");
        }

        private static void ClearPoolQueue(ScriptableObject poolInstance, System.Type poolType)
        {
            // 1. Access private '_pooledObjectQueue' field
            // 1. Private '_pooledObjectQueue' alanına eriş
            FieldInfo queueField = poolType.GetField("_pooledObjectQueue", BindingFlags.NonPublic | BindingFlags.Instance);

            if (queueField != null)
            {
                // Get the Queue object (we don't know T, so it's just 'object')
                // Queue objesini al (T'yi bilmediğimiz için 'object' olarak alıyoruz)
                object queueObject = queueField.GetValue(poolInstance);

                if (queueObject != null)
                {
                    // 2. Find 'Clear' method on the specific Queue<T> type dynamically
                    // 2. O anki Queue<T> tipi üzerindeki 'Clear' metodunu dinamik olarak bul
                    MethodInfo clearMethod = queueObject.GetType().GetMethod("Clear");

                    // 3. Invoke the Clear method
                    // 3. Clear metodunu çalıştır
                    if (clearMethod != null)
                    {
                        clearMethod.Invoke(queueObject, null);
                    }
                }
            }
            
            // 4. Reset '_poolInformer' field to null
            // 4. '_poolInformer' alanını null olarak sıfırla
            FieldInfo informerField = poolType.GetField("_poolInformer", BindingFlags.NonPublic | BindingFlags.Instance);
            if (informerField != null)
            {
                informerField.SetValue(poolInstance, null);
            }
        }
    }
}