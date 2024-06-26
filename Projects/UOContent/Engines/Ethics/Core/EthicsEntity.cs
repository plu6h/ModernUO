using System;
using ModernUO.Serialization;

namespace Server.Ethics;

[SerializationGenerator(0)]
public partial class EthicsEntity : ISerializable
{
    public EthicsEntity()
    {
        Serial = EthicsSystem.NewProfile;
        EthicsSystem.Add(this);
    }

    public DateTime Created { get; set; } = Core.Now;

    public Serial Serial { get; }

    public bool Deleted { get; private set; }

    public void Delete()
    {
        Deleted = true;
        EthicsSystem.Remove(this);
    }
}
