namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// what device to targets
    /// </summary>
    public enum SetValueTarget
    {
        /// <summary>
        /// both hardware and software
        /// </summary>
        Both = 0,
        /// <summary>
        /// show the change on the device
        /// </summary>
        Hardware = 1,

        /// <summary>
        /// show the change in the GUI
        /// </summary>
        Software = 2,
    }
}
