using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.SQLite;
using Dapper;
using System.Windows.Forms.DataVisualization.Charting;


namespace Desmos2
{
    public partial class Form1 : Form
    {
        private SQLiteConnection connection;
        List<(float, float)> points = new List<(float, float)>();
        public Form1()
        {
            InitializeComponent();

            addPage_Click(null, null);
            rbSpline.Select();
        }
        private void Draw()
        {
            Chart ch = (Chart)tabControl.SelectedTab.Controls["ch"];
            ch.Series.Clear();
            Series s = new Series("f(x)");
            s.ChartType = SeriesChartType.Spline;

            s.ChartArea = "f(x)";
            s.BorderWidth = 3;

            foreach (var point in points)
            {
                s.Points.AddXY(point.Item1, point.Item2);
            }

            ch.Series.Add(s);
        }
        private void CreateTable(string name)
        {
            connection = new SQLiteConnection("Data Source=DB.db;");
            connection.Open();

            connection.Execute(@"CREATE TABLE IF NOT EXISTS " + name + @" ( 
                                    id	INTEGER PRIMARY KEY,
	                                x	REAL NOT NULL,
	                                y	REAL NOT NULL)");

            connection.Close();
        }
        private void PullFromTable(string name)
        {
            connection = new SQLiteConnection("Data Source=DB.db;");
            connection.Open();
            using (SQLiteCommand command = new SQLiteCommand($@"SELECT x , y  FROM {name}", connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    points.Clear();
                    while (reader.Read())
                    {
                        points.Add((reader.GetFloat(0), reader.GetFloat(1)));
                    }
                }
            }
        }
        private void SaveTable(string name)
        {
            connection = new SQLiteConnection("Data Source=DB.db;");
            connection.Open();

            connection.Execute($@"INSERT INTO {name} ( x , y ) VALUES ( @x , @y )", new { x = points[points.Count - 1].Item1, y = points[points.Count - 1].Item2 });

            connection.Close();
        }
        private void removePage_Click(object sender, EventArgs e)
        {
            connection = new SQLiteConnection("Data Source=DB.db;");
            connection.Open();
            string name = tabControl.SelectedTab.Text;
            connection.Execute($@"DROP TABLE {name}");

            connection.Close();
            int index = tabControl.SelectedIndex;
            tabControl.SelectedIndex = 0;
            tabControl.TabPages.RemoveAt(index);
        }

        private void addPage_Click(object sender, EventArgs e)
        {
            TabPage tp;
            if (addTextBox.Text != "")
            {
                tp = new TabPage(addTextBox.Text);
            }
            else
            {
                tp = new TabPage("P");
            }
            tabControl.TabPages.Add(tp);
            Chart ch = new Chart();
            ch.Name = "ch";
            ch.Dock = DockStyle.Fill;
            ch.ChartAreas.Clear();
            ch.ChartAreas.Add(new ChartArea("f(x)"));
            CreateTable(tp.Text);
            PullFromTable(tp.Text);


            ch.ChartAreas[0].AxisX.Crossing = 0;
            ch.ChartAreas[0].AxisY.Crossing = 0;

            ch.ChartAreas[0].AxisX.Minimum = -5;
            ch.ChartAreas[0].AxisY.Minimum = -5;
            ch.ChartAreas[0].AxisX.Maximum = 5;
            ch.ChartAreas[0].AxisY.Maximum = 5;

            ch.ChartAreas[0].AxisY.LineWidth = 2;
            ch.ChartAreas[0].AxisX.LineWidth = 2;

            Series s = new Series("f(x)");
            if (rbSpline.Checked)
                s.ChartType = SeriesChartType.Spline;
            else
                s.ChartType = SeriesChartType.Line;

            ch.Legends.Add(s.Name);
            s.ChartArea = "f(x)";
            s.BorderWidth = 3;

            foreach (var point in points)
            {
                s.Points.AddXY(point.Item1, point.Item2);
            }
            ch.Series.Clear();
            ch.Series.Add(s);

            tp.Controls.Add(ch);
            tp.Controls.SetChildIndex(ch, 5);

            textBox1.Text = "-5";
            textBox2.Text = "-5";
            textBox4.Text = "5";
            textBox3.Text = "5";

        }

        private void addDotButton_Click(object sender, EventArgs e)
        {
            try
            {
                float x = float.Parse(textBoxX.Text);
                float y = float.Parse(textBoxY.Text);
                points.Add((x, y));
            }
            catch (Exception)
            {
                MessageBox.Show("Неверный формат ввода");
                return;
            }
            SaveTable(tabControl.SelectedTab.Text);
            Draw();
        }

        private void newZoneButton_Click(object sender, EventArgs e)
        {
            Chart ch = (Chart)tabControl.SelectedTab.Controls["ch"];
            try
            {
                ch.ChartAreas[0].AxisX.Minimum = double.Parse(textBox1.Text);
                ch.ChartAreas[0].AxisY.Minimum = double.Parse(textBox2.Text);
                ch.ChartAreas[0].AxisX.Maximum = double.Parse(textBox4.Text);
                ch.ChartAreas[0].AxisY.Maximum = double.Parse(textBox3.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Неверный формат ввода");
                return;
            }
            Draw();
        }

        private void rbSpline_CheckedChanged(object sender, EventArgs e)
        {
            Chart ch = (Chart)tabControl.SelectedTab.Controls["ch"];
            ch.Series[0].ChartType = SeriesChartType.Spline;
        }

        private void rbLine_CheckedChanged(object sender, EventArgs e)
        {
            Chart ch = (Chart)tabControl.SelectedTab.Controls["ch"];
            ch.Series[0].ChartType = SeriesChartType.Line;
        }
    }
}
