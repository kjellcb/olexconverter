using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace OlexConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();
            if (CultureInfo.CurrentCulture.Name != "no-NB")
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("nb-NO");
            }
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002088 File Offset: 0x00000288
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ".gz files (*.gz)|*.gz";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Stream stream;
                    if ((stream = openFileDialog.OpenFile()) != null)
                    {
                        using (stream)
                        {
                            this.textBox1.Text = openFileDialog.FileName;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message);
                }
            }
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002114 File Offset: 0x00000314
        private void button2_Click(object sender, EventArgs e)
        {
            string text = null;
            if (string.IsNullOrWhiteSpace(this.textBox1.Text))
            {
                MessageBox.Show("Ingen fil er valgt");
                return;
            }
            else
            {
                int num = textBox1.Text.LastIndexOf("//");
                text = this.textBox1.Text.Substring(0, num);
            }
            if (this.checkBox1.Checked && !this.checkBox2.Checked)
            {
                string str = this.DegreeMinuteDecimal(text);
                MessageBox.Show("Filen lagret som: " + text + "\\" + str);
            }
            if (this.checkBox2.Checked && !this.checkBox1.Checked)
            {
                string str2 = this.LagKML(text);
                MessageBox.Show("Filen lagret som: " + text + "\\" + str2);
            }
            if (this.checkBox2.Checked && this.checkBox1.Checked)
            {
                string text2 = this.DegreeMinuteDecimal(text);
                string text3 = this.LagKML(text);
                MessageBox.Show(string.Concat(new string[]
                {
                    "Koordinater fra OLEX lagret som: ",
                    text,
                    "\\",
                    text2,
                    "\nKML fil til Google Earth lagret som: ",
                    text,
                    "\\",
                    text3
                }));
            }
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002250 File Offset: 0x00000450
        public static string[] Decompress(string fileName)
        {
            string[] result;
            using (FileStream fileStream = File.OpenRead(fileName))
            {
                using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(memoryStream);
                        memoryStream.Seek(0L, SeekOrigin.Begin);
                        result = Encoding.UTF8.GetString(memoryStream.ToArray()).Split(new string[]
                        {
                            "\n"
                        }, StringSplitOptions.None);
                    }
                }
            }
            return result;
        }

        // Token: 0x06000005 RID: 5 RVA: 0x000022F4 File Offset: 0x000004F4
        public string DegreeMinuteDecimal(string folderName)
        {
            string[] array = Form1.Decompress(this.textBox1.Text);
            List<string> list = new List<string>();
            string fileName = "olexplot-" + DateTime.Now.ToString("HH-mm-ss") + ".txt";
            try
            {
                foreach (string text2 in array)
                {
                    if (text2.Length > 2)
                    {
                        float num;
                        if (float.TryParse(text2.Substring(0, 4), out num))
                        {
                            list.Add(text2);
                        }
                        if (text2.Contains("Navn"))
                        {
                            list.Add(text2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Noe gikk galt\n" + ex.Message);
            }
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(folderName + "\\" + fileName))
                {
                    foreach (string text3 in list.ToArray())
                    {
                        float num2;
                        if (float.TryParse(text3.Substring(0, 4), out num2))
                        {
                            string[] array3 = text3.Split(null);
                            array3[0] = (float.Parse(array3[0], CultureInfo.InvariantCulture) / 60f).ToString("n6");
                            array3[1] = (float.Parse(array3[1], CultureInfo.InvariantCulture) / 60f).ToString("n6");
                            string[] array4 = array3[0].Split(new char[]
                            {
                                ','
                            });
                            array4[1] = "0," + array4[1];
                            array4[1] = (float.Parse(array4[1]) * 60f).ToString("n6");
                            string[] array5 = array3[1].Split(new char[]
                            {
                                ','
                            });
                            array5[1] = "0," + array5[1];
                            array5[1] = (float.Parse(array5[1]) * 60f).ToString("n6");
                            if (float.Parse(array5[1]) < 10f)
                            {
                                array5[1] = ("0" + array5[1]).ToString();
                            }
                            array4[1] = array4[1].Replace(",", ".");
                            array5[1] = array5[1].Replace(",", ".");
                            streamWriter.WriteLine(string.Concat(new string[]
                            {
                                array4[0],
                                array4[1],
                                ",",
                                array5[0],
                                array5[1]
                            }));
                        }
                        else
                        {
                            streamWriter.WriteLine(text3.Substring(5));
                        }
                    }
                }
            }
            catch (Exception ex2)
            {
                MessageBox.Show("Noe gikk galt\n" + ex2.Message);
            }
            return fileName;
        }

        // Token: 0x06000006 RID: 6 RVA: 0x000025FC File Offset: 0x000007FC
        public string LagKML(string folderName)
        {
            string text = "google_earth-" + DateTime.Now.ToString("HH-mm-ss") + ".kml";
            this.textBox1.Text.LastIndexOf("\\");
            string[] result = Form1.Decompress(this.textBox1.Text);
            string str = this.RaaFil(result, folderName);
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.NewLineOnAttributes = false;
            using (XmlWriter xmlWriter = XmlWriter.Create(folderName + "\\" + text, xmlWriterSettings))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("kml", "http://www.opengis.net/kml/2.2");
                xmlWriter.WriteStartElement("Document");
                xmlWriter.WriteStartElement("name");
                xmlWriter.WriteString("Paths");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("description");
                xmlWriter.WriteString("Beskrivelse av filen");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Style");
                xmlWriter.WriteAttributeString("id", "style1");
                xmlWriter.WriteStartElement("LineStyle");
                xmlWriter.WriteStartElement("color");
                xmlWriter.WriteString("00000000");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("width");
                xmlWriter.WriteString("4");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                int num = 20;
                using (StreamReader streamReader = new StreamReader(new FileStream(folderName + "\\" + str, FileMode.Open)))
                {
                    string text2;
                    while ((text2 = streamReader.ReadLine()) != null)
                    {
                        if (num == 15 && text2.Equals(""))
                        {
                            num = 19;
                        }
                        if (text2.Equals("") && num != 19)
                        {
                            num = 20;
                        }
                        float num2;
                        if (num < 19 && float.TryParse(text2.Substring(0, 4), out num2))
                        {
                            num = 1;
                        }
                        if (text2.Contains("Rute"))
                        {
                            num = 0;
                            text2 = streamReader.ReadLine();
                        }
                        if (num == 0)
                        {
                            xmlWriter.WriteStartElement("Placemark");
                            xmlWriter.WriteStartElement("name");
                            xmlWriter.WriteString("olexplott");
                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteStartElement("description");
                            xmlWriter.WriteString(text2);
                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteStartElement("styleUrl");
                            xmlWriter.WriteString("yellowLineGreenPoly");
                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteStartElement("MultiGeometry");
                            xmlWriter.WriteStartElement("LineString");
                            xmlWriter.WriteStartElement("extrude");
                            xmlWriter.WriteString("1");
                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteStartElement("tessellate");
                            xmlWriter.WriteString("1");
                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteStartElement("altitudeMode");
                            xmlWriter.WriteString("absolute");
                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteStartElement("coordinates");
                            text2 = streamReader.ReadLine();
                            num++;
                        }
                        float num3;
                        if (num == 1 && float.TryParse(text2.Substring(0, 4), out num3))
                        {
                            string[] array = text2.Split(null);
                            array[0] = (float.Parse(array[0], CultureInfo.InvariantCulture) / 60f).ToString("n6");
                            array[1] = (float.Parse(array[1], CultureInfo.InvariantCulture) / 60f).ToString("n6");
                            xmlWriter.WriteString(array[1].Replace(",", ".") + "," + array[0].Replace(",", ".") + ",0\r           ");
                            num = 15;
                        }
                        if (num == 19)
                        {
                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteEndElement();
                            num = 20;
                        }
                    }
                    xmlWriter.WriteEndDocument();
                }
            }
            return text;
        }


    }
}
