using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes; 

namespace Hotel_Vasanov_ISP43
{
    public class Captcha
    {
        private Random random = new Random();
        private string answer = "";

        public void Refresh(Border border)
        {
            string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            answer = "";
            for (int i = 0; i < 6; i++)
            {
                answer += alphabet[random.Next(alphabet.Length)];
            }

            Grid grid = new Grid();
            grid.ClipToBounds = true;

            TextBlock text = new TextBlock();
            text.Text = answer;
            text.FontSize = 24;
            text.Foreground = Brushes.Black;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.RenderTransform = new RotateTransform(random.Next(-12, 13));
            grid.Children.Add(text);

            for (int i = 0; i < 5; i++)
            {
                Line line = new Line();
                line.X1 = random.Next(240);
                line.Y1 = random.Next(60);
                line.X2 = random.Next(240);
                line.Y2 = random.Next(60);
                line.Stroke = Brushes.SlateGray;
                line.StrokeThickness = 1;
                grid.Children.Add(line);
            }

            border.Background = Brushes.LightSteelBlue;
            border.Child = grid;
        }

        public bool Matches(string input)
        {
            return answer != "" && input.Trim() == answer;
        }
    }
}
