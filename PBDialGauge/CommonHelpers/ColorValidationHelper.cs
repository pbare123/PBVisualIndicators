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
        [Inject]
        public IJSRuntime Js { get; set; }
        private IJSObjectReference jsTask;

        public ColorValidationHelper()
        {
            //ValueTask t = Js.InvokeAsync<IJSObjectReference>("import", "./_content/PBDialGauge/js/DialGaugeAnimation.js");
        }

        public async Task<bool> ValidateColor(Task<string[]> colors)
        {
            bool output = false;
            List<bool> iSValidColors = new();
            jsTask = await Js.InvokeAsync<IJSObjectReference>("import", "./_content/PBDialGauge/js/DialGaugeAnimation.js");
            foreach (string color in colors.Result)
            {
                try
                {
                    var t = jsTask.InvokeAsync<bool>("ValitateColor", color);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e, e.Message);
                }
                
                //iSValidColors.Add((bool)result);
            }

            output = !iSValidColors.All(x => x);
            return output;
        }
    }
}
