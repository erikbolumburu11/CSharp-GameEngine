using System;
﻿using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace GameEngine.Engine
{
    public class VertexArray
    {
        public int Handle { get; private set; }

        public VertexArray()
        {
            Handle = GL.GenVertexArray();
        }

        public void Bind() => GL.BindVertexArray(Handle);
        public void Unbind() => GL.BindVertexArray(0);

        public void Delete() => GL.DeleteVertexArray(Handle);
    }

    public class VertexBuffer<T> where T : struct
    {
        public int Handle { get; private set; }

        public VertexBuffer(T[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
        {
            Handle = GL.GenBuffer();
            Bind();
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * Marshal.SizeOf<T>(), data, usage);
            Unbind();
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
        public void Unbind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        public void Delete() => GL.DeleteBuffer(Handle);
    }

    public class SSBO
    {
        public int Handle { get; private set; }

        public SSBO(int sizeBytes)
        {
            Handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Handle);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }

        public void Update<T>(T[] data) where T : struct
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Handle);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, data.Length * Marshal.SizeOf<T>(), data);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }

        public void Bind(int binding)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding, Handle);
        }
    }


    public class Framebuffer : IDisposable
    {
        public int Handle { get; private set; }
        public int ColorTexture { get; private set; }
        public int DepthRenderbuffer { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Framebuffer(int width, int height)
        {
            Width = width;
            Height = height;

            Handle = GL.GenFramebuffer();
            Bind();

            CreateColorAttachment();
            CreateDepthAttachment();

            CheckStatus();
            Unbind();
        }

        public void Bind() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
        public static void Unbind() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        public void BindColorTexture(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, ColorTexture);
        }

        public void Resize(int width, int height)
        {
            if (width == Width && height == Height) return;

            Width = width;
            Height = height;

            Bind();

            GL.BindTexture(TextureTarget.Texture2D, ColorTexture);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba8,
                Width,
                Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                IntPtr.Zero
            );
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthRenderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, Width, Height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            CheckStatus();
            Unbind();
        }

        public void Delete()
        {
            if (DepthRenderbuffer != 0)
            {
                GL.DeleteRenderbuffer(DepthRenderbuffer);
                DepthRenderbuffer = 0;
            }

            if (ColorTexture != 0)
            {
                GL.DeleteTexture(ColorTexture);
                ColorTexture = 0;
            }

            if (Handle != 0)
            {
                GL.DeleteFramebuffer(Handle);
                Handle = 0;
            }
        }

        public void Dispose() => Delete();

        void CreateColorAttachment()
        {
            ColorTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ColorTexture);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba8,
                Width,
                Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                IntPtr.Zero
            );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                ColorTexture,
                0
            );
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        void CreateDepthAttachment()
        {
            DepthRenderbuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthRenderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, Width, Height);
            GL.FramebufferRenderbuffer(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                RenderbufferTarget.Renderbuffer,
                DepthRenderbuffer
            );
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        }

        void CheckStatus()
        {
            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new InvalidOperationException($"Framebuffer incomplete: {status}");
        }
    }

}
