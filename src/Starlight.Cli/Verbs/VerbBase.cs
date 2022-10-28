namespace Starlight.Cli.Verbs;

public abstract class VerbBase
{
    protected virtual int Init()
    {
        return 0;
    }

    protected virtual int InternalInvoke()
    {
        return 0;
    }

    public int Invoke()
    {
        var initRes = Init();
        return initRes != 0 ? initRes : InternalInvoke();
    }
}