using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor; // Sadece editörde çalışması için gerekli
#endif

namespace IndianOceanAssets.Engine2_5D.Data
{
    [CreateAssetMenu(fileName = "GameEnemyDatabase", menuName = "MyGame/Enemy Database")]
    public class EnemyDatabase : ScriptableObject
    {
        [Header("📁 Düşman Kataloğu")]
        [Tooltip("Bu liste otomatik doldurulabilir veya elle düzenlenebilir.")]
        public List<EnemyDefinition> AllEnemies = new List<EnemyDefinition>();

        // --- SAĞ TIK MENÜSÜ İLE ÇALIŞAN FONKSİYONLAR ---

        [ContextMenu("🔍 Tüm Düşmanları Bul (Auto-Find)")]
        private void FindAllEnemiesInProject()
        {
#if UNITY_EDITOR
            AllEnemies.Clear();
            
            // Projedeki tüm EnemyDefinition tipindeki dosyaların ID'lerini bul
            string[] guids = AssetDatabase.FindAssets("t:EnemyDefinition");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EnemyDefinition enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
                
                if (enemy != null && !AllEnemies.Contains(enemy))
                {
                    AllEnemies.Add(enemy);
                }
            }
            
            // Bulduktan sonra otomatik sırala
            SortByThreatAscending();
            
            Debug.Log($"✅ Otomatik Tarama Tamamlandı: {AllEnemies.Count} düşman bulundu ve eklendi.");
            EditorUtility.SetDirty(this); // Kaydet
#endif
        }

        [ContextMenu("Puanına Göre Sırala (Kolay -> Zor)")]
        private void SortByThreatAscending()
        {
            // ThreatScore'a göre sırala
            AllEnemies = AllEnemies.OrderBy(x => x != null ? x.ThreatScore : 0).ToList();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            Debug.Log("📊 Düşmanlar KOLAYDAN ZORA sıralandı.");
        }

        // --- OYUN İÇİ KULLANIM (AI Director Burayı Kullanacak) ---
        
        /// <summary>
        /// Bütçeye uygun en güçlü düşmanı verir.
        /// </summary>
        public EnemyDefinition GetEnemyByCost(float maxCost)
        {
            // Bütçeyi aşmayan en yüksek puanlı düşmanı seç
            return AllEnemies
                .Where(x => x != null && x.ThreatScore <= maxCost)
                .OrderByDescending(x => x.ThreatScore)
                .FirstOrDefault();
        }
    }
}