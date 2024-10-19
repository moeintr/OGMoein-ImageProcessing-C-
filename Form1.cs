
using System.Drawing;
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Threading.Tasks;
using ImageProcessor.Processors;
using YamlDotNet.Core.Events;
using System.Threading;
using System.Text.RegularExpressions;

namespace OGMoein
{
    public partial class Form1 : Form
    {
        // مشخص کردن ناحیه‌ای که می‌خواهیم اسکرین شات بگیریم
        private int x = 0; // مختصات x نقطه شروع
        private int y = 0; // مختصات y نقطه شروع
        // ایجاد یک شیء Bitmap با ابعاد مشخص شده
        private Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            //if (!IsPlaformCompatable()) return;

            //String win1 = "Test Window"; //The name of the window
            //CvInvoke.cvNamedWindow(win1); //Create the window using the specific name

            //Image<Bgr, Byte> img = new Image<Bgr, byte>(400, 200, new Bgr(255, 0, 0)); //Create an image of 400x200 of Blue color
            //MCvFont f = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0); //Create the font

            //img.Draw("Hello, world", ref f, new System.Drawing.Point(10, 80), new Bgr(0, 255, 0)); //Draw "Hello, world." on the image using the specific font

            //CvInvoke.cvShowImage(win1, img); //Show the image
            //CvInvoke.cvWaitKey(0);  //Wait for the key pressing event
            //CvInvoke.cvDestroyWindow(win1); //Destory the window
            radioButton2.Checked = true;
            button5.Focus();
        }

