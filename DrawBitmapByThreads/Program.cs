using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;

namespace DrawBitmapByThreads
{
    //todo read about partitioners
    
    class Program
    {
        static void Main(string[] args)
        {
            var bmp = new Bitmap(1024, 1024);
            var colors = new Color[1024, 1024];

            for (int j = 0; j < 1024; j++)
            {
                Enumerable.Range(0, 1024 * 1024)
                    .AsParallel()
                    .WithDegreeOfParallelism(6)
                    .ForAll((i) =>
                    {
                        var id = Thread.CurrentThread.ManagedThreadId;
                        var color = Color.FromArgb(8 * id, 8 * id, 8 * id);
                        colors[j, i] = color;
                    });
            }

            for (int j = 0; j < 1024; j++)
            for (int i = 0; i < 1024; i++)
            {
                bmp.SetPixel(j, i, colors[j,i]);
            }

            bmp.Save(
                @"C:\Users\denis\Desktop\Kontur.Shpora.2021.Public\ReaderWriterLock\result.bmp");
        }
    }
}