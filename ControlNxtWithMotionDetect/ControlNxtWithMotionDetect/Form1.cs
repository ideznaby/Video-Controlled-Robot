using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Drawing;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace ControlNxtWithMotionDetect
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        SerialPort BluetoothConnection = new SerialPort();//Create Bluetooth Connection to connect with nxt
        public void show()
        {
            CvPoint p = new CvPoint(0,0);
            CvPoint tmp;
            CvCapture capture;
            IplImage input;
            capture = Cv.CreateCameraCapture(0);//capture the main camera
            CvWindow w = new CvWindow("Camera");//window to show the camera
            CvWindow w1 = new CvWindow("What Program sees");//window to show the processed video
            Mat mtx = new Mat(capture.FrameWidth, capture.FrameHeight, MatrixType.U8C1);//a one dimention matrix to store and process the captured video
            while (CvWindow.WaitKey(10) < 0)
            {
                input = Cv.QueryFrame(capture);//get a frame
                CvRect U = new CvRect(220, 10, 200, 100);//Rectangle at top of the screen
                CvRect L = new CvRect(10, 140, 100, 200);//Rectangle at left of the screen
                CvRect D = new CvRect(220, 370, 200, 100);//Rectangle at down of the screen
                CvRect R = new CvRect(530, 140, 100, 200);//Rectangle at top of the screen
                //draw Rectangles on the screen
                input.Rectangle(U, 0,2);
                input.Rectangle(L, 0, 2);
                input.Rectangle(D, 0, 2);
                input.Rectangle(R, 0, 2);
                CvCpp.ExtractImageCOI(input, mtx, 0);//put taken frame to matrix
                Cv.Threshold(mtx.ToCvMat(), mtx.ToCvMat(), 120, 255, 0);//set numbers below 120 to 0 and above 120 until 255 to 255 (it removes other objects that arent bright from screen
                Cv.MinMaxLoc(mtx.ToCvMat(),out tmp,out p);//get location of max number in mtx ( since it contains only 0 and 255) it will find first 255 in screen which is our object
                if ((p.X < 640) && (p.Y < 480))//if p is in screen
                {
                    //draw a rectangle on where the object is seen
                      CvRect mask = new CvRect(p, new CvSize(10, 10));
                      input.Rectangle(mask, 0,20);
                }
                if (isinrec(p, U)) MessageBox.Show("3");//if the object is in the top rectangle say robot to go up
                else if (isinrec(p, L)) MessageBox.Show("2");//if the object is in the left rectangle say robot to go left
                else if (isinrec(p, D)) MessageBox.Show("4");//if the object is in the bottom rectangle say robot to go down
                else if (isinrec(p, R)) MessageBox.Show("1");//if the object is in the right rectangle say robot to go right
                w.Image = input;//show taken image on screen
                w1.Image = mtx.ToCvMat();//show processed image on screen
            }
        }
        public bool isinrec(CvPoint p, CvRect R) // this function finds out point p is in rectangle R or not
        {
            if (p.X > R.Left && p.X < R.Right && p.Y > R.Top && p.Y < R.Bottom)
                return true;
            else
                return false;
        }
        private string NXTSendCommandAndGetReply(byte[] Command)
        {
            string r = "";
            Byte[] MessageLength = { 0x00, 0x00 };
            MessageLength[0] = (byte)Command.Length;
            BluetoothConnection.Write(MessageLength, 0, MessageLength.Length);
            BluetoothConnection.Write(Command, 0, Command.Length);
            int length = BluetoothConnection.ReadByte() + 256 * BluetoothConnection.ReadByte();
            for (int i = 0; i < length; i++)
                r += BluetoothConnection.ReadByte().ToString("X2");
            return r;
        }
        private int read(int mailbox)
        {
            byte[] NxtMessage = { 0x00, 0x13, 0x00, 0x00, 0x01 };
            NxtMessage[2] = (byte)(mailbox - 1 + 10);
            string r = NXTSendCommandAndGetReply(NxtMessage);
            return Convert.ToInt32((r[10] + "" + r[11]), 16);
        }
        private void send(int a)
        {
            byte[] NxtMessage = { 0x00, 0x09, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00 };
            NxtMessage[2] = (byte)(0);
            int tmp = (int)a;
            for (int ByteCtr = 0; ByteCtr <= 3; ByteCtr++)
            {
                NxtMessage[4 + ByteCtr] = (byte)tmp;
                tmp >>= 8;
            }
            NXTSendCommandAndGetReply(NxtMessage);
        }
        private void start_Click(object sender, EventArgs e)
        {
            show();
        }
        public CvPoint findredball(IplImage input)
        {
            for (int i = 0; i < 480; i++)
                for (int j = 0; j < 640; j++)
                    if ((input[i, j].Val0 > 180 && input[i, j].Val0 > 30) && (input[i, j].Val1 > 180 && input[i, j].Val1 > 30) && (input[i, j].Val2 > 60 && input[i, j].Val2 > 180))
                    {
                        //MessageBox.Show(input[i, j] + "");
                        return new CvPoint(i, j);
                    }
            return new CvPoint(0, 0);
        }
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try{
            this.buttonConnect.Enabled = false;
            if (BluetoothConnection.IsOpen)
            {
                BluetoothConnection.Close();
                this.buttonConnect.Text = "Connect";
            }
            else
            {
                this.buttonConnect.Text = "Disconnect";
                start.Enabled = true;
                this.BluetoothConnection.PortName = this.textBox1.Text.Trim();
                BluetoothConnection.Open();
                BluetoothConnection.ReadTimeout = 1500;
            }
            this.buttonConnect.Enabled = true;
            }
            catch(Exception ex){
              MessageBox.Show("there was a problem connecting with nxt please check your nxt device" + ex);
            }
        }
    }
}
