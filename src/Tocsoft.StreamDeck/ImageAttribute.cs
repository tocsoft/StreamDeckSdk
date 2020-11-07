using System;

namespace Tocsoft.StreamDeck
{
    public class ImageAttribute : Attribute
    {
        public ImageAttribute(string imagePath)
        {
            ImagePath = imagePath;
        }

        public string ImagePath { get; }
    }

    public class UUIDAttribute : Attribute
    {
        public UUIDAttribute(string uuid)
        {
            UUID = uuid;
        }

        public string UUID { get; }
    }
}
