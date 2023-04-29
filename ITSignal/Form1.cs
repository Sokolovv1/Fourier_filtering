//Реализовать метод фильтрации в частотной
//области зашумленного сигнала с использованием алгоритма быстрого
//преобразования Фурье.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using VisioForge.MediaFramework;

namespace ITSignal
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        double CalculatedAbsoluteComplex(Complex complex)
        {
            return (Math.Sqrt(Math.Pow(complex.Real, 2) + Math.Pow(complex.Imaginary, 2)));
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            HarmonicaBox1.Enabled = false;
            HarmonicaBox2.Enabled = false;
            HarmonicaBox3.Enabled = false;
            int HarmonicsValue = int.Parse(comboBox1.SelectedIndex.ToString());
            HarmonicsValue++;
            if (HarmonicsValue == 1)
            {
                HarmonicaBox1.Enabled = true;
            }
            if (HarmonicsValue == 2)
            {
                HarmonicaBox1.Enabled = true;
                HarmonicaBox2.Enabled = true;
            }
            if (HarmonicsValue == 3)
            {
                HarmonicaBox1.Enabled = true;
                HarmonicaBox2.Enabled = true;
                HarmonicaBox3.Enabled = true;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            chart1.Series["Series1"].Points.Clear();
            chart2.Series["Series1"].Points.Clear();
            chart5.Series["Series1"].Points.Clear();
            chart4.Series["Series1"].Points.Clear();
            chart2.Series["Series2"].Points.Clear();
            chart5.Series["Series2"].Points.Clear();
            double pi = 3.1415926535;
            double amplitude1 = Convert.ToDouble(a1.Text);
            double amplitude2 = Convert.ToDouble(a2.Text);
            double amplitude3 = Convert.ToDouble(a3.Text);
            double F1 = Convert.ToDouble(f1.Text.Replace('.', ','));
            double F2 = Convert.ToDouble(f2.Text);
            double F3 = Convert.ToDouble(f3.Text);
            double Fi1 = Convert.ToDouble(fi1.Text);
            double Fi2 = Convert.ToDouble(fi2.Text);
            double Fi3 = Convert.ToDouble(fi3.Text);
            double Fd = Convert.ToDouble(fd.Text);
            int n = Convert.ToInt32(N.Text); ;
            double dt = 1 / Fd;
            double[] points = new double[n];
            //double[] g = new double[n];
            double[] shum = new double[n];
            double[] AbsoluteCompArray = new double[n];
            double[] ArrayNoise_old = new double[n];
            double[] ArrayNoise_new = new double[n];
            double[] xArrayNoised = new double[n];
            double Eg = 0;
            double Es = 0;
            double Alpha = Convert.ToDouble(alpha.Text);
            double Gamma = Convert.ToDouble(textBox12.Text);
            double df;
            double Espectr = 0;
            int k = 0;
            double g=0;
            
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = amplitude1 * Math.Sin(2 * Math.PI * F1 * dt * i + Fi1) + amplitude2 * Math.Sin(2 * Math.PI * F2 * dt * i + Fi2) + amplitude3 * Math.Sin(2 * Math.PI * F3 * dt * i + Fi3);
                chart1.Series["Series1"].Points.AddXY(i, points[i]);
                chart5.Series["Series1"].Points.AddXY(i, points[i]);
                Es += (points[i]) * (points[i]);
            }
       
            Random rnd = new Random();
            for (int i = 0; i < n; i++)
            {
                g = 0;
                for (int j = 0; j < 12; j++)
                {
                    g += Convert.ToDouble(rnd.Next(-10, 10) / 10.0);
                }
                ArrayNoise_old[i] = g;
                Eg += (g) * (g);
            }
            double betta = Math.Sqrt((Alpha * Es) / (100 * Eg));
            for (int i = 0; i < n; i++)
            {
                ArrayNoise_new[i] = betta * ArrayNoise_old[i];
            }
            for (int i = 0; i < n; i++)
            {
                xArrayNoised[i] = points[i] + ArrayNoise_new[i];
            }
            for (int i = 0; i < n; i++)
            {
                chart4.Series["Series1"].Points.AddXY(i, xArrayNoised[i]);
            }
            Complex[] CompArray;
            if (Math.Log(n, 2) % 1 != 0)
            {
                int Power = (int)Math.Ceiling(Math.Log(n, 2));
                k = (int)Math.Pow(2, Power);
                CompArray = new Complex[k];
            }
            else
            {
                CompArray = new Complex[n];
            }
            
            for (int i = 0; i < n; i++)
            {
                CompArray[i] = new Complex(xArrayNoised[i], 0);
            }
            CompArray = FFT.fft(CompArray);
            df = (Fd / (n - 1));
            for (double i = 0; i < Fd; i += df)
            {
                AbsoluteCompArray[k] = FFT.Absolute(CompArray[k]);
                chart2.Series["Series1"].Points.AddXY(i, AbsoluteCompArray[k]);
                Espectr += Math.Pow(AbsoluteCompArray[k], 2);
                k++;
            }
            double EFFiltered = 0;
            k = 0;
            for (int i = 0; i < n; i++)
            {

                EFFiltered += Math.Pow(AbsoluteCompArray[i], 2) + Math.Pow(AbsoluteCompArray[n - 1 - i], 2);
                k++;
                if (EFFiltered >= Espectr * (Gamma / 100))
                {
                    break;
                }
            }
            //зануление шума
            for (int i = k; i < n - k; i++)
            {
                AbsoluteCompArray[i] = 0;
                CompArray[i] = Complex.Zero;
            }
            k = 0;
            //вывод отфильтрованного спектра
            for (double i = 0; i < Fd; i += df)
            {
                chart2.Series["Series2"].Points.AddXY(i, AbsoluteCompArray[k]);
                k++;
            }
            //обратное преобразование Фурье
            CompArray = FFT.fftRev(CompArray);
            //Вывод отфильтрованного графика
            double[] AbsoluteFilteredArray = new double[n];
            for (int i = 0; i < n; i++)
            {
                AbsoluteFilteredArray[i] = FFT.Absolute(CompArray[i]);
                chart5.Series["Series2"].Points.AddXY(i, CompArray[i].Real);
                
            }
            double Edif = 0;
            for (int i = 0; i < n; i++)
            {
                Edif += Math.Pow((points[i] - CompArray[i].Real), 2);
            }
            textBox13.Text = (Edif / Es).ToString();
        }

     
    }
}
    public class FFT
    {
        private static Complex w(int k, int N)
        {
            if (k % N == 0) return 1;
            double arg = -2 * Math.PI * k / N;
            return new Complex(Math.Cos(arg), Math.Sin(arg));
        }
        public static Complex[] fft(Complex[] x)
        {
            Complex[] X;
            int N = x.Length;
            if (N == 2)
            {
                X = new Complex[2];
                X[0] = x[0] + x[1];
                X[1] = x[0] - x[1];
            }
            else
            {
                Complex[] x_even = new Complex[N / 2];
                Complex[] x_odd = new Complex[N / 2];
                for (int i = 0; i < N / 2; i++)
                {
                    x_even[i] = x[2 * i];
                    x_odd[i] = x[2 * i + 1];
                }
                Complex[] X_even = fft(x_even);
                Complex[] X_odd = fft(x_odd);
                X = new Complex[N];
                for (int i = 0; i < N / 2; i++)
                {
                    X[i] = X_even[i] + w(i, N) * X_odd[i];
                    X[i + N / 2] = X_even[i] - w(i, N) * X_odd[i];
                }
            }
            return X;
        }
        public static double Absolute(Complex complex)
        {
            return (Math.Sqrt(Math.Pow(complex.Real, 2) + Math.Pow(complex.Imaginary, 2)));
        }
        public static Complex[] Conjugate(Complex[] x)
        {
            int N = x.Length;
            for (int i = 0; i < N; i++)
            {
                x[i] = new Complex(x[i].Real, -x[i].Imaginary);
            }
            return x;
        }
        public static Complex[] fftRev(Complex[] x)
        {
            int N = x.Length;
            x = FFT.Conjugate(x);
            x = FFT.fft(x);
            x = FFT.Conjugate(x);
            for (int i = 0; i < N; i++)
            {
                x[i] = new Complex((x[i].Real) / N, (x[i].Imaginary) / N);
            }
            return x;
        }
    }


