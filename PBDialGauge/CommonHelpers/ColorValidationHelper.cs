using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PBDialGauge.CommonHelpers
{
    public class ColorValidationHelper : IColorValidationHelper
    {
        
        private readonly IJSRuntime Js;
        private IJSObjectReference jsTask;

        public ColorValidationHelper(IJSRuntime Js)
        {
            this.Js = Js;
        }

        public async Task<bool> ValidateColor(Task<string[]> colors)
        {
            string[] myColors = await colors;
            bool output = false;
            List<bool> iSValidColors = new();
            jsTask = await Js.InvokeAsync<IJSObjectReference>("import", "./_content/PBDialGauge/js/DialGaugeAnimation.js");
            foreach (string color in myColors)
            {
                try
                {
                    var t = await jsTask.InvokeAsync<bool>("ValitateColor", color);
                    iSValidColors.Add(t);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e, e.Message);
                }
            }

            output = iSValidColors.All(x => x);
            return output;
        }
    }
}
