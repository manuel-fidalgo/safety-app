﻿using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

namespace Safe
{
    public class MainPage : CustomPage
    {

        static readonly int MAX_MILISECONDS = 3000;

        SettingsPage settings_page;

        NavigationPage nav_page;
        SKCanvasView activityview;
        SKCanvasView settingsview;

        int angle;
        bool animation_runing;

        public static MainPage singleton;



        public MainPage()
        {
            initColors();
            angle = 0;

            settings_page = new SettingsPage();

            nav_page = new NavigationPage(this);
            NavigationPage.SetHasNavigationBar(this, false);

            var tapGestureRecognizer = new TapGestureRecognizer();
            var tapGestureRecognizer_settings = new TapGestureRecognizer();
            var tapGestureRecognizer_settings_double_tap = new TapGestureRecognizer();

            activityview = new SKCanvasView();
            settingsview = new SKCanvasView();

            //Tapped events
            tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
            tapGestureRecognizer_settings.Tapped += TapGestureRecognizer_settings_Tapped;
            tapGestureRecognizer_settings_double_tap.Tapped += TapGestureRecognizer_settings_double_tap_Tapped;
            //Nuber os taps
            tapGestureRecognizer.NumberOfTapsRequired = 2;
            tapGestureRecognizer_settings_double_tap.NumberOfTapsRequired = 2;



            activityview.PaintSurface += View_PaintSurface;
            settingsview.PaintSurface += Settingsview_PaintSurface;

            activityview.GestureRecognizers.Add(tapGestureRecognizer);
            settingsview.GestureRecognizers.Add(tapGestureRecognizer_settings);
            settingsview.GestureRecognizers.Add(tapGestureRecognizer_settings_double_tap);


            int uheigth = (int)(Height / 16);
            int height = (int)Height;
            int width = (int)Width;

            AbsoluteLayout l = new AbsoluteLayout();

            AbsoluteLayout.SetLayoutBounds(activityview, new Rectangle(0, 0, 1, 0.75));
            AbsoluteLayout.SetLayoutFlags(activityview, AbsoluteLayoutFlags.All);

            AbsoluteLayout.SetLayoutBounds(settingsview, new Rectangle(0, 1, 1, 0.25));
            AbsoluteLayout.SetLayoutFlags(settingsview, AbsoluteLayoutFlags.All);

            l.Children.Add(activityview);
            l.Children.Add(settingsview);

            Content = l;

            singleton = this;
        }

        private void TapGestureRecognizer_settings_double_tap_Tapped(object sender, EventArgs e)
        {
            if (animation_runing)
            {
                VibrationManager.vibrate(500);
                FinishAnimation();
            }
        }

        private void StartAnimation()
        {
            Task.Factory.StartNew(
              () => Animation(),
              CancellationToken.None,
              TaskCreationOptions.None,
              TaskScheduler.FromCurrentSynchronizationContext()
              );
        }
        private void FinishAnimation()
        {
            animation_runing = false;
            angle = 0;
            settingsview.InvalidateSurface();
            activityview.InvalidateSurface();
        }

        private async Task Animation()
        {

            animation_runing = true;
            settingsview.InvalidateSurface();
            while (animation_runing)
            {
                if (angle >= 360)
                {
                    angle = 0;
                    FinishAnimation();
                    AlertMode();
                }
                else
                {
                    angle++; //Update the angle
                }
                activityview.InvalidateSurface(); //Repaints the surface each second
                await Task.Delay(MAX_MILISECONDS / 360); //One cycle for each 3 seconds
            }
        }

        public void AlertMode()
        {

            var source = new CancellationTokenSource();
            CancellationToken ct = source.Token;

            Task t = Task.Factory.StartNew(
               () => Vibrate(ct),
               ct,
               TaskCreationOptions.None,
               TaskScheduler.FromCurrentSynchronizationContext()
               );

            Task l = Task.Factory.StartNew(
               () => ShowDialog(t, ct, source),
               CancellationToken.None,
               TaskCreationOptions.None,
               TaskScheduler.FromCurrentSynchronizationContext()
               );

        }
        //shows a dialog and waits for a response. if the user closes the notification will cancell the other task
        private async Task ShowDialog(Task t, CancellationToken ct, CancellationTokenSource cts)
        {

            var answer = await DisplayAlert(AppResources.alert, AppResources.alert_question, AppResources.yes, AppResources.no);
            //Have an answer, lets cancell the other task
            cts.Cancel();

            if (!answer) //The person needs help
            {
                var answer2 = await DisplayAlert(AppResources.info, AppResources.send_message, AppResources.yes, AppResources.no);
                if (answer2)
                {   //Sends the message
                    MessageManager.sendAlertMessage();
                }
            }
        }
        //Will vibrate during n seconds, if this task is not cancelled will send the message
        private async Task Vibrate(CancellationToken ct)
        {
            System.Diagnostics.Debug.WriteLine("Vibration task init.");
            for (int i = 0; i < 10; i++)
            {
                if (ct.IsCancellationRequested) return;
                await Task.Delay(1000);
                VibrationManager.vibrate(900);
            }
            if (!ct.IsCancellationRequested)
            {
                System.Diagnostics.Debug.WriteLine("Vibration task has not been cancelled, send message");
                MessageManager.sendAlertMessage();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Vibration task has been cancelled");
            }
        }


