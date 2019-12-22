﻿using System;
using System.Drawing;
using System.Threading;
using Timers = System.Timers;
using System.Windows.Forms;

namespace WindowsFormsAppPechenka
{
    public partial class PlayForm : Form
    {
        //Для размеров формы и _sizeOfSides для размера ячеек и фигурок
        private readonly int _width = 320;
        private readonly int _height = 320;
        private readonly int _sizeOfSides = 40;
        //*****

        //Любимые массивы(В них хранятся фигурки и их номера)
        public static PictureBox[,] PictureArrfigures = new PictureBox[8, 8];
        public static int[,] NumberArrfigures = new int[8, 8];
        //*****

        public static bool isfiguresdelete;
        //*****

        //Для номеров ячеек, которые меняются местави(Для PictureBox_Click и ReversSwapElements)
        private Point locationfirstfigure;
        private Point locationsecondfigure;
        //*****

        //Для красивых фигурок, которые мы меняем местами
        private PictureBox picturebox1;
        private PictureBox picturebox2;
        //*****

        //Для первого Picturebox, который выделился(необходим для проверок и метода пузырька в PictureBox_Click)
        private PictureBox firstcelectedfigure;
        //*****

        //Для числа содержашегося в массиве NumberArrfigures
        private int valuearrayfirstfigure;
        //*****

        //Стандартный цвет всех PictureBox'ов (Нужно для визуального выделения при шелчке мыши)
        private readonly Color PicBackColor = Color.FromArgb(255, 240, 240, 240);

        Random random = new Random();//Это чтобы фигруки были всегда разные

        private int _gameSecondsLeft = 60;
        private const int GameSecondsLeftWhenResultStart = 0;
        
        private readonly Timers.Timer _timer;
        private readonly Form _mainForm;
        public static readonly Thread Thread;

        public PlayForm(Form mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
            this.Width = _width + 170;
            this.Height = _height + 35;
            MapGenerate();
            ArraysGenerate();

            _timer = new Timers.Timer
            {
                AutoReset = true,
                Interval = 1000,
                Enabled = true
            };
            _timer.Elapsed += TimerTick;
            _timer.Start();
        }
        private void MapGenerate()
        {
            for (int i = 0; i <= _width / _sizeOfSides; i++)
            {
                PictureBox pic = new PictureBox();
                pic.BackColor = Color.Black;
                pic.Location = new Point(0, _sizeOfSides * i);
                pic.Size = new Size(_width, 1);
                this.Controls.Add(pic);
            }
            for (int i = 0; i <= _height / _sizeOfSides; i++)
            {
                PictureBox pic = new PictureBox();
                pic.BackColor = Color.Black;
                pic.Location = new Point(_sizeOfSides * i, 0);
                pic.Size = new Size(1, _height);
                this.Controls.Add(pic);
            }
        }

