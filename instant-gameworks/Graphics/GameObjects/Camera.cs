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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace InstantGameworks.Graphics.GameObjects
{
    public interface ICamera
    {
        Vector3 Position { get; set; }
        Vector2 Orientation { get; set; }
        Matrix4 PerspectiveMatrix { get; set; }
        float AspectRatio { get; set; }

        Matrix4 Update();
    }

    public class Camera : ICamera
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 LookAt
        {
            get
            {
                return new Vector3(
                    (float)(System.Math.Sin(Orientation.X) * System.Math.Cos(Orientation.Y)),
                    (float)System.Math.Sin(Orientation.Y),
                    (float)(System.Math.Cos(Orientation.X) * System.Math.Cos(Orientation.Y))
                    );
            }
        }
        public Vector2 Orientation { get; set; } = new OpenTK.Vector2(0, 0);
        public Matrix4 PerspectiveMatrix { get; set; } = new Matrix4();
        public float AspectRatio { get; set; } = 16f / 9f;

        public float FieldOfView { get; set; } = 80;
        public float MinViewDistance { get; set; } = 0.001f;
        public float MaxViewDistance { get; set; } = 4000;


        public Camera()
        {
            Position = Vector3.Zero;
            Orientation = new OpenTK.Vector2((float)System.Math.PI, 0);
        }
        public Camera(Vector3 position)
        {
            Position = position;
            Orientation = new OpenTK.Vector2();
        }
        public Camera(Vector3 position, OpenTK.Vector2 orientation)
        {
            Position = position;
            Orientation = orientation;
        }


        float Radian = (float)System.Math.PI / 180f;
        public Matrix4 Update()
        {
            PerspectiveMatrix = Matrix4.LookAt(Position, Position + LookAt, Vector3.UnitY) * Matrix4.CreatePerspectiveFieldOfView(FieldOfView * Radian, AspectRatio, MinViewDistance, MaxViewDistance);
            return PerspectiveMatrix;
        }

    }

    public class StudioCamera : Camera
    {
        public float MoveSensitivity { get; set; } = 0.3f;
        public float LookSensitivity { get; set; } = 0.0025f;

        public void AddRotation(float DeltaX, float DeltaY)
        {
            DeltaX *= -LookSensitivity;
            DeltaY *= -LookSensitivity;

            Orientation = new OpenTK.Vector2(
                (float)((Orientation.X + DeltaX) % ((float)System.Math.PI * 2f)),
                System.Math.Max(
                    System.Math.Min(
                        (float)(Orientation.Y + DeltaY),
                        (float)System.Math.PI / 2f - 0.01f
                    ),
                    (float)-System.Math.PI / 2f + 0.01f
                )
            );
        }
        public void Move(float x, float y, float z)
        {
            Vector3 offset = new Vector3();

            Vector3 forward = new Vector3(-(float)System.Math.Sin(Orientation.X), -(float)System.Math.Tan(Orientation.Y), -(float)System.Math.Cos(Orientation.X));
            forward.NormalizeFast();
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);
            right.NormalizeFast();
            Vector3 up = new Vector3((float)System.Math.Sin(Orientation.Y), (float)System.Math.Tan(Orientation.Y), (float)System.Math.Cos(Orientation.Y));
            up.NormalizeFast();

            offset += x * right;
            offset += y * up;
            offset += z * forward;
            
            offset = Vector3.Multiply(offset, MoveSensitivity);

            Position += offset;
        }
    }
}
