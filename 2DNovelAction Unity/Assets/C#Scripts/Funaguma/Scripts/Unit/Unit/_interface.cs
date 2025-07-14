namespace BlackRose
{
    public interface IPlayable
    {
        void InputReject();
        void AllowedInput();
    }
    public interface IPausable
    {
        void Pause();
        void Play();
    }
}
// unicode