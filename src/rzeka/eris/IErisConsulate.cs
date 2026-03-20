namespace Rzeka
{
    public interface IErisConsulate
    {
        void OpenConsole();
        void ReceiveSpellOccurence(ISerializableSpellOccurence occurence);
        void ReceiveMatterOccurence(ISerializableMatterOccurence occurence);
        void ReceiveMessage(SerializableMessageOccurence messageOccurence);
    }
}