using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Sıralama işlemleri için

namespace IndianOceanAssets.Engine2_5D.Data
{
    [CreateAssetMenu(fileName = "GameEnemyDatabase", menuName = "MyGame/Enemy Database")]
    public class EnemyDatabase : ScriptableObject
    {
        [Header("📁 Düşman Kataloğu")]
        [Tooltip("Düşman Definition dosyalarını buraya sürükle.")]
        public List<EnemyDefinition> AllEnemies = new List<EnemyDefinition>();

        // [BUTON YERİNE SAĞ TIK MENÜSÜ]
        // Bu scriptin Inspector başlığına sağ tıklayarak bu fonksiyonları çalıştırabilirsin.
        
        [ContextMenu("Sort Enemies (Easy -> Hard)")]
        private void SortByThreatAscending()
        {
            // Puanı düşükten yükseğe sırala
            AllEnemies = AllEnemies.OrderBy(x => x != null ? x.ThreatScore : 0).ToList();
            
#if UNITY_EDITOR
            // Değişikliği kaydet (Unity editörüne "bu dosya değişti" de)
            UnityEditor.EditorUtility.SetDirty(this); 
#endif
            Debug.Log("✅ Düşmanlar KOLAYDAN ZORA sıralandı.");
        }

        [ContextMenu("Sort Enemies (Hard -> Easy)")]
        private void SortByThreatDescending()
        {
            // Puanı yüksekten düşüğe sırala
            AllEnemies = AllEnemies.OrderByDescending(x => x != null ? x.ThreatScore : 0).ToList();
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            Debug.Log("🔥 Düşmanlar ZORDAN KOLAYA sıralandı.");
        }
        
        /// <summary>
        /// Belirli bir bütçeye uygun en güçlü düşmanı bulur (AI Director için hazırlık)
        /// </summary>
        public EnemyDefinition GetEnemyByCost(float maxCost)
        {
            // Bütçemi aşmayanlar arasında, bütçeme en yakın olanı (en güçlüsünü) ver.
            return AllEnemies
                .Where(x => x.ThreatScore <= maxCost)
                .OrderByDescending(x => x.ThreatScore)
                .FirstOrDefault();
        }
    }
}