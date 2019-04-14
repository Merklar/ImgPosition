using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImgPosition
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string sourceFileAdress;
        private string[] layersFilesAdress;
        private string[] patternArray;
        private List<String> outputArray= new List<string>();

        private int deltaX = 0;
        private int deltaY = 0;

        public string SourceFileAdress { get => sourceFileAdress; set => sourceFileAdress = value; }
        public string[] LayersFilesAdress { get => layersFilesAdress; set => layersFilesAdress = value; }
        public string[] PatternArray { get => patternArray; set => patternArray = value; }
        public List<string> OutputArray { get => outputArray; set => outputArray = value; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SetTextBoxFieldText(string _text)
        {
            textBoxMainField.Text += "\n" + _text;
        }

        private void DropSourceFileField_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string file in files)
                {
                    SourceFileAdress = file.ToString();
                    textBoxSourceFile.Text = SourceFileAdress;
                }
            }
        }

        private void DropLayersField_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                LayersFilesAdress = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string file in LayersFilesAdress)
                {
                    SetTextBoxFieldText(file.ToString());
                }
            }
        }

        private string GetZText()
        {
            string temp = textBoxZLevel.Text;
            int z;
            try
            {
                int.TryParse(temp, out z);
            }
            catch
            {
                SetTextBoxFieldText("z - неправильного формата!");
                return "110";
            }

            return z.ToString();
        }

        private string GetIHTText()
        {
            Object tempObj  = IgnoreHitTestCB.SelectionBoxItem;
            string iht;
           
            try
            {
                iht = tempObj.ToString();
            }
            catch
            {
                SetTextBoxFieldText("IGH - неправильного формата!");
                return "true";
            }
            return iht;
        }

        private string GetImportStringPatern(string _name, string _x, string _y, string _z, string _animPath, string _tw, string _th, string _hx, string _hy, string _iht)
        {
            const string PATERN_STRING  = "currentScene:CreateGameObject{{name='{0}',x={1},y={2},z={3},aDeg=0,sx=1,sy=1,animPath='{4}',tx=0,ty=0,tw={5},th={6},nFrames=1,fps=0,hx={7},hy={8},blendMode=EBlendMode.NORMAL,color='0xFFFFFFFF',ignoreHitTest={9}, loadImmediately=false,checkScene=true}}";
            string _patern              = string.Format(PATERN_STRING, _name, _x, _y, _z, _animPath, _tw, _th, _hx, _hy, _iht);
            return _patern;
        }

        private void buttonSaveToBuffer_Click(object sender, RoutedEventArgs e)
        {
            textBoxMainField.Text = "";
            OutputArray.Clear();

            try
            {
                PatternArray = File.ReadAllLines(SourceFileAdress);
            }
            catch {
                SetTextBoxFieldText("АХТУНГ!!! Произошла какая то хрень с шаблонным файлом");
                return;
            }

            deltaX = 0;
            deltaY = 0;

            try
            {
                string[] paternFile = PatternArray[0].Split('	');
                int.TryParse(paternFile[0], out deltaX);
                int.TryParse(paternFile[1], out deltaY);
                deltaX = 1366 - deltaX;
                deltaY = 768 - deltaY;
            }
            catch
            {
                SetTextBoxFieldText("СБОЙ с поправкой на зум");
                deltaX = 0;
                deltaY = 0;
            }

            try
            {
                foreach (string filePath in LayersFilesAdress)
                {
                    string file = System.IO.Path.GetFileNameWithoutExtension(filePath);
                    int length = PatternArray.Length;
                    for (int i = 0; i < length; i++)
                    {                       
                        string[] paternFile = PatternArray[i].Split('	');

                        if (file == paternFile[0].ToString())
                        {
                            int _dx;
                            int _dy;

                            int.TryParse(paternFile[3], out _dx);
                            int.TryParse(paternFile[4], out _dy);
                            _dx += deltaX / 2;
                            _dy += deltaY / 2;

                            SetTextBoxFieldText("Есть совпадение");
                            string[] imgProp = GetImageProperies(filePath);
                            string animPath = filePath.Remove(0, filePath.IndexOf("assets"));
                            animPath = animPath.Replace('\\', '/');
                            OutputArray.Add(GetImportStringPatern(file, _dx.ToString(), _dy.ToString(), GetZText(), animPath, imgProp[0], imgProp[1], imgProp[2], imgProp[3], GetIHTText()));
                            OutputArray.Add("\n\n");
                        }

                    }
                }
            }
            catch
            {
                SetTextBoxFieldText("АХТУНГ!!! Проверте, кинули ли вы файлы со слоями или шаблоном");
                return;
            }
            string outPutString = "";
            foreach (string stroke in OutputArray)
            {
                outPutString += stroke;
            }
            Clipboard.SetText(outPutString);
            SetTextBoxFieldText(outPutString);
            SetTextBoxFieldText("Данные скопированны в буфер обмена");
        }

        private string[] GetImageProperies(string _path)
        {
            string[] returnParam = new string[4];
            try
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(_path);
                returnParam[0] = image.Width.ToString();
                returnParam[1] = image.Height.ToString();
                returnParam[2] = ((int)(Math.Floor((double)(image.Width / 2)))).ToString();
                returnParam[3] = ((int)(Math.Floor((double)(image.Height / 2)))).ToString();
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("There was an error opening the bitmap." +
                    "Please check the path.");
            }

            return returnParam;
        }
    }
}
