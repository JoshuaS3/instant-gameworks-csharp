﻿/*  Copyright (c) Joshua Stockin 2018
 *
 *  This file is part of Instant Gameworks.
 *
 *  Instant Gameworks is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Instant Gameworks is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Instant Gameworks.  If not, see <http://www.gnu.org/licenses/>.
 */


using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

using InstantGameworks;
using InstantGameworks.Graphics;
using InstantGameworks.Graphics.GameObjects;


namespace InstantGameworks
{
    class Init
    {
        private static GameworksWindow GameWindow;
        private static IntPtr GameWindowHandle;

        private static Vector2 GameWindowSize;
        private static Vector2 GameWindowPosition;
        private static WindowBorder GameWindowBorder;
        private static WindowState GameWindowState;
        private static float GameWindowRefreshRate;

        [STAThread]
        private static void CreateGameworksWindow()
        {
            GameWindow = new GameworksWindow();
            GameWindowHandle = GameWindow.WindowInfo.Handle;

            GameWindow.Width = (int)GameWindowSize.X;
            GameWindow.Height = (int)GameWindowSize.Y;
            GameWindow.WindowBorder = GameWindowBorder;
            GameWindow.WindowState = GameWindowState;
            GameWindow.X = (int)GameWindowPosition.X;
            GameWindow.Y = (int)GameWindowPosition.Y;

            GameWindow.VSync = VSyncMode.On;

            GameWindow.CursorVisible = false;
            GameWindow.Run(GameWindowRefreshRate, GameWindowRefreshRate);
        }

