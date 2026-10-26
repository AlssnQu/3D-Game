namespace LoQueCallaste
{
    public interface IReviewInteractable
    {
        bool CanInteract { get; }
        string Hint { get; }
        void Interact();
    }
}
