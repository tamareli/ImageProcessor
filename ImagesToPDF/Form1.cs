using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImagesToPDF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void search_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browser = new FolderBrowserDialog();
            browser.RootFolder = Environment.SpecialFolder.Desktop;
            browser.Description = "Select Folder";
            DialogResult dr = browser.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string path = browser.SelectedPath;
                Thread thread = new Thread(() => func(path));
                thread.Start();
            }
        }
        public static void func(string path)
        {
            List<string> files = ListOfFiles(path);
            if (files.Count == 0)
            {
                MessageBox.Show("     אין בתקיה זו תמונות      ", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            //int maxWidth = 0, maxHeight = 0;
            Document doc = new Document();//PageSize.A4PageSize.A4
            //for (int i = 0; i < files.Count; i++)
            //{
            //    System.Drawing.Image image = Image(files[i]);
            //    image = RotateImage(image);
            //    if (image.Width > maxWidth)
            //        maxWidth = image.Width;
            //    if (image.Height > maxHeight)
            //        maxHeight = image.Height;
            //}
            var pgSize = new iTextSharp.text.Rectangle(1080, 1550);
            doc.SetPageSize(pgSize);
            using (FileStream stream = new FileStream(path + "/" + DateTime.Now.ToString("dd.MM.yyyy HH-mm-ss") + ".pdf", FileMode.Create))
            {
                PdfWriter writer = PdfWriter.GetInstance(doc, stream);
                doc.Open();
                for (int i = 0; i < files.Count; i++)
                {
                    System.Drawing.Image image = Image(files[i]);
                    image = RotateImage(image);
                    image = ResizeImage2(image,new RectangleF(0,0,1080,720));
                    //image = ResizeImage(image,500,320);
                    //image = resizeImage3(500, 320, image);

                    AddImage(doc, image);        
                }

                doc.Close();
                writer.Close();
            }

        }
      
        public static System.Drawing.Image Image(string file)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(file);
            return image;
        }
        public static List<string> ListOfFiles(string pathOfFile)
        {
            List<string> filesPathes = Directory.GetFiles(pathOfFile).ToList<string>();
            string[] arr = new string[2];
            for (int i = 0; i < filesPathes.Count; i++)
            {
                arr = filesPathes[i].Split('.');
                if (arr[1].ToLower() != "jpg" && arr[1].ToLower() != "jpeg" && arr[1].ToLower() != "png")
                {
                    filesPathes.RemoveAt(i);
                    i--;
                }
            }
            return filesPathes;
        }
        public static System.Drawing.Image RotateImage(System.Drawing.Image image)
        {
            image = Extention.ExifRotate(image);
            return image;
        }
        public static void AddImage(Document doc, System.Drawing.Image image)
        {

                iTextSharp.text.Image iTextImage = iTextSharp.text.Image.GetInstance(image, System.Drawing.Imaging.ImageFormat.Jpeg);
                iTextImage.Alignment = Element.ALIGN_MIDDLE;

            //float width = iTextSharp.text.Utilities.InchesToPoints(15f);
            //float height = iTextSharp.text.Utilities.InchesToPoints(10f);
            //iTextImage.ScaleToFit(width, height);
            doc.Add(iTextImage);           
            doc.Add(new Paragraph(Chunk.NEWLINE));

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
       


        public static System.Drawing.Image  ResizeImage2(System.Drawing.Image source, RectangleF destinationBounds)
        {
            RectangleF sourceBounds = new RectangleF(0.0f, 0.0f, (float)source.Width, (float)source.Height);
            RectangleF scaleBounds = new RectangleF();

            System.Drawing.Image destinationImage = new Bitmap((int)destinationBounds.Width, (int)destinationBounds.Height);
            Graphics graph = Graphics.FromImage(destinationImage);
            graph.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            // Fill with background color
            graph.FillRectangle(new SolidBrush(System.Drawing.Color.White), destinationBounds);

            float resizeRatio, sourceRatio;
            float scaleWidth, scaleHeight;

            sourceRatio = (float)source.Width / (float)source.Height;

            if (sourceRatio >= 1.0f)
            {
                //landscape
                resizeRatio = destinationBounds.Width / sourceBounds.Width;
                scaleWidth = destinationBounds.Width;
                scaleHeight = sourceBounds.Height * resizeRatio;
                float trimValue = destinationBounds.Height - scaleHeight;
                graph.DrawImage(source, 0, (trimValue / 2), destinationBounds.Width, scaleHeight);
            }
            else
            {
                //portrait
                resizeRatio = destinationBounds.Height / sourceBounds.Height;
                scaleWidth = sourceBounds.Width * resizeRatio;
                scaleHeight = destinationBounds.Height;
                float trimValue = destinationBounds.Width - scaleWidth;
                graph.DrawImage(source, (trimValue / 2), 0, scaleWidth, destinationBounds.Height);
            }

            return destinationImage;

        }
    }
   
}