        private void ArraysGenerate()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    NumberArrfigures[i, j] = random.Next(1, 6);
                    FigureGenerate(j, i, NumberArrfigures[i, j]);
                }
            }
        }

        private void FigureGenerate(int i, int j, int value)
        {
            var picture = Draw.CreateFigure(value, i, j);
            picture.Click += new EventHandler(PictureBox_Click);
            this.Controls.Add(picture);
            PictureArrfigures[i, j] = picture;
            picture.Refresh();
        }

        void moving()
        {
            int counter;
            for (int i = 7; i >= 0; i--)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (NumberArrfigures[i, j] == 0)
                    {
                        counter = 0;
                        //Считает последовательно идущие пустые ячейки
                        while (NumberArrfigures[i - counter, j] == 0)
                        {
                            //Тут не считает элемент на пересечении двух линий нулей
                            counter++;
                            if (i - counter < 0)
                            {
                                break;
                            }
                        }
                        //Опускает/создает ячейки со значением на месте пустых
                        for (int h = 0; h < counter; h++)
                        {
                            InvokeMethod();
                            //Если ячейка 0-го ряда пуста, то генерируется элемент и опускается вниз
                            if (i - counter - h < 0)
                            {
                                for (int u = 0; u < counter; u++)
                                {
                                    if (counter > 4 && u == counter % 2)
                                    {
                                        InvokeMethod();
                                    }
                                    //В случае, когда опускать нечего, мы просто создаем элемент и останавливаем цикл
                                    CreatNewPictureBox(j);
                                    if (counter - 1 - u == 0)
                                    {
                                        break;
                                    }
                                    NumberArrfigures[i - h - u, j] = NumberArrfigures[0, j];
                                    NumberArrfigures[0, j] = 0;

                                    PictureBox picturebox = PictureArrfigures[j, 0];
                                    PictureArrfigures[j, 0] = null;
                                    picturebox.Location = new Point(j * 40, (i - h - u) * 40);
                                    Thread.Sleep(100);
                                    PictureArrfigures[j, i - h - u] = picturebox;
                                }
                                break;
                            }
                            //Опускает
                            else
                            {
                                if (NumberArrfigures[i - counter - h, j] == 0) continue;

                                NumberArrfigures[i - h, j] = NumberArrfigures[i - counter - h, j];
                                NumberArrfigures[i - counter - h, j] = 0;

                                PictureBox picturebox = PictureArrfigures[j, i - counter - h];
                                PictureArrfigures[j, i - counter - h] = null;

                                picturebox.Location = new Point(j * 40, (i - h) * 40);
                                Thread.Sleep(100);
                                PictureArrfigures[j, i - h] = picturebox;
                            }
                        }
                    }
                }
            }
        }

        void CreatNewPictureBox(int y = 0)
        {
            NumberArrfigures[0, y] = random.Next(1, 6);
            FigureGenerate(y, 0, NumberArrfigures[0, y]);
        }

        private void PictureBox_Click(object sender, EventArgs e)
        {
            if (SwapElements(sender))
            {
                CheckAndDeletefigure.CheckingForIdenticalElements();
                if (NumberArrfigures[locationfirstfigure.Y / 40, locationfirstfigure.X / 40] != 0 & NumberArrfigures[locationsecondfigure.Y / 40, locationsecondfigure.X / 40] != 0)
                {
                    Thread.Sleep(150);
                    ReversSwapElements();
                }
                picturebox1 = null;
                picturebox2 = null;
                locationfirstfigure = new Point(0, 0);
                locationsecondfigure = new Point(0, 0);
                firstcelectedfigure = null;
                valuearrayfirstfigure = 0;
                do
                {
                    moving();
                    CheckAndDeletefigure.CheckingForIdenticalElements();
                }
                while (isfiguresdelete);
            }
        }

        bool SwapElements(object sender)  //Метод меняющий местами 2 элемента
        {
            if (firstcelectedfigure == null)
            {
                firstcelectedfigure = (sender as PictureBox);
                locationfirstfigure = firstcelectedfigure.Location;

                valuearrayfirstfigure = NumberArrfigures[locationfirstfigure.Y / 40, locationfirstfigure.X / 40];
                firstcelectedfigure.BackColor = Color.FromArgb(40, 0, 0, 0);
                return false;
            }
            else
            {
                //После проверки исключает варианты, где фигуры расположены не рядом друг с другом |(или) находятся в углу от первой фигуры |(или) клик проихошел по одной фигуре
                if ((Math.Abs(firstcelectedfigure.Location.X - (sender as PictureBox).Location.X) > 40 | Math.Abs(firstcelectedfigure.Location.Y - (sender as PictureBox).Location.Y) > 40) |
                    (Math.Abs(firstcelectedfigure.Location.Y - (sender as PictureBox).Location.Y) == 40 & Math.Abs(firstcelectedfigure.Location.X - (sender as PictureBox).Location.X) == 40) |
                    ((sender as PictureBox).Location == firstcelectedfigure.Location))
                {
                    firstcelectedfigure.BackColor = PicBackColor;
                    firstcelectedfigure = null;
                    return false;
                }
                else //Происходит замена в массивах NumberArrfigures и PictureArrfigures и замена Location у обоих PictureBox'ов 
                {
                    locationsecondfigure = (sender as PictureBox).Location;

                    NumberArrfigures[locationfirstfigure.Y / 40, locationfirstfigure.X / 40] = NumberArrfigures[locationsecondfigure.Y / 40, locationsecondfigure.X / 40];
                    NumberArrfigures[locationsecondfigure.Y / 40, locationsecondfigure.X / 40] = valuearrayfirstfigure;

                    picturebox1 = PictureArrfigures[locationfirstfigure.X / 40, locationfirstfigure.Y / 40];
                    picturebox2 = PictureArrfigures[locationsecondfigure.X / 40, locationsecondfigure.Y / 40];
                    picturebox1.Location = new Point(locationsecondfigure.X, locationsecondfigure.Y);
                    picturebox2.Location = new Point(locationfirstfigure.X, locationfirstfigure.Y);

                    PictureArrfigures[locationsecondfigure.X / 40, locationsecondfigure.Y / 40] = picturebox1;
                    PictureArrfigures[locationfirstfigure.X / 40, locationfirstfigure.Y / 40] = picturebox2;

                    firstcelectedfigure.BackColor = PicBackColor;
                    PictureArrfigures[locationsecondfigure.X / 40, locationsecondfigure.Y / 40].Refresh();
                    PictureArrfigures[locationfirstfigure.X / 40, locationfirstfigure.Y / 40].Refresh();
                    return true;
                }
            }
        }

        void ReversSwapElements()
        {
            valuearrayfirstfigure = NumberArrfigures[locationfirstfigure.Y / 40, locationfirstfigure.X / 40];
            NumberArrfigures[locationfirstfigure.Y / 40, locationfirstfigure.X / 40] = NumberArrfigures[locationsecondfigure.Y / 40, locationsecondfigure.X / 40];
            NumberArrfigures[locationsecondfigure.Y / 40, locationsecondfigure.X / 40] = valuearrayfirstfigure;

            picturebox1.Location = new Point(locationfirstfigure.X, locationfirstfigure.Y);
            picturebox2.Location = new Point(locationsecondfigure.X, locationsecondfigure.Y);
            PictureArrfigures[locationsecondfigure.X / 40, locationsecondfigure.Y / 40] = picturebox2;
            PictureArrfigures[locationfirstfigure.X / 40, locationfirstfigure.Y / 40] = picturebox1;
            PictureArrfigures[locationsecondfigure.X / 40, locationsecondfigure.Y / 40].Refresh();
            PictureArrfigures[locationfirstfigure.X / 40, locationfirstfigure.Y / 40].Refresh();
        }

        public delegate void InvokeDelegate();

        private void TimerTick(Object sourse, Timers.ElapsedEventArgs e)
        {
            labelTime.BeginInvoke(new InvokeDelegate(InvokeMethod));

            if (_gameSecondsLeft == GameSecondsLeftWhenResultStart)
            {
                _timer.Stop();
                _timer.Dispose();
                this.BeginInvoke(new InvokeDelegate(InvokeShowResult));
            }
            else _gameSecondsLeft--;
        }

        public void InvokeMethod()
        {
            labelTime.Text = _gameSecondsLeft.ToString();
            labelTime.Refresh();
        }

        public void InvokeShowResult()
        {
            ResultForm ResultForm = new ResultForm(_mainForm, this,CheckAndDeletefigure.gamepoint);
            ResultForm.ShowDialog();
        }

        private void Playspace_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClosePlayspace(false);
        }

        private void ClosePlayspace(bool isButtonClose = true)
        {
            _timer.Stop();
            _timer.Dispose();
            if (isButtonClose)
            {
                this.Close();
                this.Dispose();
            }
            _mainForm.Visible = true;
        }

        private void ExitToMainFormBotton(object sender, EventArgs e)
        {
            ClosePlayspace();
        }
    }
}
