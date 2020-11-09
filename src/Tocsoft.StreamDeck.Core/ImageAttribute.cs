using System;

namespace Tocsoft.StreamDeck
{
    /// <summary>
    /// Allows setting the image without using the configuration apis.
    /// </summary>
    public class ImageAttribute : Attribute
    {
        /// <summary>
        /// create a ImageAttribute 
        /// </summary>
        /// <param name="imagePath">the path to the image</param>
        public ImageAttribute(string imagePath)
        {
            ImagePath = imagePath;
        }

        /// <summary>
        /// The path to the image
        /// </summary>
        public string ImagePath { get; }
    }

    /// <summary>
    /// Allows overriding the UUID used by the action
    /// </summary>
    public class UUIDAttribute : Attribute
    {
        /// <summary>
        /// define the uuid
        /// </summary>
        /// <param name="uuid"></param>
        public UUIDAttribute(string uuid)
        {
            UUID = uuid;
        }

        /// <summary>
        /// The UUID to use
        /// </summary>
        public string UUID { get; }
    }
}
