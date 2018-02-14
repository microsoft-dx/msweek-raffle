using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSWeekRaffle.Interfaces
{
    public interface IImageProcessing
    {
        Bitmap GenerateQR(string data);

        string SaveImage(Bitmap image);
    }
}
