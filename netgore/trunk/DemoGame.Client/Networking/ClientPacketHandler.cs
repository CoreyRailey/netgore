using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using log4net;
using Microsoft.Xna.Framework.Graphics;
using NetGore;
using NetGore.Graphics.GUI;
using NetGore.IO;
using NetGore.Network;

#pragma warning disable 168
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace DemoGame.Client
{
    public class ClientPacketHandler : IMessageProcessor, IGetTime
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly GameMessages _gameMessages = new GameMessages();
        readonly GameplayScreen _gameplayScreen;
        readonly Stopwatch _pingWatch = new Stopwatch();
        readonly MessageProcessorManager _ppManager;
        readonly ISocketSender _socketSender;

        /// <summary>
        /// Notifies listeners when a successful login request has been made.
        /// </summary>
        public event SocketEventHandler OnLoginSuccessful;

        /// <summary>
        /// Notifies listeners when an unsuccessful login request has been made.
        /// </summary>
        public event SocketEventHandler<string> OnLoginUnsuccessful;

        /// <summary>
        /// Gets the GameplayScreen the ClientSockets are a part of
        /// </summary>
        public GameplayScreen GameplayScreen
        {
            get { return _gameplayScreen; }
        }

        /// <summary>
        /// Gets the ItemInfoTooltip from the GameplayScreen
        /// </summary>
        public ItemInfoTooltip ItemInfoTooltip
        {
            get { return GameplayScreen.ItemInfoTooltip; }
        }

        /// <summary>
        /// Gets the map used by the world (Parent.World.Map)
        /// </summary>
        public Map Map
        {
            get { return GameplayScreen.World.Map; }
            set { GameplayScreen.World.Map = value; }
        }

        /// <summary>
        /// Gets the user's character
        /// </summary>
        public Character User
        {
            get { return GameplayScreen.UserChar; }
        }

        public UserEquipped UserEquipped
        {
            get { return GameplayScreen.UserEquipped; }
        }

        /// <summary>
        /// Gets the user's inventory
        /// </summary>
        public Inventory UserInventory
        {
            get { return GameplayScreen.Inventory; }
        }

        /// <summary>
        /// Gets the user's CharacterStats
        /// </summary>
        public CharacterStats UserStats
        {
            get { return GameplayScreen.UserStats; }
        }

        /// <summary>
        /// Gets the world used by the game (Parent.World)
        /// </summary>
        public World World
        {
            get { return GameplayScreen.World; }
        }

        public ClientPacketHandler(ISocketSender socketSender, GameplayScreen gameplayScreen)
        {
            _socketSender = socketSender;
            _gameplayScreen = gameplayScreen;
            _ppManager = new MessageProcessorManager(this, GameData.ServerMessageIDBitLength);
        }

        static List<StyledText> CreateChatText(string name, string method, string message)
        {
            if (string.IsNullOrEmpty(method))
                return new List<StyledText> { new StyledText(name + ": ", Color.Green), new StyledText(message, Color.Black) };
            else
                return new List<StyledText>
                       { new StyledText(name + " " + method + ": ", Color.Green), new StyledText(message, Color.Black) };
        }

        public void Ping()
        {
            if (_pingWatch.IsRunning)
                return;

            _pingWatch.Reset();

            using (PacketWriter pw = ClientPacket.Ping())
            {
                _socketSender.Send(pw);
            }

            _pingWatch.Start();
        }

        [MessageHandler((byte)ServerPacketID.CharAttack)]
        void RecvCharAttack(IIPSocket conn, BitStream r)
        {
            MapEntityIndex mapCharIndex = r.ReadMapEntityIndex();

            Character chr = Map.GetDynamicEntity<Character>(mapCharIndex);
            if (chr == null)
                return;

            chr.Attack();
        }

        [MessageHandler((byte)ServerPacketID.CharDamage)]
        void RecvCharDamage(IIPSocket conn, BitStream r)
        {
            MapEntityIndex mapCharIndex = r.ReadMapEntityIndex();
            int damage = r.ReadInt();

            Character chr = Map.GetDynamicEntity<Character>(mapCharIndex);
            if (chr == null)
                return;

            GameplayScreen.DamageTextPool.Create(damage, chr, GetTime());
        }

        [MessageHandler((byte)ServerPacketID.Chat)]
        void RecvChat(IIPSocket conn, BitStream r)
        {
            string text = r.ReadString(GameData.MaxServerSayLength);
            GameplayScreen.AppendToChatOutput(text);
        }

        [MessageHandler((byte)ServerPacketID.ChatSay)]
        void RecvChatSay(IIPSocket conn, BitStream r)
        {
            string name = r.ReadString(GameData.MaxServerSayNameLength);
            ushort mapCharIndex = r.ReadUShort();
            string text = r.ReadString(GameData.MaxServerSayLength);

            // NOTE: Make use of the mapCharIndex for a chat bubble
            // TODO: Should use a GameMessage so we don't have the constant "says"
            var chatText = CreateChatText(name, "says", text);
            GameplayScreen.AppendToChatOutput(chatText);
        }

        [MessageHandler((byte)ServerPacketID.CreateDynamicEntity)]
        void RecvCreateDynamicEntity(IIPSocket conn, BitStream r)
        {
            MapEntityIndex mapEntityIndex = r.ReadMapEntityIndex();
            DynamicEntity dynamicEntity = DynamicEntityFactory.Read(r);
            Map.AddDynamicEntity(dynamicEntity, mapEntityIndex);

            Character character = dynamicEntity as Character;
            if (character != null)
            {
                // HACK: Having to call this .Initialize() and pass these parameters seems hacky
                character.Initialize(Map, GameplayScreen.SkeletonManager);
            }

            if (log.IsInfoEnabled)
                log.InfoFormat("Created DynamicEntity with index `{0}` of type `{1}`", dynamicEntity.MapEntityIndex,
                               dynamicEntity.GetType());
        }

        [MessageHandler((byte)ServerPacketID.LoginSuccessful)]
        void RecvLoginSuccessful(IIPSocket conn, BitStream r)
        {
            if (OnLoginSuccessful != null)
                OnLoginSuccessful(conn);
        }

        [MessageHandler((byte)ServerPacketID.LoginUnsuccessful)]
        void RecvLoginUnsuccessful(IIPSocket conn, BitStream r)
        {
            string message = r.ReadGameMessage(_gameMessages);

            if (OnLoginUnsuccessful != null)
                OnLoginUnsuccessful(conn, message);
        }

        [MessageHandler((byte)ServerPacketID.UseEntity)]
        void RecvUseEntity(IIPSocket conn, BitStream r)
        {
            MapEntityIndex usedEntityIndex = r.ReadMapEntityIndex();
            MapEntityIndex usedByIndex = r.ReadMapEntityIndex();

            // Grab the used DynamicEntity
            DynamicEntity usedEntity = Map.GetDynamicEntity(usedEntityIndex);
            if (usedEntity == null)
            {
                const string errmsg = "UseEntity received but usedEntityIndex `{0}` is not a valid DynamicEntity.";
                Debug.Fail(string.Format(errmsg, usedEntityIndex));
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, usedEntityIndex);
                return;
            }

            // Grab the one who used this DynamicEntity (we can still use it, we'll just pass null)
            DynamicEntity usedBy = Map.GetDynamicEntity(usedEntityIndex);
            if (usedBy == null)
            {
                const string errmsg = "UseEntity received but usedByIndex `{0}` is not a valid DynamicEntity.";
                Debug.Fail(string.Format(errmsg, usedEntityIndex));
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, usedEntityIndex);
            }

            // Ensure the used DynamicEntity is even usable
            IUsableEntity asUsable = usedEntity as IUsableEntity;
            if (asUsable == null)
            {
                const string errmsg = "UseEntity received but usedByIndex `{0}` refers to DynamicEntity `{1}` which does " +
                    "not implement IUsableEntity.";
                Debug.Fail(string.Format(errmsg, usedEntityIndex, usedEntity));
                if (log.IsErrorEnabled)
                    log.WarnFormat(errmsg, usedEntityIndex, usedEntity);
                return;
            }

            // Use it
            asUsable.Use(usedBy);
        }

        [MessageHandler((byte)ServerPacketID.NotifyExpCash)]
        void RecvNotifyExpCash(IIPSocket conn, BitStream r)
        {
            int exp = r.ReadInt();
            int cash = r.ReadInt();

            Character userChar = World.UserChar;
            if (userChar == null)
            {
                Debug.Fail("UserChar is null.");
                return;
            }

            string msg = string.Format("Got {0} exp and {1} cash", exp, cash);
            _gameplayScreen.InfoBox.Add(msg);
        }

        [MessageHandler((byte)ServerPacketID.NotifyLevel)]
        void RecvNotifyLevel(IIPSocket conn, BitStream r)
        {
            MapEntityIndex mapCharIndex = r.ReadMapEntityIndex();

            Character chr = Map.GetDynamicEntity<Character>(mapCharIndex);
            if (chr == null)
                return;

            if (chr == World.UserChar)
                _gameplayScreen.InfoBox.Add("You have leveled up!");
        }

        [MessageHandler((byte)ServerPacketID.NotifyGetItem)]
        void RecvNotifyPickup(IIPSocket conn, BitStream r)
        {
            string name = r.ReadString();
            byte amount = r.ReadByte();

            string msg;
            if (amount > 1)
                msg = string.Format("You got {0} {1}s", amount, name);
            else
                msg = string.Format("You got a {0}", name);

            _gameplayScreen.InfoBox.Add(msg);
        }

        [MessageHandler((byte)ServerPacketID.Ping)]
        void RecvPing(IIPSocket conn, BitStream r)
        {
            _pingWatch.Stop();
        }

        [MessageHandler((byte)ServerPacketID.RemoveDynamicEntity)]
        void RecvRemoveDynamicEntity(IIPSocket conn, BitStream r)
        {
            MapEntityIndex mapEntityIndex = r.ReadMapEntityIndex();
            DynamicEntity dynamicEntity = Map.GetDynamicEntity(mapEntityIndex);

            if (dynamicEntity != null)
            {
                Map.RemoveEntity(dynamicEntity);
                if (log.IsInfoEnabled)
                    log.InfoFormat("Removed DynamicEntity with index `{0}`", mapEntityIndex);
            }
            else
            {
                const string errmsg = "Could not remove DynamicEntity with index `{0}` - no DynamicEntity found.";
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, mapEntityIndex);
                Debug.Fail(string.Format(errmsg, mapEntityIndex));
            }
        }

        [MessageHandler((byte)ServerPacketID.SendItemInfo)]
        void RecvSendItemInfo(IIPSocket conn, BitStream r)
        {
            string name = r.ReadString();
            string desc = r.ReadString();
            int value = r.ReadInt();

            ItemInfo itemInfo = ItemInfoTooltip.ItemInfo;

            itemInfo.SetItemInfo(name, desc, value);
            r.ReadStatCollection(itemInfo.Stats);

            itemInfo.SetAsUpdated();
        }

        [MessageHandler((byte)ServerPacketID.SendMessage)]
        void RecvSendMessage(IIPSocket conn, BitStream r)
        {
            string message = r.ReadGameMessage(_gameMessages);

            if (string.IsNullOrEmpty(message))
            {
                const string errmsg = "Received empty or null GameMessage.";
                if (log.IsErrorEnabled)
                    log.Error(errmsg);
                return;
            }

            GameplayScreen.AppendToChatOutput(message, Color.Black);
        }

        [MessageHandler((byte)ServerPacketID.SetInventorySlot)]
        void RecvSetInventorySlot(IIPSocket conn, BitStream r)
        {
            InventorySlot slot = r.ReadInventorySlot();
            ushort graphic = r.ReadUShort();
            byte amount = r.ReadByte();

            UserInventory.Update(slot, graphic, amount, GetTime());
        }

        [MessageHandler((byte)ServerPacketID.SetMap)]
        void RecvSetMap(IIPSocket conn, BitStream r)
        {
            MapIndex mapIndex = r.ReadMapIndex();

            // Create the new map
            Map newMap = new Map(mapIndex, World, GameplayScreen.ScreenManager.GraphicsDevice);
            newMap.Load(ContentPaths.Build, false);

            // Change maps
            World.SetMap(newMap);

            // Unload all map content from the previous map and from the new map loading
            GameplayScreen.ScreenManager.MapContent.Unload();
        }

        [MessageHandler((byte)ServerPacketID.SetUserChar)]
        void RecvSetUserChar(IIPSocket conn, BitStream r)
        {
            MapEntityIndex mapCharIndex = r.ReadMapEntityIndex();
            World.UserCharIndex = mapCharIndex;
        }

        [MessageHandler((byte)ServerPacketID.UpdateEquipmentSlot)]
        void RecvUpdateEquipmentSlot(IIPSocket conn, BitStream r)
        {
            EquipmentSlot slot = r.ReadEquipmentSlot();
            bool hasValue = r.ReadBool();

            if (hasValue)
            {
                ushort graphic = r.ReadUShort();
                UserEquipped.SetSlot(slot, graphic);
            }
            else
                UserEquipped.ClearSlot(slot);
        }

        [MessageHandler((byte)ServerPacketID.UpdateStat)]
        void RecvUpdateStat(IIPSocket conn, BitStream r)
        {
            r.ReadStat(UserStats);
        }

        [MessageHandler((byte)ServerPacketID.UpdateVelocityAndPosition)]
        void RecvUpdateVelocityAndPosition(IIPSocket conn, BitStream r)
        {
            MapEntityIndex mapEntityIndex = r.ReadMapEntityIndex();
            DynamicEntity dynamicEntity;

            // Grab the DynamicEntity
            try
            {
                dynamicEntity = Map.GetDynamicEntity<DynamicEntity>(mapEntityIndex);
            }
            catch (Exception)
            {
                // Ignore errors about finding the DynamicEntity
                dynamicEntity = null;
            }

            // Deserialize
            IValueReader valueReader = new BitStreamValueReader(r);
            if (dynamicEntity != null)
            {
                // Read the value into the DynamicEntity
                dynamicEntity.DeserializePositionAndVelocity(valueReader);
            }
            else
            {
                // Just flush the values from the reader
                DynamicEntity.FlushPositionAndVelocity(valueReader);
            }
        }

        #region IGetTime Members

        /// <summary>
        /// Gets the current time.
        /// </summary>
        /// <returns>Current time.</returns>
        public int GetTime()
        {
            return GameplayScreen.GetTime();
        }

        #endregion

        #region IMessageProcessor Members

        /// <summary>
        /// Handles received data and forwards it to the corresponding MessageProcessors.
        /// </summary>
        /// <param name="rec">SocketReceiveData to process.</param>
        public void Process(SocketReceiveData rec)
        {
            _ppManager.Process(rec);
        }

        /// <summary>
        /// Handles received data and forwards it to the corresponding MessageProcessors.
        /// </summary>
        /// <param name="socket">Socket the data came from.</param>
        /// <param name="data">Data to process.</param>
        public void Process(IIPSocket socket, byte[] data)
        {
            _ppManager.Process(socket, data);
        }

        /// <summary>
        /// Handles a list of received data and forwards it to the corresponding MessageProcessors.
        /// </summary>
        /// <param name="recvData">List of SocketReceiveData to process.</param>
        public void Process(IEnumerable<SocketReceiveData> recvData)
        {
            _ppManager.Process(recvData);
        }

        #endregion
    }
}