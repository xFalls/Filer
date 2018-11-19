using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Universal.WebP;

namespace MediFiler
{
    public class FilerModel
    {
        public List<Content> ContentList = new List<Content>();
        public int ContentPosition;


        public FilerModel()
        {

        }

        /*public WriteableBitmap ConvertWebPtoBitmap()
        {

        }*/
    }
}