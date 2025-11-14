using UnityEngine;

namespace BlackRose.Core.Models.Units
{
	/// <summary>
	/// シールド（Inspector向けラッパー）
	/// - 実処理は SieldBlock_Base に委譲
	/// - 既存のシーン/プレハブで `SieldBlock` を参照している場合でも置き換え可能
	/// </summary>
	public class SieldBlock : SieldBlock_Base
	{
		[Header("Inspector設定（子クラスから変更可）")]
		[SerializeField] private float inspectorDamageScale = 1f;
		[SerializeField] private float inspectorStunDuration = 0.5f;

		protected override void Awake()
		{
			base.Awake();
			// ベースの保護フィールドへ反映
			damageScale = inspectorDamageScale;
			tackleStunDuration = inspectorStunDuration;
		}
	}
}

