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
        //float pageHeight, pageWidth;
        float selectedHeightOfImg, selectedWidthOfImg;//in mm
        float pageHeight, pageWidth;//in mm
        float marginTopBottom = iTextSharp.text.Utilities.MillimetersToPoints(5f);
        float marginLeftRight = iTextSharp.text.Utilities.MillimetersToPoints(5f);

        public Form1()
        {
            InitializeComponent();
        }

        private void search_Click(object sender, EventArgs e)
        {
            if(radioButton4.Checked)
            {
                if (numericUpDown1.Value == 0)
                {
                    MessageBox.Show("הכנס ערך שונה מ-0 לאורך", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (numericUpDown2.Value == 0)
                {
                    MessageBox.Show("הכנס ערך שונה מ-0 לרוחב", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }


            //select images
            try
            {
                using (OpenFileDialog open = new OpenFileDialog())
                {
                    open.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    open.Filter = "Image files |*.jpg;*.jpeg;*.tif;*.tiff;*.png|Custom type files|*.crx|All files|*.*";
                    open.Multiselect = true;
                    if (open.ShowDialog() == DialogResult.OK)
                    {

                        string[] paths = open.FileNames;
                        Thread thread = new Thread(() => mainProcess(paths));
                        thread.Start();
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
           
        }
       public int CalcWidthAccordingRatio(int ratioHeight,int ratioWidth,int sourceHeight)
        {
            float newWidth = (((float)sourceHeight / (float)ratioHeight)) * ratioWidth;
            return Convert.ToInt32(newWidth)+1;
        }
        public string GetFolderPath(string pdfPath)
        {
            string[] pdfPaths = pdfPath.Split('\\');
            pdfPath = "";
            for (int i = 0; i < pdfPaths.Length - 1; i++)
            {
                pdfPath += pdfPaths[i] + '\\';
            }
            return pdfPath;
        }

        public void mainProcess(string[] paths)
        {
            List<string> files = paths.ToList<string>();
            string pdfPath = SavedPath();
            if (pdfPath == "")
                return;

            Document doc = new Document();
            float width = getPageWidthInPoints();
            float height = getPageHeightInPoints();
            var pgSize = new iTextSharp.text.Rectangle(width, height);
            doc.SetPageSize(pgSize);
            string pdfName = getFileName(pdfPath);
            using (FileStream stream = new FileStream(pdfPath, FileMode.Create))
            {
                PdfWriter writer = PdfWriter.GetInstance(doc, stream);

                writer.Open();
                doc.Open();
                for (int i = 0; i < files.Count - 1; i++)
                {
                    drawImage(doc, writer, files[i]);
                    doc.NewPage();
                }
                
                drawImage(doc, writer, files[files.Count - 1]);
                if (radioButton1.Checked == true)
                    addSignToLastPage(writer, pdfName, BaseColor.WHITE);
                else
                    addSignToLastPage(writer, pdfName, BaseColor.BLACK);

                doc.NewPage();
                doc.Close();
                writer.Close();
            }

        }

        private string getFileName(string pdfPath) {
            string[] arr = pdfPath.Split('\\');
            string[] brr = arr[arr.Length - 1].Split('.');
            return brr[0];
        }

        private float getPageWidthInPoints() {
            return iTextSharp.text.Utilities.MillimetersToPoints(this.pageWidth);
        }

        private float getPageHeightInPoints()
        {
            return iTextSharp.text.Utilities.MillimetersToPoints(this.pageHeight);
       }

        private void drawImage(Document doc, PdfWriter writer, string imageName) {
            System.Drawing.Image image = Image(imageName);
            image = RotateImage(image);
            AddImage(doc, image, writer);
        }
      
        private void addSignToLastPage(PdfWriter writer, string pdfName,BaseColor color) {
            float margin = iTextSharp.text.Utilities.MillimetersToPoints(5f);
            iTextSharp.text.Font font = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.UNDEFINED, 8,1,color);
            Phrase phrase = new Phrase(pdfName, font);
            PdfContentByte canvas = writer.DirectContent;
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, phrase, margin+2, margin+4, 0);
        }

       

        public static System.Drawing.Image Image(string file)
        {
            System.Drawing.Image image;
            int timeout = 240000;
            while (timeout > 0)
            {
                try
                {
                    image = System.Drawing.Image.FromFile(file);
                    //don't forget to either return from the function or break out fo the while loop
                    break;
                }
                catch (IOException)
                {
                }
                Thread.Sleep(100);
                timeout -= 100;
            }
            image = System.Drawing.Image.FromFile(file);
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
            return Extention.ExifRotate(image);
        }

        public void ScaleSameRatioImage(iTextSharp.text.Image iTextImage, Document doc)
        {
            iTextImage.ScalePercent(24f);
            iTextImage.ScaleAbsoluteHeight(doc.PageSize.Height - (this.marginTopBottom * 2));
            iTextImage.ScaleAbsoluteWidth(doc.PageSize.Width - (this.marginLeftRight *2));
        }
        public bool IsSameRatio(Document doc, iTextSharp.text.Image iTextImage)
        {
            float f1 = (iTextImage.Height / iTextImage.Width);
            float f2 = ((doc.PageSize.Height - (this.marginTopBottom * 2)) / (doc.PageSize.Width - (this.marginLeftRight * 2)));
            if (f1.Equals(f2))
                return true;
            return false;
        }
        public void SetBlackBorder(float margin,Document doc, PdfContentByte cb, iTextSharp.text.Image iTextImage,float newMarginLeftRight, float newMarginTopBottom)
        {
            //the order is important
            cb.SetColorFill(BaseColor.BLACK);
            if(checkBox1.Checked)
             cb.Rectangle(newMarginLeftRight - margin,newMarginTopBottom - margin, iTextImage.ScaledWidth + (margin * 2), iTextImage.ScaledHeight + (margin * 2));
            else
                cb.Rectangle(this.marginLeftRight - margin, this.marginTopBottom - margin, iTextImage.ScaledWidth + (margin * 2), iTextImage.ScaledHeight + (margin * 2));
            cb.FillStroke();
        }
        public static string SavedPath()
        {
            string path = "";
            Thread t = new Thread((ThreadStart)(() =>
            {
                using (SaveFileDialog save = new SaveFileDialog())
                {                 
                    save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    save.Filter = "PDF file|*.pdf";
                    save.ValidateNames = true;
                    save.FileName = DateTime.Now.ToString("dd.MM.yyyy HH-mm-ss");
                    DialogResult dr = save.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        path = save.FileName;
                    } 
                }

            }));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            return path;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
        }



        public void AddImage(Document doc, System.Drawing.Image image, PdfWriter writer)
        {
            //איפוס
            this.marginTopBottom = iTextSharp.text.Utilities.MillimetersToPoints((this.pageHeight - this.selectedHeightOfImg) / 2);
            this.marginLeftRight = iTextSharp.text.Utilities.MillimetersToPoints((this.pageWidth - this.selectedWidthOfImg) / 2);
            float margin = iTextSharp.text.Utilities.MillimetersToPoints(5f);
            float newMarginLeftRight=margin*2;
            float newMarginTopBottom=margin*2;
            iTextSharp.text.Image iTextImage = iTextSharp.text.Image.GetInstance(image, System.Drawing.Imaging.ImageFormat.Jpeg);
            PdfContentByte cb = writer.DirectContent;
            if (radioButton1.Checked == true)
            {
                margin += iTextSharp.text.Utilities.MillimetersToPoints(5f);
                this.marginTopBottom += iTextSharp.text.Utilities.MillimetersToPoints(5f);
                this.marginLeftRight += iTextSharp.text.Utilities.MillimetersToPoints(5f);
            }
            iTextSharp.text.Image clipped = iTextImage;
            if (IsSameRatio(doc, iTextImage))
            {
                iTextImage.SetAbsolutePosition(this.marginLeftRight, this.marginTopBottom);
                ScaleSameRatioImage(iTextImage, doc);
            }        
            else
            if(checkBox1.Checked == true)
            {
                //הקטנת התמונה
                float precentWidth = ((doc.PageSize.Width - (this.marginLeftRight * 2)) / iTextImage.Width) * 100;
                float precentHeight = ((doc.PageSize.Height - (this.marginTopBottom * 2)) / iTextImage.Height) * 100;
                float minPrecent = Math.Min(precentWidth, precentHeight);
                iTextImage.ScalePercent(minPrecent);
                newMarginLeftRight = (doc.PageSize.Width - iTextImage.ScaledWidth) / 2;
                newMarginTopBottom = (doc.PageSize.Height - iTextImage.ScaledHeight) / 2;
                //מיקום 
                iTextImage.SetAbsolutePosition(newMarginLeftRight, newMarginTopBottom);
            }
            else
            {
                //הקטנת התמונה
                float precentWidth = ((doc.PageSize.Width - (this.marginLeftRight * 2)) / iTextImage.Width) * 100;
                float precentHeight = ((doc.PageSize.Height - (this.marginTopBottom * 2)) / iTextImage.Height) * 100;
                float maxPrecent = Math.Max(precentWidth, precentHeight);
                iTextImage.ScalePercent(maxPrecent);
                //cut the minimum width/heigt scaledwidth-doc.PageSize.Width - (margin * 2)
                //position in the middle 
                 newMarginLeftRight = (doc.PageSize.Width - iTextImage.ScaledWidth) / 2;
                 newMarginTopBottom = (doc.PageSize.Height - iTextImage.ScaledHeight) / 2;
                //cutting the image
                float a = iTextImage.ScaledHeight;
                float b = doc.PageSize.Height - (this.marginTopBottom * 2);
                float c = a - b;
                float cropY =c/2 ;
                 a = iTextImage.ScaledWidth;
                 b = doc.PageSize.Width - (this.marginLeftRight * 2);
                 c = a - b;
                float cropX = c / 2;
                iTextImage.SetAbsolutePosition(newMarginLeftRight, newMarginTopBottom);
                clipped = cropImage(iTextImage, cb, writer, cropX, cropY, doc.PageSize.Width - (this.marginLeftRight * 2), doc.PageSize.Height - (this.marginTopBottom * 2));
                clipped.SetAbsolutePosition(this.marginLeftRight, this.marginTopBottom);
            }
            if (radioButton1.Checked == true)
            {
                SetBlackBorder(iTextSharp.text.Utilities.MillimetersToPoints(5f), doc, cb, clipped,newMarginLeftRight,newMarginTopBottom);
            }
            cb.AddImage(clipped);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            this.pageHeight = 10f;
            search.Enabled = true;
            decimal val = numericUpDown1.Value;
            this.selectedHeightOfImg = (float)(val * 10);
            this.pageHeight += this.selectedHeightOfImg;
            this.marginTopBottom = iTextSharp.text.Utilities.MillimetersToPoints((this.pageHeight - this.selectedHeightOfImg) / 2);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            this.pageWidth = 10f;
            search.Enabled = true;
            decimal val = numericUpDown2.Value;
            this.selectedWidthOfImg = (float)(val * 10);
            this.pageWidth += this.selectedWidthOfImg;
            this.marginLeftRight = iTextSharp.text.Utilities.MillimetersToPoints((this.pageWidth - this.selectedWidthOfImg) / 2);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                comboBox1.Enabled = true;
                numericUpDown1.Enabled = false;
                numericUpDown2.Enabled = false;
                numericUpDown1.Value = 0;
                numericUpDown2.Value = 0;
            }
            else
            {
                comboBox1.Enabled = false;
                numericUpDown1.Enabled = true;
                numericUpDown2.Enabled = true;
                comboBox1.SelectedIndex = -1;
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton4.Checked)
            {
                comboBox1.Enabled = false;
                numericUpDown1.Enabled = true;
                numericUpDown2.Enabled = true;
                comboBox1.SelectedIndex = -1;
            }
            else
            {
                comboBox1.Enabled = true;
                numericUpDown1.Enabled = false;
                numericUpDown2.Enabled = false;
                numericUpDown1.Value = 0;
                numericUpDown2.Value = 0;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.pageHeight = 10f;
            this.pageWidth = 10f;
            search.Enabled = true;
            if(comboBox1.SelectedIndex== 0)
            {
                this.selectedWidthOfImg = 145f;
                this.selectedHeightOfImg = 90f;
            }
            else
                if(comboBox1.SelectedIndex == 1)
                {
                    this.selectedWidthOfImg = 140f;
                    this.selectedHeightOfImg = 105f;
                }
            this.pageHeight += this.selectedHeightOfImg;
            this.pageWidth += this.selectedWidthOfImg;
            this.marginTopBottom = iTextSharp.text.Utilities.MillimetersToPoints((this.pageHeight- this.selectedHeightOfImg)/2);
            this.marginLeftRight = iTextSharp.text.Utilities.MillimetersToPoints((this.pageWidth- this.selectedWidthOfImg)/2);

        }

        public iTextSharp.text.Image cropImage(iTextSharp.text.Image image, PdfContentByte cb, PdfWriter writer, float x, float y, float width, float height)
        {
            PdfTemplate t = cb.CreateTemplate(width, height);
            float origWidth = image.ScaledWidth;
            float origHeight = image.ScaledHeight;
            t.AddImage(image, origWidth, 0, 0, origHeight, -x, -y);
            return iTextSharp.text.Image.GetInstance(t);
        }




    }
  
}
