using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web.Hosting;
using MessagingToolkit.QRCode.Codec;
using MSWeekRaffle.Interfaces;

namespace MSWeekRaffle.Services
{
    public class MessageToolkitImageProcessing : IImageProcessing
    {
        public Bitmap GenerateQR(string data)
        {
            var encoder = new QRCodeEncoder();

            return encoder.Encode(data);
        }

        public string SaveImage(Bitmap image)
        {
            var imageName = Guid.NewGuid();
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 25L);

            image.Save(
                this.GetServerPath(imageName),
                this.GetEncoderInfo("image/jpeg"),
                encoderParameters);

            return string.Format(
                "{0}/Pictures/{1}.jpg",
                ConfigurationManager.AppSettings["BASE_URL"],
                imageName);
        }

        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            return ImageCodecInfo
                .GetImageEncoders()
                .Where(x => x.MimeType == mimeType)
                .FirstOrDefault();
        }

        private string GetServerPath(Guid imageName)
        {
            var imagePath = string.Format("~/Pictures/{0}.jpg", imageName);

            return HostingEnvironment.MapPath(imagePath);
        }
    }
}
