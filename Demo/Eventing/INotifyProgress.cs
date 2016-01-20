namespace Eventing
{
    public delegate void ProgressEventHandler<T>(object sender, ProgressEventArgs<T> args);

    public interface INotifyProgress<T>
    {
        event ProgressEventHandler<T> CommandProgress;
    }
}