using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Starlight.Misc.Profiling
{
    public class ProgressTracker
    {
        double _lastValue;

        double _value;

        public double Delta;

        public double TotalValue;

        public double Value
        {
            get => _value;
            set
            {
                _lastValue = _value;
                _value = Math.Max(Math.Min(value, TotalValue), 0); // Clamp value between 0 and TotalValue
                Delta = (_value - _lastValue) / TotalValue;
            }
        }

        public string Annotation = string.Empty;

        public int PercentComplete => (int)Math.Round(Value / TotalValue * 100);

        public bool Fufilled => PercentComplete == 100;

        public delegate void ProgressUpdatedCallback(ProgressTracker sender);

        public event ProgressUpdatedCallback ProgressUpdated;

        public ProgressTracker SubStep(int totalValue = 1)
        {
            var subTracker = new ProgressTracker();
            subTracker.Start(totalValue);
            subTracker.ProgressUpdated += sender =>
            {
                Value += sender.Delta;

                if (sender.Annotation is not null)
                    Annotation = sender.Annotation;
                
                ProgressUpdated?.Invoke(this);
            };

            return subTracker;
        }

        public void Step(string annotation = null)
        {
            Value++;

            if (annotation is not null)
                Annotation = annotation;

            ProgressUpdated?.Invoke(this);
        }

        public ProgressTracker Start(int totalValue = 1, string annotation = null)
        {
            Annotation = annotation;
            Value = 0;
            TotalValue = totalValue;
            return this;
        }

        public ProgressTracker()
        {
        }
    }
}
