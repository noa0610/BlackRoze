using BlackRose.Core.Models.Units;
using UnityEditor;
using UnityEngine;

namespace BlackRose.Datas.Definitions
{
    [CreateAssetMenu(menuName = "BlackRose/BulletData")]
    public class BulletData : ScriptableObject
    {
        public string bulletName;
        public Bullet prefab;
        public BulletStatus originalstatus;

#if UNITY_EDITOR
        private void OnValidate()
        {
            //if (!string.IsNullOrEmpty(bulletName))
            //{
            //    // アセット内部の名前更新
            //    this.name = bulletName;

            //    // 実際のファイル名も同期
            //    string path = AssetDatabase.GetAssetPath(this);
            //    string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

            //    // 現在のファイル名とbulletNameが違う場合だけ変更
            //    if (!string.IsNullOrEmpty(path) && fileName != bulletName)
            //    {
            //        string newPath = System.IO.Path.GetDirectoryName(path) + "/" + bulletName + ".asset";
            //        AssetDatabase.RenameAsset(path, bulletName);
            //        AssetDatabase.SaveAssets();
            //        AssetDatabase.Refresh();
            //    }
            //}
        }
#endif
    }
}