        private bool IsPlaformCompatable()
        {
            int clrBitness = Marshal.SizeOf(typeof(IntPtr)) * 8;
            if (clrBitness != CvInvoke.UnmanagedCodeBitness)
            {
                MessageBox.Show(String.Format("Platform mismatched: CLR is {0} bit, C++ code is {1} bit."
                   + " Please consider recompiling the executable with the same platform target as C++ code.",
                   clrBitness, CvInvoke.UnmanagedCodeBitness));
                return false;
            }
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBox1.Text = openFileDialog1.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox1.Text))
            {
                Image<Bgr, Byte> img = new Image<Bgr, byte>(textBox1.Text).Resize(400, 400, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR, true);

                if (!IsPlaformCompatable()) return;
                Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();

                Gray cannyThreshold = new Gray(180);
                Gray cannyThresholdLinking = new Gray(120);
                Gray circleAccumulatorThreshold = new Gray(500);

                CircleF[] circles = gray.HoughCircles(
                cannyThreshold,
                circleAccumulatorThreshold,
                4.0, //Resolution of the accumulator used to detect centers of the circles
                15.0, //min distance 
                5, //min radius
                0 //max radius
                )[0]; //Get the circles from the first channel                

                Image<Gray, Byte> cannyEdges = gray.Canny(cannyThreshold, cannyThresholdLinking);

                // Create threads for each window
                Thread thread1 = new Thread(() =>
                {
                    String win1 = "Test Window 1";
                    CvInvoke.cvNamedWindow(win1);
                    CvInvoke.cvShowImage(win1, cannyEdges);
                    CvInvoke.cvWaitKey(0);
                    CvInvoke.cvDestroyWindow(win1);
                });

                Thread thread2 = new Thread(() =>
                {
                    String win2 = "Test Window 2";  
                    CvInvoke.cvNamedWindow(win2);
                    CvInvoke.cvShowImage(win2, img);
                    CvInvoke.cvWaitKey(0);
                    CvInvoke.cvDestroyWindow(win2);
                });

                // Start the threads
                thread1.Start();
                thread2.Start();

                // Wait for both threads to finish
                //thread1.Join();
                //thread2.Join();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBox2.Text = openFileDialog1.FileName;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Image<Bgr, Byte> img1 = new Image<Bgr, byte>(textBox1.Text);
            Image<Bgr, Byte> img2 = new Image<Bgr, byte>(textBox2.Text);
            if (!IsPlaformCompatable()) return;
            Image<Gray, Byte> gray1 = img1.Convert<Gray, Byte>().PyrDown().PyrUp();
            Image<Gray, Byte> gray2 = img2.Convert<Gray, Byte>().PyrDown().PyrUp();
            Image<Gray, float> gray = gray1.MatchTemplate(gray2, TM_TYPE.CV_TM_SQDIFF_NORMED);
            int rad = gray.Width / 2;
            double[] minVal, maxVal;
            Point[] minLoc, maxLoc;
            gray.MinMax(out minVal, out maxVal, out minLoc, out maxLoc);

            if (maxVal[0] > 0.8)
            {
                CvInvoke.cvCircle(img1, minLoc[0], 50, new MCvScalar(0, 0, 255), 0, LINE_TYPE.FOUR_CONNECTED, 2);

                //Thread thread2 = new Thread(() =>
                //{
                //    String win2 = "Test Window 2";
                //    CvInvoke.cvNamedWindow(win2);
                //    CvInvoke.cvShowImage(win2, gray);
                //    CvInvoke.cvWaitKey(0);
                //    CvInvoke.cvDestroyWindow(win2);
                //});

                //Thread thread1 = new Thread(() =>
                //{
                 String win1 = "Test Window 1";
                 CvInvoke.cvNamedWindow(win1);
                 CvInvoke.cvShowImage(win1, img1);
                 CvInvoke.cvWaitKey(0);
                 CvInvoke.cvDestroyWindow(win1);
                //});
                //thread1.Start();
                //thread2.Start();
            }

            //using (Image<Gray, float> result = gray.MatchTemplate(gray2, TM_TYPE.CV_TM_SQDIFF_NORMED))
            //{
            //    double[] minVal, maxVal;
            //    Point[] minLoc, maxLoc;
            //    result.MinMax(out minVal, out maxVal, out minLoc, out maxLoc);

            //    String win2 = "Test Window 2";
            //    CvInvoke.cvNamedWindow(win2);
            //    CvInvoke.cvShowImage(win2, result);
            //    CvInvoke.cvWaitKey(0);
            //    CvInvoke.cvDestroyWindow(win2);

            //    if (minVal[0] < 0.1) // آستانه را بر اساس نیاز تنظیم کنید
            //    {
            //        // موقعیت الگو در تصویر اصلی: minLoc
            //        // شما می‌توانید یک مستطیل دور این ناحیه بکشید تا آن را مشخص کنید
            //        CvInvoke.cvRectangle(img1, minLoc[0], maxLoc[0], new MCvScalar(0, 0, 255), 0, LINE_TYPE.CV_AA, 1);
            //        //CvInvoke.Rectangle(img1, new Rectangle(minLoc[0], img2Gray.Size), new MCvScalar(0, 0, 255), 2);
            //        CvInvoke.cvShowImage("Result", img1);
            //        //CvInvoke.Imshow("Result", img1);
            //        CvInvoke.cvWaitKey(0);
            //    }
            //    else
            //    {
            //        Console.WriteLine("الگو پیدا نشد");
            //    }
            //}
        }

        private void button5_Click(object sender, EventArgs e)
        {

                   
        }

        private void radioButton1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F1)
            {
                radioButton1.Checked = true;
            }
            else if (e.Control && e.KeyCode == Keys.F2)
            {
                radioButton2.Checked = true;
            }
        }

        private void button5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F1)
            {
                radioButton1.Checked = true;
            }
            else if (e.Control && e.KeyCode == Keys.F2)
            {
                radioButton2.Checked = true;
            }
        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F1)
            {
                radioButton1.Checked = true;
            }
            else if (e.Control && e.KeyCode == Keys.F2)
            {
                radioButton2.Checked = true;
            }

        }

        private void button3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F1)
            {
                radioButton1.Checked = true;
            }
            else if (e.Control && e.KeyCode == Keys.F2)
            {
                radioButton2.Checked = true;
            }

        }

        private void button4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F1)
            {
                radioButton1.Checked = true;
            }
            else if (e.Control && e.KeyCode == Keys.F2)
            {
                radioButton2.Checked = true;
            }
        }

        private void radioButton2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F1)
            {
                radioButton1.Checked = true;
            }
            else if (e.Control && e.KeyCode == Keys.F2)
            {
                radioButton2.Checked = true;
            }
        }

        private void button2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F1)
            {
                radioButton1.Checked = true;
            }
            else if (e.Control && e.KeyCode == Keys.F2)
            {
                radioButton2.Checked = true;
            }
        }

        //private Emgu.CV.Capture capture;
        private Image<Bgr, Byte> frame;
        private bool isCapturing = false;


        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            //String win11 = "Win";
            //CvInvoke.cvNamedWindow(win11);
            //Image<Bgr, Byte> img1 = null;
            //Thread thread2 = new Thread(() =>
            //{
            //    while (radioButton1.Checked)
            //    {
            //        // ایجاد یک شیء Graphics برای انجام عملیات گرافیکی
            //        using (Graphics g = Graphics.FromImage(bmp))
            //        {
            //            // کپی کردن محتوای صفحه به داخل Bitmap
            //            g.CopyFromScreen(x, y, 0, 0, bmp.Size);
            //        }

            //        // ذخیره تصویر
            //        //bmp.Save("screenshot.png", System.Drawing.Imaging.ImageFormat.Png);
            //        img1 = new Image<Bgr, byte>(bmp);

            //    }
            //});
            //if (radioButton1.Checked) 
            //{                 
            //    thread2.Start();
            //    CvInvoke.cvShowImage(win11, img1);
            //}

            //if (radioButton2.Checked)
            //{
            //    thread2.Abort();
            //    CvInvoke.cvWaitKey(0);
            //    CvInvoke.cvDestroyWindow(win11);
            //}

            if (radioButton1.Checked)

            {
                //capture = new Emgu.CV.Capture();
                Application.Idle += new EventHandler(ProcessFrame);
                isCapturing = true;
            }
            else
            {
                Application.Idle -= ProcessFrame;
                //capture.Dispose();
                isCapturing = false;
            }
        }
        private void ProcessFrame(object sender, EventArgs e)
        {
            if (isCapturing)
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    // کپی کردن محتوای صفحه به داخل Bitmap
                    g.CopyFromScreen(x, y, 0, 0, bmp.Size);
                }

                // ذخیره تصویر
                //bmp.Save("screenshot.png", System.Drawing.Imaging.ImageFormat.Png);
                frame = new Image<Bgr, byte>(bmp).Resize(imageBox1.Size.Width, imageBox1.Size.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR, true);
                //frame = capture.QueryFrame();

                if (!String.IsNullOrEmpty(textBox2.Text))
                {
                    Image<Bgr, Byte> img2 = new Image<Bgr, byte>(textBox2.Text);
                    if (!IsPlaformCompatable()) return;
                    Image<Gray, Byte> gray1 = frame.Convert<Gray, Byte>().PyrDown().PyrUp();
                    Image<Gray, Byte> gray2 = img2.Convert<Gray, Byte>().PyrDown().PyrUp();
                    Image<Gray, float> gray = gray1.MatchTemplate(gray2, TM_TYPE.CV_TM_SQDIFF_NORMED);
                    int rad = gray.Width / 2;
                    double[] minVal, maxVal;
                    Point[] minLoc, maxLoc;
                    gray.MinMax(out minVal, out maxVal, out minLoc, out maxLoc);

                    if (maxVal[0] > 0.8)
                    {
                        CvInvoke.cvCircle(frame, minLoc[0], 50, new MCvScalar(0, 0, 255), 0, LINE_TYPE.FOUR_CONNECTED, 2);
                    }
                }

                imageBox1.Image = frame.Bitmap;
            }
        }
    }
}
