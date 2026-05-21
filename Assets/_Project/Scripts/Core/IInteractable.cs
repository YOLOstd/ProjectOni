namespace ProjectOni.Core
{
    /// <summary>
    /// Interface for any object that the player can interact with (e.g., pickups, NPCs, levers).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Logic to execute when the player interacts with this object.
        /// </summary>
        void Interact(UnityEngine.GameObject interactor);

        /// <summary>
        /// Text to display in the UI when the player is near this object.
        /// </summary>
        string GetInteractionText();
    }
}
