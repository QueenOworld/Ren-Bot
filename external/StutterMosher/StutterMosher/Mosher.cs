using System;
using System.IO;
using System.Threading.Tasks;

namespace StutterMosher
{
    public class Mosher
    {
        public class ProgressEventArgs : EventArgs
        {
            public double Progress { get; }

            public ProgressEventArgs(double progress)
            {
                Progress = progress;
            }
        }

        private Stream InputStream { get; }
        private Stream OutputStream { get; }

        public event EventHandler<ProgressEventArgs> ProgressChanged;

        public Mosher(Stream inputStream, Stream outputStream)
        {
            InputStream = inputStream;
            OutputStream = outputStream;
        }

        public async Task MoshAsync(int iterations)
        {
            await Task.Run(() => Mosh(iterations));
        }

        protected virtual void OnProgressChanged(double newProgress)
        {
            ProgressChanged?.Invoke(this, new ProgressEventArgs(newProgress));
        }

        private void Mosh(int iterations)
        {
            bool iFrameYet = false;
            while (true)
            {
                Frame frame = Frame.ReadFromStream(InputStream);
                if (frame == null)
                    break;
                if (!iFrameYet)
                {
                    frame.WriteToStream(OutputStream);
                    if (frame.IsIFrame) iFrameYet = true;
                }
                else if (frame.IsPFrame)
                {
                    for (int n = 0; n < iterations; n++)
                        frame.WriteToStream(OutputStream);
                }
                OnProgressChanged((double)InputStream.Position / InputStream.Length);
            }
        }
    }
}
