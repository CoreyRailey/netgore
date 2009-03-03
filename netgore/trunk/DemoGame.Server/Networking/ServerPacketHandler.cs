﻿using System;
using NetGore.IO.Bits;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DemoGame.Extensions;
using log4net;
using MySql.Data.MySqlClient;
using NetGore;
using NetGore.Network;

namespace DemoGame.Server
{
    class ServerPacketHandler : IMessageProcessor, IGetTime
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly MessageProcessorManager _ppManager;
        readonly SayHandler _sayHandler;
        readonly Server _server;
        readonly ServerSockets _serverSockets;

        /// <summary>
        /// Gets the server that the data is coming from.
        /// </summary>
        public Server Server
        {
            get { return _server; }
        }

        /// <summary>
        /// Gets the Sql connection to use.
        /// </summary>
        public MySqlConnection SqlConn
        {
            get { return Server.DBController.Connection; }
        }

        /// <summary>
        /// Gets the World to use.
        /// </summary>
        public World World
        {
            get { return Server.World; }
        }

        public ServerPacketHandler(ServerSockets serverSockets, Server server)
        {
            if (serverSockets == null)
                throw new ArgumentNullException("serverSockets");
            if (server == null)
                throw new ArgumentNullException("server");

            _server = server;
            _serverSockets = serverSockets;
            _serverSockets.OnDisconnect += ServerSockets_OnDisconnect;
            _sayHandler = new SayHandler(server);

            _ppManager = new MessageProcessorManager(this);
        }

        [MessageHandler((byte)ClientPacketID.Attack)]
#pragma warning disable 168
        void RecvAttack(TCPSocket conn, BitStream r)
#pragma warning restore 168
        {
            User user;
            if (TryGetUser(conn, out user))
                user.Attack();
        }

        [MessageHandler((byte)ClientPacketID.DropInventoryItem)]
        void RecvDropInventoryItem(TCPSocket conn, BitStream r)
        {
            byte slot = r.ReadByte();

            User user;
            if (!TryGetUser(conn, out user))
                return;

            user.DropInventoryItem(slot);
        }

        [MessageHandler((byte)ClientPacketID.GetEquipmentItemInfo)]
        void RecvGetEquipmentItemInfo(TCPSocket conn, BitStream r)
        {
            EquipmentSlot slot = r.ReadEquipmentSlot();

            User user;
            if (TryGetUser(conn, out user))
                user.SendEquipmentItemStats(slot);
        }

        [MessageHandler((byte)ClientPacketID.GetInventoryItemInfo)]
        void RecvGetInventoryItemInfo(TCPSocket conn, BitStream r)
        {
            byte slot = r.ReadByte();

            User user;
            if (TryGetUser(conn, out user))
                user.SendInventoryItemStats(slot);
        }

        [MessageHandler((byte)ClientPacketID.Jump)]
#pragma warning disable 168
        void RecvJump(TCPSocket conn, BitStream r)
#pragma warning restore 168
        {
            User user;
            if (TryGetUser(conn, out user) && user.CanJump)
                user.Jump();
        }

        [MessageHandler((byte)ClientPacketID.Login)]
        void RecvLogin(TCPSocket conn, BitStream r)
        {
            string name = r.ReadString();
            string password = r.ReadString();

            if (conn == null)
            {
                Debug.Fail("conn is null.");
                log.Error("conn is null.");
                return;
            }

            if (!User.IsValidAccount(SqlConn, name, password))
            {
                if (log.IsInfoEnabled)
                    log.InfoFormat("Invalid login attempt: {0} / {1}", name, password);
                using (PacketWriter pw = ServerPacket.InvalidAccount())
                {
                    conn.Send(pw);
                }
                return;
            }

            using (PacketWriter pw = ServerPacket.Login())
            {
                conn.Send(pw);
            }

            new User(conn, World, name);
        }

        [MessageHandler((byte)ClientPacketID.MoveLeft)]
#pragma warning disable 168
        void RecvMoveLeft(TCPSocket conn, BitStream r)
#pragma warning restore 168
        {
            User user;
            if (TryGetUser(conn, out user) && !user.IsMovingLeft)
                user.MoveLeft();
        }

        [MessageHandler((byte)ClientPacketID.MoveRight)]
#pragma warning disable 168
        void RecvMoveRight(TCPSocket conn, BitStream r)
#pragma warning restore 168
        {
            User user;
            if (TryGetUser(conn, out user) && !user.IsMovingRight)
                user.MoveRight();
        }

        [MessageHandler((byte)ClientPacketID.MoveStop)]
#pragma warning disable 168
        void RecvMoveStop(TCPSocket conn, BitStream r)
#pragma warning restore 168
        {
            User user;
            if (TryGetUser(conn, out user) && user.IsMoving)
                user.StopMoving();
        }

        [MessageHandler((byte)ClientPacketID.PickupItem)]
        void RecvPickupItem(TCPSocket conn, BitStream r)
        {
            ushort mapItemIndex = r.ReadUShort();

            User user;
            Map map;
            if (!TryGetMap(conn, out user, out map))
                return;

            // TODO: Distance validation on item pickup

            ItemEntityBase item;
            if (map.TryGetItem(mapItemIndex, out item))
                item.Pickup(user);
        }

