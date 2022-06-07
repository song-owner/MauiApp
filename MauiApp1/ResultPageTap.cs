using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1
{
    public partial class ResultPage
    {
        private void appTapped(object sender, EventArgs e)
        {
            if (slider.Value == 0 || slider.Value == 1) googlePageOpen(); else appPageOpen();
        }
        private void googleTapped(object sender, EventArgs e)
        {
            if (slider.Value == 0) appPageOpen(); else googlePageOpen();
        }
        void googlePageOpen()
        {
            GooglePage.Width = GridLength.Star;
            AppPge.Width = 0;
            slider.Value = AppPge.Width.Value / resultPage.Width;
        }
        void appPageOpen()
        {
            AppPge.Width = GridLength.Star;
            GooglePage.Width = 0;
            slider.Value = (resultPage.Width - GooglePage.Width.Value) / resultPage.Width;
        }
        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            var width = resultPage.Width;
            sliderValue = e.NewValue;
            AppPge.Width = width * sliderValue;
            GooglePage.Width = GridLength.Star;
        }
    }
}
