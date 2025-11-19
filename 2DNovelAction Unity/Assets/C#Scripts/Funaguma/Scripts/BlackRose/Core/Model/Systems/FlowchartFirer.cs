using UnityEngine;

namespace BlackRose.Core.Models.Systems
{
    public sealed class FlowchartFirer : MonoBehaviour
    {
        [SerializeField] private Fungus.Flowchart _flowchart;
        [SerializeField] private string _message = "Rewrite me plz!";
        public void Fire()
        {
            _flowchart.SendFungusMessage(_message);
        }
    }
}