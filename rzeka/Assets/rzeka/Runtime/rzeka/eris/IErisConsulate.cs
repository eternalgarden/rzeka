namespace Rzeka
{
    public interface IErisConsulate
    {
        void OpenConsole();
        void ReceiveSpellOccurence(SerializableSpellOccurence occurence);
        void ReceiveMatterOccurence(SerializableMatterOccurence occurence);
        void ReceiveMessage(SerializableMessageOccurence messageOccurence);
    }
}