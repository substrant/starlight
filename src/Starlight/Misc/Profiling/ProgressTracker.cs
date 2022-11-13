using System;

namespace Starlight.Misc.Profiling;

/// <summary>
///     A class that tracks progress of a task.
/// </summary>
public class ProgressTracker
{
    double _lastValue;
    double _value;

    /// <summary>
    ///     A callback for when the progress is updated.
    /// </summary>
    /// <param name="sender">The progress tracker object.</param>
    public delegate void ProgressUpdatedCallback(ProgressTracker sender);

    /// <summary>
    ///     The event that is called when the progress is updated.
    /// </summary>
    public event ProgressUpdatedCallback ProgressUpdated;

    /// <summary>
    ///     The annotation of the currently running step.
    /// </summary>
    public string Annotation = string.Empty;

    /// <summary>
    ///     The delta between the last value and the current value.
    /// </summary>
    public double Delta;

    /// <summary>
    ///     The max amount of steps in the task.
    /// </summary>
    public double TotalValue;

    /// <summary>
    ///     The current step in the task.
    /// </summary>
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

    /// <summary>
    ///     A percentage value between 0 and 100 representing the progress.
    /// </summary>
    public int PercentComplete => (int)Math.Round(Value / TotalValue * 100);

    /// <summary>
    ///     A boolean value indicating if the task is complete.
    /// </summary>
    public bool Fufilled => PercentComplete == 100;

    /// <summary>
    ///     Step into another progress tracker.
    /// </summary>
    /// <param name="totalValue"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     Mark the current step as complete and move on.
    /// </summary>
    /// <param name="annotation">The annotation for the next step.</param>
    public void Step(string annotation = null)
    {
        Value++;

        if (annotation is not null)
            Annotation = annotation;

        ProgressUpdated?.Invoke(this);
    }

    /// <summary>
    ///     Initialize or reset the progress tracker.
    /// </summary>
    /// <param name="totalValue">The max amount of steps in the task.</param>
    /// <param name="annotation">The annotation of the first step.</param>
    /// <returns>A progress tracker refering to the same instance.</returns>
    public ProgressTracker Start(int totalValue = 1, string annotation = null)
    {
        Annotation = annotation;
        Value = 0;
        TotalValue = totalValue;
        return this;
    }
}