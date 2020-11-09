namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// The set of known devices
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// a standard streamdeck (3x5)
        /// </summary>
        StreamDeck = 0,

        /// <summary>
        /// a streamdeck mini (2x3)
        /// </summary>
        StreamDeckMini = 1,
        
        /// <summary>
        /// a streamdeck XL (4x8)
        /// </summary>
        StreamDeckXL = 2,

        /// <summary>
        /// the stream deck mobile client
        /// </summary>
        StreamDeckMobile = 3,

        /// <summary>
        /// Corsair G Keys
        /// </summary>
        CorsairGKeys = 4
    }
}
