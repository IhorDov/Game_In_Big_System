using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainGameServer
{
    internal class GameWorld
    {
        float ballMoveSpeed = 4;
        /// <summary>
        ///this is half window size from mono game. used to define the world in pixels...
        ///Should probably be in a different format and fitted the clients resolution
        /// </summary>
        float playerPosX = 100;
        float playerPosY = 100;
        int snapShotId = 0;
        public GameWorld()
        {
        }
        public void UpdatePlayerMovement(MovementUpdate movement)
        {
            if (movement.Moveleft)
            {
                playerPosX -= 10;
            }
            else
            {
                playerPosX += 10;
            }
            snapShotId = movement.SequenceNumber;
        }
        public SnapShot GetWorldStateSnapShot()
        {
            return new SnapShot() { playerPosY = playerPosY, playerPosX = playerPosX, SnapSeqId = snapShotId };
        }
    }
}
