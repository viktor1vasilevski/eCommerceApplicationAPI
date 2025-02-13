using Main.Interfaces;

namespace Main.Services;

public class ImageService : IImageService
{
    public byte[] ConvertBase64ToBytes(string base64String)
    {
        if (string.IsNullOrEmpty(base64String)) return null;

        string base64Data = base64String.Contains("base64,")
            ? base64String.Substring(base64String.IndexOf("base64,") + 7)
            : base64String;

        return Convert.FromBase64String(base64Data);
    }

    public string ExtractImageType(string base64String)
    {
        if (string.IsNullOrEmpty(base64String)) return null;

        var imageData = base64String.Split(";");
        var imageType = imageData[0].Split("/");

        if (imageType[1] == "jpeg")
        {
            return "jpg";
        }

        return imageType[1];
    }
}
