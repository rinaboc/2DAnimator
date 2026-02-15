using UnityEngine;

namespace Assets.Scripts.Utility
{
    [System.Serializable]
    public class SerializableTexture
    {
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private byte[] data;

        public SerializableTexture(Texture2D texture)
        {
            Data = texture;
        }

        public Texture2D Data
        {
            get
            {
                var tex = new Texture2D(width, height, textureFormat: TextureFormat.RGBA32, false);
                tex.LoadImage(data);
                return tex;
            }
            set
            {
                width = value.width;
                height = value.height;
                data = value.EncodeToPNG();
            }
        }

        public SerializableTexture Clone() => new(Data)
        {
            data = Data.EncodeToPNG(),
            width = Data.width,
            height = Data.height
        };
    }
}
