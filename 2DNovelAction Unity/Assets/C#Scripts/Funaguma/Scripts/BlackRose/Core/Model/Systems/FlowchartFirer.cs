using UnityEngine;

namespace BlackRose.Core.Models.Systems
{
    public sealed class FlowchartFirer : MonoBehaviour
    {
        [SerializeField] private Fungus.Flowchart _flowchart;
        [SerializeField] private string _message = "Rewrite me plz!";
        [SerializeField] private string _flowchartName = "EP";
        public void Fire()
        {
            if (_flowchart)
            {
                _flowchart.SendFungusMessage(_message);
            }
            else
            {
                Debug.Log("Flowchartの指定がありません。Flowchartをオブジェクト名で探します。");
                _flowchart = GameObject.Find(_flowchartName).GetComponent<Fungus.Flowchart>();
                if (_flowchart)
                {
                    _flowchart.SendFungusMessage(_message);
                }
                else
                {
                    Debug.LogError("撃破時に呼び出すFlowchartのオブジェクト名を指定してください。");
                }
            }
        }
    }
}