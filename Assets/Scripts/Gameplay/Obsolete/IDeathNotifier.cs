public interface IDeathNotifier
{
    event System.Action<IDeathNotifier> OnDeath;
}