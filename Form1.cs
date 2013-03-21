using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ASC2LandXML
{
    public partial class Form1 : Form
    {
        string status = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            status = "";
            richTextBox1.Text = "";
            progressBar1.ForeColor = Color.Blue;
            progressBar1.Value = 0;
            backgroundWorker1.RunWorkerAsync();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = saveFileDialog1.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                SortedDictionary<int, int> points = new SortedDictionary<int, int>();
                NumberFormatInfo nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";
                nfi.NumberGroupSeparator = ",";
                XmlTextWriter xmltextWriter = new XmlTextWriter(Properties.Settings.Default.output, Encoding.UTF8);
                xmltextWriter.Formatting = Formatting.Indented;
                xmltextWriter.WriteStartDocument();
                xmltextWriter.WriteStartElement("LandXML");
                xmltextWriter.WriteAttributeString("version", "1.0");
                xmltextWriter.WriteAttributeString("date", "2012-11-12");
                xmltextWriter.WriteAttributeString("time", "14:54:53");
                xmltextWriter.WriteAttributeString("xmlns", "http://www.landxml.org/schema/LandXML-1.0");
                xmltextWriter.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                xmltextWriter.WriteAttributeString("xsi:schemaLocation", "http://www.landxml.org/schema/LandXML-1.0 http://www.landxml.org/schema/landxml-1.0/LandXML-1.0.xsd");
                xmltextWriter.WriteStartElement("Units");
                xmltextWriter.WriteStartElement("Metric");
                xmltextWriter.WriteAttributeString("areaUnit", "squareMeter");
                xmltextWriter.WriteAttributeString("linearUnit", "meter");
                xmltextWriter.WriteAttributeString("volumeUnit", "cubicMeter");
                xmltextWriter.WriteAttributeString("temperatureUnit", "celsius");
                xmltextWriter.WriteAttributeString("pressureUnit", "HPA");
                xmltextWriter.WriteEndElement();//Metric
                xmltextWriter.WriteEndElement();//Units
                xmltextWriter.WriteStartElement("Application");
                xmltextWriter.WriteAttributeString("name", "LandXML");
                xmltextWriter.WriteAttributeString("manufacturer", "addin.dk");
                xmltextWriter.WriteAttributeString("version", "1.0");
                xmltextWriter.WriteAttributeString("manufacturerURL", "www.addin.dk");
                xmltextWriter.WriteEndElement();//Application
                xmltextWriter.WriteStartElement("Surfaces");
                xmltextWriter.WriteStartElement("Surface");
                xmltextWriter.WriteAttributeString("name", "DHM");
                xmltextWriter.WriteAttributeString("desc", "DHM");
                xmltextWriter.WriteAttributeString("state", "proposed");
                xmltextWriter.WriteStartElement("Definition");
                xmltextWriter.WriteAttributeString("surfType", "TIN");
                xmltextWriter.WriteStartElement("Pnts");

                using (StreamReader sr = new StreamReader(Properties.Settings.Default.input))
                {
                    string line = sr.ReadLine();
                    string[] data = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int ncols = 0;
                    int.TryParse(data[1], out ncols);

                    line = sr.ReadLine();
                    data = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int nrows = 0;
                    int.TryParse(data[1], out nrows);

                    line = sr.ReadLine();
                    data = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int xllcorner = 0;
                    int.TryParse(data[1], NumberStyles.Number, nfi, out xllcorner);

                    line = sr.ReadLine();
                    data = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int yllcorner = 0;
                    int.TryParse(data[1], NumberStyles.Number, nfi, out yllcorner);

                    line = sr.ReadLine();
                    data = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int cellsize = 0;
                    int.TryParse(data[1], out cellsize);

                    line = sr.ReadLine();
                    data = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    double NODATA_value = 0;
                    double.TryParse(data[1], out NODATA_value);

                    int id = 0;
                    int a = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        backgroundWorker1.ReportProgress((int)(100 * sr.BaseStream.Position / sr.BaseStream.Length));

                        int y = yllcorner + nrows * cellsize - a * cellsize;
                        a++;
                        data = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < ncols; i++)
                        {
                            double z = 0;
                            double.TryParse(data[i], out z);
                            if (z != NODATA_value)
                            {
                                id++;
                                int x = xllcorner + i * cellsize;

                                xmltextWriter.WriteStartElement("P");
                                xmltextWriter.WriteAttributeString("id", id.ToString());
                                xmltextWriter.WriteString(y.ToString() + " " + x.ToString() + " " + data[i]);
                                xmltextWriter.WriteEndElement();
                                points.Add(id, 0);
                            }
                        }
                    }

                    xmltextWriter.WriteEndElement();//Pnts
                    xmltextWriter.WriteStartElement("Faces");
                    int k = 0;
                    int total = (nrows-1) * (ncols-1);
                    int lastprogress = 0;
                    for (int i = 0; i < nrows - 1; i++)
                    {
                        for (int j = 1; j < ncols; j++)
                        {
                            k++;
                            int progress = (int)(100 * (float)k / (float)total);
                            if (progress != lastprogress)
                            {
                                backgroundWorker1.ReportProgress(progress);
                                lastprogress = progress;
                            }
                            if (points.ContainsKey(i * ncols + j) && points.ContainsKey(i * ncols + j + 1) && points.ContainsKey(i * ncols + j + ncols))
                            {
                                xmltextWriter.WriteStartElement("F");
                                xmltextWriter.WriteString((i * ncols + j).ToString() + " " + (i * ncols + j + 1).ToString() + " " + (i * ncols + j + ncols).ToString());
                                xmltextWriter.WriteEndElement();
                            }
                            if (points.ContainsKey(i * ncols + j + ncols) && points.ContainsKey(i * ncols + j + 1) && points.ContainsKey(i * ncols + j + 1 + ncols))
                            {
                                xmltextWriter.WriteStartElement("F");
                                xmltextWriter.WriteString((i * ncols + j + ncols).ToString() + " " + (i * ncols + j + 1).ToString() + " " + (i * ncols + j + 1 + ncols).ToString());
                                xmltextWriter.WriteEndElement();
                            }
                        }
                    }
                    xmltextWriter.WriteEndElement();//Faces
                    xmltextWriter.WriteEndElement();//Definition
                    xmltextWriter.WriteStartElement("Feature");
                    xmltextWriter.WriteAttributeString("code", "Surface");
                    xmltextWriter.WriteStartElement("Property");
                    xmltextWriter.WriteAttributeString("label", "pref");
                    xmltextWriter.WriteAttributeString("value", "Default");
                    xmltextWriter.WriteEndElement();//Property
                    xmltextWriter.WriteStartElement("Property");
                    xmltextWriter.WriteAttributeString("label", "guid");
                    xmltextWriter.WriteAttributeString("value", "c83d3755-cece-4fdc-9bfe-d39c834dad1f");
                    xmltextWriter.WriteEndElement();//Property
                    xmltextWriter.WriteEndElement();//Feature

                    xmltextWriter.WriteEndElement();//Surface
                    xmltextWriter.WriteEndElement();//Surfaces
                    xmltextWriter.WriteEndElement();//LandXML
                }
                xmltextWriter.Close();
                status = "OK";
            }
            catch (Exception ex)
            {
                status= ex.Message+"\n"+ex.StackTrace;
                backgroundWorker1.CancelAsync();
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
//            progressBar1.Update();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            richTextBox1.Text = status;
        }
    }
}