        private void TapGestureRecognizer_settings_Tapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(settings_page);
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (!animation_runing)
                StartAnimation();
            else
                angle = 0;
        }

        //w 8 divisions, h 4 divisions
        private void Settingsview_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            int width, heigth, uheigth, uwidth;

            width = e.Info.Width;
            heigth = e.Info.Height;
            uheigth = heigth / 4;
            uwidth = width / 8;

            using (SKPaint paint = new SKPaint())
            {
                paint.IsAntialias = true;

                var shader = SKShader.CreateColor(GRADIENT1);
                paint.Shader = shader;
                canvas.DrawPaint(paint);

                //Line
                paint.Shader = SKShader.CreateColor(TEXT);
                for (int i = 0; i < 3; i++) canvas.DrawLine(uwidth, 0 + i, uwidth * 7, 0 + i, paint);

                //Texts
                paint.Style = SKPaintStyle.Fill;
                paint.TextAlign = SKTextAlign.Center;
                paint.Shader = SKShader.CreateColor(TEXT);
                if (!animation_runing)
                {
                    paint.TextSize = (int)(1.5 * uheigth);
                    canvas.DrawText(AppResources.settings, width / 2, (int)(uheigth * 2.5), paint);
                }
                else
                {
                    paint.TextSize = (uheigth);
                    canvas.DrawText(AppResources.stop_message, width / 2, (int)(uheigth * 2.5), paint);
                }

            }
        }

        //w 8 divisions, h 12 divisions
        private void View_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            int surfaceWidth, surfaceHeight, uheigth, uwidth;

            surfaceWidth = e.Info.Width;
            surfaceHeight = e.Info.Height;

            uheigth = surfaceHeight / 14;
            uwidth = surfaceWidth / 8;


            int middle_width = e.Info.Width / 2;
            int middle_heigth = 5 * uheigth;
            int radius = 4 * uheigth;

            using (SKPaint paint = new SKPaint())
            {
                canvas.Clear();

                //Gradient
                createGradient(canvas, paint, surfaceWidth, surfaceHeight);

                //Circle

                paint.Shader = SKShader.CreateColor(TEXT);
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 5;
                canvas.DrawCircle(middle_width, middle_heigth, radius, paint);
                paint.StrokeWidth = 1;

                paint.Style = SKPaintStyle.StrokeAndFill;

                //Text tap twice
                paint.TextAlign = SKTextAlign.Center;
                paint.TextSize = uheigth;
                if (animation_runing)
                    canvas.DrawText(AppResources.tap_twice, middle_width, 12 * uheigth, paint);
                else
                    canvas.DrawText(AppResources.tap_start, middle_width, 12 * uheigth, paint);

                //External dot
                double angle_rad = (angle - 90) * 0.0174533;
                double x = middle_width + radius * Math.Cos(angle_rad);
                double y = middle_heigth + radius * Math.Sin(angle_rad);

                if (angle > 270)
                {
                    paint.TextSize = 2 * uheigth;
                    paint.TextAlign = SKTextAlign.Center;
                    paint.Shader = SKShader.CreateColor(BALL);
                    canvas.DrawText(AppResources.click_fast, middle_width, (int)(uheigth * 5.5), paint);
                }
                paint.Shader = SKShader.CreateColor(BALL);
                canvas.DrawCircle((int)x, (int)y, 15, paint);

            }
        }

        private void displayLayout(SKCanvas canvas, SKPaint paint, int uheigth, int uwidth)
        {
            for (int i = 0; i < 8; i++)
            {
                canvas.DrawLine(uwidth * i, 0, uwidth * i, uheigth * 16, paint); //Vertical
            }
            for (int i = 0; i < 14; i++)
            {
                canvas.DrawLine(0, uheigth * i, uwidth * 8, uheigth * i, paint); //horizontal
            }
        }
    }
}

