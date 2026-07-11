using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Medical.BodyBags;

[Serializable, NetSerializable]
public sealed partial class BodyBagStretcherDoAfterEvent : SimpleDoAfterEvent
{
}
