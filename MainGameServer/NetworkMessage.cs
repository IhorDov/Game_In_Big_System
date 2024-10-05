using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeyAttribute = MessagePack.KeyAttribute;

namespace MainGameServer
{
    public enum MessageType { SnapShot, MovementUpdate, Join, JoinAnswer, UpdateTickRate }

    [MessagePackObject]
    public abstract class NetworkMessage
    {
        [IgnoreMember]
        public abstract MessageType MessageType { get; }
        [IgnoreMember]
        public byte GetMessageTypeAsByte
        {
            get { return (byte)MessageType; }
        }
    }

    public class SnapShot : NetworkMessage
    {
        [Key(0)]
        public int SnapSeqId;

        [Key(1)]
        public float playerPosX;

        [Key(2)]
        public float playerPosY;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.SnapShot;
    }

    public class MovementUpdate : NetworkMessage
    {
        [Key(0)]
        public bool Moveleft { get; set; }
        [Key(1)] public int SequenceNumber { get; set; }
        [IgnoreMember]
        public override MessageType MessageType => MessageType.MovementUpdate;
    }

    [MessagePackObject]
    public class JoinMessage : NetworkMessage
    {
        [IgnoreMember]
        public override MessageType MessageType => MessageType.Join;
    }

    [MessagePackObject]
    public class JoinAnswer : NetworkMessage
    {
        [Key(0)]
        public bool PlayerOwner;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.JoinAnswer;
    }
    [MessagePackObject]
    public class UpdateTickRate : NetworkMessage
    {
        [Key(0)]
        public int TickRate;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.UpdateTickRate;
    }
}
