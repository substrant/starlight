namespace Starlight.Cli.Verbs
{
    public abstract class VerbBase
    {
        protected virtual int Init() => 0;

        protected virtual int InternalInvoke() => 0;

        public int Invoke()
        {
            var initRes = Init();
            return initRes != 0 ? initRes : InternalInvoke();
        }
    }
}
