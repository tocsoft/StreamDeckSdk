namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// the location of the eciotn as positioned on the device
    /// </summary>
    public class ActionLocation
    {
        /// <summary>
        /// The column the action is in
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// The row the action is in.
        /// </summary>
        public int Row { get; set; }
    }

    /// <summary>
    /// The size of a device.
    /// </summary>
    public class DeviceSize
    {
        /// <summary>
        /// The number of columns the device displays
        /// </summary>
        public int Columns { get; set; }

        /// <summary>
        /// The number of rows the device displays
        /// </summary>
        public int Rows{ get; set; }
    }
}
