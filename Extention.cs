using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace ImagesToPDF
{
    public static class  Extention
    {
      private const int exifOrientationID = 0x112; //274
    public static Image ExifRotate(this System.Drawing.Image img)
    {
           // for images with no exif information
    
            var rot2 = RotateFlipType.RotateNoneFlipNone;
            if (img.Width < img.Height)
            {
                rot2 = RotateFlipType.Rotate90FlipNone;
                img.RotateFlip(rot2);
                return img;
            }
            if (!img.PropertyIdList.Contains(exifOrientationID))
            return img;
        var prop = img.GetPropertyItem(exifOrientationID);
        int val = BitConverter.ToUInt16(prop.Value, 0);
        var rot = RotateFlipType.RotateNoneFlipNone;

        if (val == 3 || val == 4)
            rot = RotateFlipType.Rotate180FlipNone;
         if (val == 5 || val == 6)
            rot = RotateFlipType.Rotate180FlipNone;
        //else if (val == 7 || val == 8)
            //rot = RotateFlipType.Rotate180FlipNone;
        if (val == 2 || val == 4 || val == 5 || val == 7)
            rot |= RotateFlipType.RotateNoneFlipX;

        if (rot != RotateFlipType.RotateNoneFlipNone)
            img.RotateFlip(rot);
            return img;
    }

    }
}
