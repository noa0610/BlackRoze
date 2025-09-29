namespace BlackRose.Core.Models.States
{
    public interface IGameObjectUser
    {
        void SetGameObject(UnityEngine.GameObject go, params UnityEngine.GameObject[] options);
    }
}