        [MessageHandler((byte)ClientPacketID.Ping)]
#pragma warning disable 168
        void RecvPing(TCPSocket conn, BitStream r)
#pragma warning restore 168
        {
            // Get the User
            User user;
            if (!TryGetUser(conn, out user))
                return;

            using (PacketWriter pw = ServerPacket.Ping())
            {
                user.Send(pw);
            }
        }

        [MessageHandler((byte)ClientPacketID.RaiseStat)]
        void RecvRaiseStat(TCPSocket conn, BitStream r)
        {
            StatType statType;

            // Get the StatType
            try
            {
                statType = r.ReadStatType();
            }
            catch (InvalidCastException)
            {
                const string errorMsg = "Received invaild StatType on connection `{0}`.";
                Debug.Fail(string.Format(errorMsg, conn));
                if (log.IsWarnEnabled)
                    log.WarnFormat(errorMsg, conn);
                return;
            }

            // Get the User
            User user;
            if (!TryGetUser(conn, out user))
                return;

            // Raise the user's stat
            user.RaiseStat(statType);
        }

        [MessageHandler((byte)ClientPacketID.Say)]
        void RecvSay(TCPSocket conn, BitStream r)
        {
            string text = r.ReadString(GameData.MaxClientSayLength);

            User user;
            if (!TryGetUser(conn, out user))
                return;

            _sayHandler.Process(text, user);
        }

        [MessageHandler((byte)ClientPacketID.UnequipItem)]
        void RecvUnequipItem(TCPSocket conn, BitStream r)
        {
            EquipmentSlot slot = r.ReadEquipmentSlot();

            User user;
            if (TryGetUser(conn, out user))
                user.Equipped.RemoveAt(slot);
        }

        [MessageHandler((byte)ClientPacketID.UseInventoryItem)]
        void RecvUseInventoryItem(TCPSocket conn, BitStream r)
        {
            byte slot = r.ReadByte();

            User user;
            if (!TryGetUser(conn, out user))
                return;

            user.UseInventoryItem(slot);
        }

        [MessageHandler((byte)ClientPacketID.UseWorld)]
#pragma warning disable 168
        void RecvUseWorld(TCPSocket conn, BitStream r)
#pragma warning restore 168
        {
            User user;
            Map map;
            if (!TryGetMap(conn, out user, out map))
                return;

            // Use the first IUseableEntity on the map at the user's position
            IUseableEntity useable = map.GetUseable(user.CB, user);
            if (useable != null)
                useable.Use(user);
        }

        /// <summary>
        /// A connection has been lost with a client.
        /// </summary>
        /// <param name="conn">Connection the user was using.</param>
        void ServerSockets_OnDisconnect(TCPSocket conn)
        {
            // The user attached to the connection may already be disposed, so if we fail to find the user,
            // just ignore the problem
            User user;
            if (TryGetUser(conn, out user, false)) 
                user.DelayedDispose();
        }

        static bool TryGetMap(Character user, out Map map)
        {
            // Check for a valid user
            if (user == null)
            {
                const string errmsg = "user is null.";
                if (log.IsErrorEnabled)
                    log.Error(errmsg);
                Debug.Fail(errmsg);
                map = null;
                return false;
            }

            // Get the map
            map = user.Map;
            if (map == null)
            {
                // Invalid map
                const string errorMsg = "Received UseWorld from user `{0}`, but their map is null.";
                Debug.Fail(string.Format(errorMsg, user));
                if (log.IsWarnEnabled)
                    log.WarnFormat(errorMsg, user);
                return false;
            }

            // Valid map
            return true;
        }

        bool TryGetMap(TCPSocket conn, out User user, out Map map)
        {
            if (!TryGetUser(conn, out user))
            {
                map = null;
                return false;
            }

            return TryGetMap(user, out map);
        }

        bool TryGetUser(TCPSocket conn, out User user, bool failRecover)
        {
            // Check for a valid connection
            if (conn == null)
            {
                Debug.Fail("conn is null.");
                if (log.IsWarnEnabled)
                    log.Warn("conn is null.");
                user = null;
                return false;
            }

            // Get the user
            user = World.GetUser(conn, failRecover);

            // Check for a valid user
            if (user == null)
            {
                if (failRecover)
                {
                    Debug.Fail("user is null.");
                    log.Error("user is null.");
                }
                return false;
            }

            return true;
        }

        bool TryGetUser(TCPSocket conn, out User user)
        {
            return TryGetUser(conn, out user, true);
        }

        #region IGetTime Members

        public int GetTime()
        {
            return Server.GetTime();
        }

        #endregion

        #region IMessageProcessor Members

        public void Process(SocketReceiveData rec)
        {
            _ppManager.Process(rec);
        }

        public void Process(TCPSocket socket, byte[] data)
        {
            _ppManager.Process(socket, data);
        }

        public void Process(IEnumerable<SocketReceiveData> recvData)
        {
            _ppManager.Process(recvData);
        }

        #endregion
    }
}