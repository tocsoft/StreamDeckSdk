namespace Tocsoft.StreamDeck
{
    /// <summary>
    /// Paramaters for how to layout the title on the button.
    /// </summary>
    public class TitleParamaters
    {
        /// <summary>
        /// the title text
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// whether we should show the title or not
        /// </summary>
        public bool? ShowTitle { get; set; }

        /// <summary>
        /// the color of the title in hex #RRGGBB
        /// </summary>
        public string TitleColor { get; set; }

        /// <summary>
        /// The alignment of the text.
        /// </summary>
        public TitleAlignments? TitleAlignment { get; set; }

        /// <summary>
        /// The texts font.
        /// </summary>
        public FontFamilies? FontFamily { get; set; }

        /// <summary>
        /// the style of the text.
        /// </summary>
        public FontStyles? FontStyle { get; set; }

        /// <summary>
        /// the texts size.
        /// </summary>
        public int? FontSize { get; set; }
    }
}
