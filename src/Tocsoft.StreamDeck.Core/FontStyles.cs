using System.ComponentModel;

namespace Tocsoft.StreamDeck
{
    /// <summary>
    /// The varous font styles titles support
    /// </summary>
    public enum FontStyles
    {
        /// <summary>
        /// Render using regualr style
        /// </summary>
        Regular,
        
        /// <summary>
        /// Render as bold
        /// </summary>
        Bold,
        
        /// <summary>
        /// Render in italic
        /// </summary>
        Italic,

        /// <summary>
        /// render with both bold and italic simultatiously
        /// </summary>
        [Description("Bold Italic")] BoldItalic,
    }
}
