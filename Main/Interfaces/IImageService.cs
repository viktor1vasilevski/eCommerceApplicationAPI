namespace Main.Interfaces;

public interface IImageService
{
    byte[] ConvertBase64ToBytes(string base64String);
    string ExtractImageType(string base64String);
}
