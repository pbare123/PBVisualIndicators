using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace PBDialGauge.DialGauge.ComponentModels
{
    public class DialGaugeBase : ComponentBase
    {
        protected ElementReference gauge;
        protected double _toRadious = 90;
        protected double _fromRadious = -90;
        protected double _pointerValue;
        private IJSObjectReference jsTask;

        [Inject]
        protected IJSRuntime JsRuntime { get; set; }

        [Parameter]
        public bool IsRateGauge { get; set; } = false;

        //TO BE REMOVED:   This is only for testing. The Dial Color Offsets should be calculated and assigned automatically based on the DialGoal Value.  The Real DialColorOffsets are commented out below in the Code.
        //[Parameter]
        //public double[] DialColorOffsets { get; set; } = new double[6];

        [Parameter]
        public double DialStartValue { get; set; } = 0;

        [Parameter]
        public double DialEndValue { get; set; } = 100;

        [Parameter]
        public double DialGoal { get; set; } = 86;

        [Parameter]
        public string ID { get; set; }

        [Parameter]
        public double PointerValue
        {
            get => _pointerValue;
            set
            {
                _pointerValue = value;
                Radious = ScaledValue(DialStartValue, DialEndValue, value);
                
            }
        }

        protected double[] GradientScale()
        {
            double goalValue = DialGoal;
            double actualMax = DialEndValue;
            double actualMin = DialStartValue;
            double[] gradients = new double[6];
            double desiredMin = 0;
            double desiredMax = 1;

            double m = (desiredMax - desiredMin) / (actualMax - actualMin);
            double c = (desiredMin - actualMin) * m;

            double gradientgoal = (m * goalValue + c);
            double gradient0 = gradientgoal + .1;
            double gradient1 = gradientgoal - .005;
            double gradient2 = gradientgoal - .15;
            double gradient3 = gradientgoal - .2;
            double gradient4 = gradientgoal - .3;
            double gradient5 = gradientgoal - .4;

            switch (IsRateGauge)
            {
                case true:
                    gradient0 = gradientgoal - .1;
                    gradient1 = gradientgoal - .05;
                    gradient2 = gradientgoal;
                    gradient3 = gradientgoal + .1;
                    gradient4 = gradientgoal + .25;
                    gradient5 = gradientgoal + .3;
                    gradients = new double[]
                    {
                        0,
                        Math.Round(gradient0, 2, MidpointRounding.ToEven),
                        Math.Round(gradient1, 2, MidpointRounding.ToEven),
                        Math.Round(gradient2, 2, MidpointRounding.ToEven),
                        Math.Round(gradient3, 2, MidpointRounding.ToEven),
                        Math.Round(gradient4, 2, MidpointRounding.ToEven),
                        gradient5,
                        1
                    };
                    break;
                case false:
                    gradient0 = gradientgoal + .1;
                    gradient1 = gradientgoal - .005;
                    gradient2 = gradientgoal - .15;
                    gradient3 = gradientgoal - .2;
                    gradient4 = gradientgoal - .3;
                    gradient5 = gradientgoal - .4;

                    gradients = new double[]
                    {
                        0,
                        Math.Round(gradient5, 2, MidpointRounding.ToEven),
                        Math.Round(gradient4, 2, MidpointRounding.ToEven),
                        Math.Round(gradient3, 2, MidpointRounding.ToEven),
                        Math.Round(gradient2, 2, MidpointRounding.ToEven),
                        Math.Round(gradient1, 2, MidpointRounding.ToEven),
                        gradient0,
                        //Math.Round((gradientgoal + .00), 2),
                        1
                    };
                    break;
            }

            return gradients;
        }

        protected double Radious
        {
            get => _toRadious;
            set
            {
                if(value != _toRadious)
                {
                    _fromRadious = _toRadious;
                    _toRadious = value;

                    try { jsTask.InvokeVoidAsync("AnimatePointer", gauge); }
                    catch (Exception e ) { }
                }
            }
        }

        protected double[] DialColorOffsets { get; set; } = new double[6];

        protected double[] DialTicks { get; set; } = new double[11];

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                jsTask = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/PBDialGauge/js/DialGaugeAnimation.js");
                await SetDialTicks();
                var g = GradientScale();
                DialColorOffsets = g;
            }
        }

        private Task SetDialTicks()
        {
            double si = DialStartValue;
            double ei = DialEndValue;
            double mi = (ei - si) / 10d;
            double m = (ei - si) / mi + 1;
            double i = si;
            int s = 0;


            while (s < m)
            {
                DialTicks[s] = i;

                i += mi;

                s++;

            }

            return Task.CompletedTask;
        }

        private double ScaledValue(double actualmin, double actualmax, double value, double desiredmin = -90, double desiredmax = 90)
        {
            return ( desiredmin + (value - actualmin) * (desiredmax - desiredmin) / (actualmax - actualmin));
        }

        protected async Task Animate()
        {
            await jsTask.InvokeVoidAsync("AnimatePointer", gauge);
        }
    }
}
