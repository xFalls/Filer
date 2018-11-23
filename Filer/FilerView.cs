using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;

namespace ReFiler
{
    public class FilerView
    {
        private FilerModel model;
        public MainPage window;

        public FilerView(FilerModel model, MainPage window)
        {
            this.model = model;
            this.window = window;
        }


    }
}