        [STAThread]
        public static void Main()
        {
            Console.Title = "Instant Gameworks";
            Console.WriteLine("Instant Gameworks (c)  2018");

            Logging.LogEvent("Main thread startup");

            // Set window settings
            DisplayDevice DefaultDisplay = DisplayDevice.Default;
            GameWindowRefreshRate = 0;//DefaultDisplay.RefreshRate;
            GameWindowSize = new OpenTK.Vector2(1920, 1080);
            GameWindowPosition = new OpenTK.Vector2(0, 0);
            GameWindowBorder = WindowBorder.Hidden;
            GameWindowState = WindowState.Fullscreen;

            // Create window
            Logging.LogEvent("Initializing GameworksWindow");

            ThreadStart GameThread = new ThreadStart(CreateGameworksWindow);
            Thread RunGame = new Thread(GameThread);
            RunGame.Start();

            // Wait for window to initialize
            SpinWait.SpinUntil(() => GameWindow != null && GameWindow.Exists);





            //
            //
            //
            // Game logic
            //
            //
            //

            Logging.LogEvent("Importing resources");
            
            // Initialize camera
            StudioCamera Camera = new StudioCamera
            {
                MoveSensitivity = 0.01f
            };
            GameWindow.Camera = Camera;

            // Establish lighting
            StudioCamera SunLook = new StudioCamera();
            SunLook.Orientation = new Vector2(0, (float)-Math.PI/2); // look down
            var Sun = GameWindow.AddDirectionalLight();
            Sun.Name = "Sun";
            Sun.RelativeDirection = SunLook.LookAt;
            Sun.Intensity = 128;
            Sun.Enabled = true;
            Sun.DiffuseColor = Color4.Black;

            // Import objects
            var Land = GameWindow.AddObject(@"Testing\dragon.igwo");
            Land.DiffuseColor = Color4.DarkRed;
            Land.AmbientColor = new Color4(30, 0, 0, 0);
            Land.SpecularColor = Color4.White;
            Land.Scale = new Vector3(1.5f, 1.5f, 1.5f);

            var GuiHolder = GameWindow.AddGui();
            GuiHolder.Color = new Color4(128, 128, 128, 255);
            GuiHolder.AbsoluteSize = new Vector2(32f / 1920f, 2f / 1080f);
            GuiHolder.AbsolutePosition = new Vector2(0.5f - 16f / 1920f, 0.5f - 1f / 1080f);


            var G2 = GameWindow.AddGui();
            G2.Color = new Color4(128, 128, 128, 255);
            G2.AbsoluteSize = new Vector2(2f / 1920f, 32f / 1080f);
            G2.AbsolutePosition = new Vector2(0.5f - 1f / 1920f, 0.5f - 16f / 1080f);

            




            double _lastTime = 0;
            double _time = 0;
            float AdjustedSpeedForFramerate = 1f;
            void OnUpdateFrameTimer(object sender, FrameEventArgs e)
            {
                _lastTime = _time;
                _time += e.Time;
            }
            void ObjectUpdateFrame(object sender, FrameEventArgs e)
            {
                Sun.RelativeDirection = SunLook.LookAt;
                Land.Rotation += new Vector3(0, 0.005f * AdjustedSpeedForFramerate, 0);
                GuiHolder.Color = Color4.FromHsv(new Vector4(DateTime.Now.Millisecond / 1000f, 0.8f, 1f, 0.5f));
                G2.Color = Color4.FromHsv(new Vector4(DateTime.Now.Millisecond / 1000f, 0.8f, 1f, 0.5f));
            }

            // Camera implementation
            Dictionary<Key, bool> KeysDown = new Dictionary<Key, bool>() { [Key.W] = false, [Key.A] = false, [Key.S] = false, [Key.D] = false };
            
            OpenTK.Vector2 LastMousePosition = new OpenTK.Vector2(0, 0);
            bool IsRightMouseDown = false;
            bool IsLeftMouseDown = false;
            bool IsSettingMousePosition = false;

            Vector2 CurrentMousePosition = new Vector2(0, 0);
            void CameraUpdateFrame(object sender, FrameEventArgs e)
            {
                if (IsRightMouseDown || IsLeftMouseDown)
                {
                    IsSettingMousePosition = true;
                    Mouse.SetPosition(LastMousePosition.X + GameWindow.X, LastMousePosition.Y + GameWindow.Y);
                }

                AdjustedSpeedForFramerate = 144f / (1f / ((float)_time - (float)_lastTime));
                if (KeysDown[Key.W] == true)
                {
                    Camera.Move(0, 0, -AdjustedSpeedForFramerate);
                }
                if (KeysDown[Key.A] == true)
                {
                    Camera.Move(AdjustedSpeedForFramerate, 0, 0);
                }
                if (KeysDown[Key.S] == true)
                {
                    Camera.Move(0, 0, AdjustedSpeedForFramerate);
                }
                if (KeysDown[Key.D] == true)
                {
                    Camera.Move(-AdjustedSpeedForFramerate, 0, 0);
                }

                Vector2 mousePos = new Vector2(CurrentMousePosition.X + GameWindow.X, CurrentMousePosition.Y + GameWindow.Y);
                GuiHolder.AbsolutePosition = new Vector2((mousePos.X/1920f) - (16 / 1920f), (mousePos.Y/1080f) - (1 / 1080f));
                G2.AbsolutePosition = new Vector2((mousePos.X/1920f) - (1 / 1920f), (mousePos.Y/1080f) - (16 / 1080f));
            }


            void MouseDown(object sender, MouseButtonEventArgs e)
            {
                if (e.Button == MouseButton.Right)
                {
                    if (!IsRightMouseDown)
                    {
                        IsRightMouseDown = true;
                        LastMousePosition = new OpenTK.Vector2(e.X, e.Y);
                    }
                }
                if (e.Button == MouseButton.Left)
                {
                    if (!IsLeftMouseDown)
                    {
                        IsLeftMouseDown = true;
                        LastMousePosition = new OpenTK.Vector2(e.X, e.Y);
                    }
                }
            }
            void MouseUp(object sender, MouseButtonEventArgs e)
            {
                if (e.Button == MouseButton.Right)
                {
                    IsRightMouseDown = false;
                }
                if (e.Button == MouseButton.Left)
                {
                    IsLeftMouseDown = false;
                }
            }
            void MouseMove(object sender, MouseMoveEventArgs e)
            {
                if (GameWindow.Focused && IsRightMouseDown && !IsSettingMousePosition)
                {
                    Camera.AddRotation(e.XDelta, e.YDelta);
                }
                if (GameWindow.Focused && IsLeftMouseDown && !IsSettingMousePosition)
                {
                    SunLook.AddRotation(e.XDelta, e.YDelta);
                }
                IsSettingMousePosition = false;
                CurrentMousePosition = new Vector2(e.X, e.Y);
            }
            void KeyDown(object sender, KeyboardKeyEventArgs e)
            {
                switch (e.Key)
                {
                    case Key.W:
                        KeysDown[Key.W] = true;
                        break;
                    case Key.A:
                        KeysDown[Key.A] = true;
                        break;
                    case Key.S:
                        KeysDown[Key.S] = true;
                        break;
                    case Key.D:
                        KeysDown[Key.D] = true;
                        break;
                    case Key.Escape:
                        GameWindow.Exit();
                        break;
                }
            }
            void KeyUp(object sender, KeyboardKeyEventArgs e)
            {
                switch (e.Key)
                {
                    case Key.W:
                        KeysDown[Key.W] = false;
                        break;
                    case Key.A:
                        KeysDown[Key.A] = false;
                        break;
                    case Key.S:
                        KeysDown[Key.S] = false;
                        break;
                    case Key.D:
                        KeysDown[Key.D] = false;
                        break;
                }
            }
            void MouseWheel(object sender, MouseWheelEventArgs e)
            {
                Camera.Move(0, 0, -e.Delta / Camera.MoveSensitivity * 0.25f);
            }

            //assign OnUpdateFrame
            Logging.LogEvent("Adding update frame events");
            GameWindow.UpdateFrame += OnUpdateFrameTimer;
            GameWindow.UpdateFrame += ObjectUpdateFrame;
            GameWindow.UpdateFrame += CameraUpdateFrame;

            //assign input events
            Logging.LogEvent("Adding input events");
            GameWindow.MouseDown += MouseDown;
            GameWindow.MouseUp += MouseUp;
            GameWindow.MouseMove += MouseMove;
            GameWindow.KeyDown += KeyDown;
            GameWindow.KeyUp += KeyUp;
            GameWindow.MouseWheel += MouseWheel;


            //Exit
            RunGame.Join();
            NativeMethods.ConsoleApp.ShowConsole();
            Logging.LogEvent("Shutting down");

            /*Logging.WriteToFile();
            Logging.DisplayLog().Join();
            *///end of thread
        }

    }

}
