using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IndianOceanAssets.Engine2_5D.Data
{
    [CreateAssetMenu(fileName = "GameEnemyDatabase", menuName = "MyGame/Enemy Database")]
    public class EnemyDatabase : ScriptableObject
    {
        public List<EnemyDefinition> AllEnemies = new List<EnemyDefinition>();

        // --- AI İÇİN FİLTRELEME ---
        
        /// <summary>
        /// Belirli bir kategoriye ait, bütçeye en uygun düşmanı bulur.
        /// </summary>
        public EnemyDefinition GetEnemyByCategory(EnemyCategory category, float maxCost)
        {
            // 1. Sadece istenen kategoridekileri al
            // 2. Bütçeyi aşanları ele
            // 3. En pahalıdan (güçlüden) ucuza sırala
            // 4. İlkini seç
            return AllEnemies
                .Where(x => x != null && x.Category == category && x.ThreatScore <= maxCost)
                .OrderByDescending(x => x.ThreatScore)
                .FirstOrDefault();
        }

        // --- EDİTÖR ARAÇLARI ---
        [ContextMenu("🔍 Tüm Düşmanları Bul (Auto-Find)")]
        private void FindAllEnemiesInProject()
        {
#if UNITY_EDITOR
            AllEnemies.Clear();
            string[] guids = AssetDatabase.FindAssets("t:EnemyDefinition");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EnemyDefinition enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
                if (enemy != null && !AllEnemies.Contains(enemy)) AllEnemies.Add(enemy);
            }
            // Puanına göre sırala
            AllEnemies = AllEnemies.OrderBy(x => x.ThreatScore).ToList();
            EditorUtility.SetDirty(this);
            Debug.Log($"✅ {AllEnemies.Count} düşman bulundu ve kataloğa eklendi.");
#endif
        }
    }
}