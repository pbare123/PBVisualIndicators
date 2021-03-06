using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PBDialGauge.CommonHelpers;
using System;
using System.Threading.Tasks;

namespace PBVisualIndicators.DialGauge.ComponentModels
{
    /// <summary>
    /// 
    /// </summary>
    public class DialGaugeBase : ComponentBase
    {
        protected ElementReference gauge;
        protected double _toRadious = 90;
        protected double _fromRadious = -90;
        protected double _pointerValue;
        protected string[] _dialColors = new string[] { "Red", "Orange", "Yellow", "Lime" };
        private IJSObjectReference jsTask;

        [Inject]
        protected IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// Sets State Of Gauge to be of Rate in nature
        /// <remarks>optional: Default is false</remarks>
        /// </summary>
        [Parameter]
        public bool IsRateGauge { get; set; } = false;

        /// <summary>
        /// <c>Colors</c> Get Or Set Colors used for Dial Colors in Order as presented
        /// <example>Example: <code>@bind-Colors="Colors" </code></example>
        /// <example><code>private string[] Colors {get; set;} = new string[]{"red", "orange", "yellow" "green"}; </code></example>
        /// <exception cref="member"> Invalid Colors will result with the default colors </exception>
        /// <remarks>Defaults are red, orange, yellow, green</remarks>
        /// </summary>
        [Parameter]
        public string[] Colors
        {
            get => _dialColors;
            set
            {
                _dialColors = value;
            }
        }
        /// <summary>
        /// Binds the default dial colors if supplied colors are invalid 
        /// </summary>
        [Parameter]
        public EventCallback<string[]> ColorsChanged { get; set; }

        [Parameter]
        public EventCallback<double> DialStartValueChanged { get; set; }

        [Parameter]
        public EventCallback<double> DialEndValueChanged { get; set; }

        /// <summary>
        /// The lowerend value of the Gauge Range Scale
        /// <remarks>Optional:  Default is 0</remarks>
        /// </summary>
        [Parameter]
        public double DialStartValue { get; set; } = 0;

        /// <summary>
        /// The Higherend Value of the Gauge Range Scale
        /// <remarks>optional: Default is 100</remarks>
        /// </summary>
        [Parameter]
        public double DialEndValue { get; set; } = 100;

        /// <summary>
        /// The Lowest Target Value for the pointer
        /// <remarks>Optional: Default is 86</remarks>
        /// </summary>
        [Parameter]
        public double DialGoal { get; set; } = 86;

        /// <summary>
        /// The Identifier for the current Component
        /// </summary>
        [Parameter]
        public string ID { get; set; }

        /// <summary>
        /// Te Value used to Command the Pointer
        /// </summary>
        [Parameter]
        public double PointerValue
        {
            get => _pointerValue;
            set
            {
                _pointerValue = value;

                VlidatePointerValue(value);
                Radious = ScaledValue(DialStartValue, DialEndValue, value);

            }
        }

        private async void VlidatePointerValue(double value)
        {
            if (value > DialEndValue || value < DialStartValue)
            {
                if (value > DialEndValue)
                {
                    DialEndValue = Math.Round(value + 10, 0);
                    await DialEndValueChanged.InvokeAsync(DialEndValue);
                }
                await SetupGauge();

            }
        }

        protected double[] GradientScale()
        {
            double goalValue = DialGoal;
            double actualMax = DialEndValue;
            double actualMin = DialStartValue;
            double[] gradients = new double[4];
            double desiredMin = 0;
            double desiredMax = 1;

            double gradientgoal = ScaledValue(actualMin, actualMax, goalValue, desiredMin, desiredMax);

            double gradient0;
            double gradient1;
            double gradient2;
            double gradient3;

            switch (IsRateGauge)
            {
                case false:
                    gradient0 = gradientgoal - .3;
                    gradient1 = gradientgoal - .2;
                    gradient2 = gradientgoal - .08;
                    gradient3 = 1;
                    break;
                case true:
                    gradient0 = gradientgoal + .08;
                    gradient1 = gradientgoal + .2;
                    gradient2 = gradientgoal + .3;
                    gradient3 = 1;
                    break;
            }

            gradients = new double[]
                    {
                        gradient0,
                        gradient1,
                        gradient2,
                        gradient3
                    };

            return gradients;
        }

        protected double Radious
        {
            get => _toRadious;
            set
            {
                if (value != _toRadious)
                {
                    _fromRadious = _toRadious;
                    _toRadious = value;

                    try { jsTask.InvokeVoidAsync("AnimatePointer", gauge); }
                    catch (Exception e) { }
                }
            }
        }

        protected double[] DialColorOffsets { get; set; } = new double[4];

        protected double[] DialTicks { get; set; } = new double[11];

        protected override async void OnAfterRender(bool firstRender)
        {

            if (firstRender)
            {
                jsTask = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/PBVisualIndicators/js/DialGaugeAnimation.js");
                await SetupGauge();
            }
        }

        private async Task ValidateColors()
        {
            ColorValidationHelper ch = new(JsRuntime);
            bool isValidColors = await ch.ValidateColor(Task<string[]>.Factory.StartNew(() => Colors));
            if (!isValidColors)
            {
                Colors = new string[] { "Red", "Orange", "Yellow", "Lime" };
                await ColorsChanged.InvokeAsync(Colors);
            }
        }

        private async Task SetupGauge()
        {
            await SetDialTicks();
            await ValidateColors();

            DialColorOffsets = GradientScale();
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
                DialTicks[s] = Math.Round(i, 0);

                i += mi;

                s++;

            }

            return Task.CompletedTask;
        }

        private double ScaledValue(double actualmin, double actualmax, double value, double desiredmin = -90, double desiredmax = 90)
        {
            return desiredmin + (value - actualmin) * (desiredmax - desiredmin) / (actualmax - actualmin);
        }

        protected async Task Animate()
        {
            await jsTask.InvokeVoidAsync("AnimatePointer", gauge);
        }
    }
}
