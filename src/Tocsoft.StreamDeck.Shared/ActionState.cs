using System.ComponentModel;

namespace Tocsoft.StreamDeck
{
    public enum ActionState
    {
        Default = 0,
        Alternative = 1
    }

    public class TitleParamaters
    {
        public string Title { get; set; }

        public bool? ShowTitle { get; set; }

        public string TitleColor { get; set; }

        public TitleAlignments? TitleAlignment { get; set; }

        public FontFamilies? FontFamily { get; set; }

        public FontStyles? FontStyle { get; set; }

        public int? FontSize { get; set; }
    }

    public enum TitleAlignments
    {
        Top = 0,
        Bottom = 1,
        Middle = 1,
    }
    public enum FontStyles
    {
        Regular,
        Bold,
        Italic,
        [Description("Bold Italic")] BoldItalic,
    }

    public enum FontFamilies
    {
        [Description("Arial")] Arial,
        [Description("Arial Black")] ArialBlack,
        [Description("Comic Sans MS")] ComicSansMS,
        [Description("Courier")] Courier,
        [Description("Courier New")] CourierNew,
        [Description("Georgia")] Georgia,
        [Description("Impact")] Impact,
        [Description("Microsoft Sans Serif")] MicrosoftSansSerif,
        [Description("Symbol")] Symbol,
        [Description("Tahoma")] Tahoma,
        [Description("Times New Roman")] TimesNewRoman,
        [Description("Trebuchet MS")] TrebuchetMS,
        [Description("Verdana")] Verdana,
        [Description("Webdings")] Webdings,
        [Description("Wingdings")] Wingdings
    }
}
