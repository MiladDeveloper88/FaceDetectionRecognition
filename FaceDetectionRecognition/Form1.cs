using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceDetectionRecognition
{
    public partial class Form1 : Form
    {
        //Declare Variables
        MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d, 0.6d);
        HaarCascade faceDetected;
        Image<Bgr, Byte> Frame;
        Capture camera;
        Image<Gray, byte> result;
        Image<Gray, byte> trainedFace = null;
        Image<Gray, byte> grayFace = null;
        List<Image<Gray, byte>> trainingImage = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> Users = new List<string>();
        int count, numLabels, t;
        string name, names = null;


        public Form1()
        {
            InitializeComponent();
            // HaarCascade is for face detection.
            faceDetected = new HaarCascade("haarcascade_frontalface_default.xml");

            try{
                string Labelsinf = File.ReadAllText(Application.StartupPath + "/Faces/faces.txt");
                string[] Labels = Labelsinf.Split(',');
                // The first label before , will be the number of faces saved.
                numLabels = Convert.ToInt16(Labels[0]);
                count = numLabels;
                string FacesLoad;
                for (int i = 0; i < numLabels + 1; i++) {
                    FacesLoad = "face" + i + ".bmp";
                    trainingImage.Add(new Image<Gray, byte>(Application.StartupPath + "/Faces/Faces.txt"));
                    labels.Add(Labels[i]);
                }
            }catch(Exception){
                MessageBox.Show("Nothing is in database");
            }
        }

        private void StartDetectionAndRecognition_Click(object sender, EventArgs e)
        {
            camera = new Capture();
            camera.QueryFrame();
            Application.Idle += new EventHandler(FrameProcedure);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            count++;
            grayFace = camera.QueryGrayFrame().Resize(320,240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            MCvAvgComp[][] DetectedFaces = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach(MCvAvgComp f in DetectedFaces[0]) { 
                trainedFace = Frame.Copy(f.rect).Convert<Gray, byte>();
                break;
            }
            trainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            trainingImage.Add(trainedFace);
            labels.Add(textName.Text);
            File.WriteAllText(Application.StartupPath + "/Faces/Faces.txt", trainingImage.ToArray().Length.ToString() + ",");
            for(int i =1;i<trainingImage.ToArray().Length + 1; i++)
            {
                trainingImage.ToArray()[i-1].Save(Application.StartupPath + "/Faces/face" + i + ".bmp");
                File.AppendAllText(Application.StartupPath + "/Faces/Faces.txt", labels.ToArray()[i - 1] + ",");
            }

            MessageBox.Show(textName.Text + " Added Successfully");
        }

        private void FrameProcedure(object sender, EventArgs e)
        {
            Users.Add("");
            Frame = camera.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            grayFace = Frame.Convert<Gray, Byte>();
            MCvAvgComp[][] faceDetecteNow = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach(MCvAvgComp f in faceDetecteNow[0])
            {
                result = Frame.Copy(f.rect).Convert<Gray, Byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                Frame.Draw(f.rect, new Bgr(Color.Green), 3);
                if(trainingImage.ToArray().Length != 0)
                {
                    MCvTermCriteria termCriterias = new MCvTermCriteria(count, 0.001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImage.ToArray(), labels.ToArray(), 1500, ref termCriterias);
                    name = recognizer.Recognize(result);
                    Frame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.Red));
                }

                Users.Add("");
            }
            cameraBox.Image = Frame;
            names = "";
            Users.Clear();
        }
    }
}
