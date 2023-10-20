using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using VRage.Library;
using VRage;
using VRage.Serialization;
using Sandbox.ModAPI;
using System.IO;
using Sandbox.Engine.Utils;
using System.Xml.Serialization;

namespace Faction_Territories.Config
{
	[Serializable]
	[ProtoContract]
	public class ServerConfig
	{
		[ProtoMember(1)]
		public SerializableDictionary<long, string> _AllianceNames;

		[ProtoMember(2)]
		public SerializableDictionary<long, ulong> _AllianceChannelIds;

		[ProtoMember(3)]
		public ulong GlobalDiscordChannelId;

		[XmlIgnore]
		[ProtoIgnore]
		public Dictionary<long, string> AllianceNames
		{
			get
			{
				if (_AllianceNames == null)
					_AllianceNames = new SerializableDictionary<long, string>( new Dictionary<long, string> { { 0, "None"} } );

				return _AllianceNames.Dictionary;
			}
			set
			{
				_AllianceNames.Dictionary = value;
			}
		}

		[XmlIgnore]
		[ProtoIgnore]
		public Dictionary<long, ulong> AllianceChannelIds
		{
			get
			{
				if (_AllianceChannelIds == null)
					_AllianceChannelIds = new SerializableDictionary<long, ulong>( new Dictionary<long, ulong> { { 0, 0} } );

				return _AllianceChannelIds.Dictionary;
			}
			set
			{
				_AllianceChannelIds.Dictionary = value;
			}
		}

		public ServerConfig() {}

		public static ServerConfig Load()
		{
			ServerConfig config = new ServerConfig();
			if (MyAPIGateway.Utilities.FileExistsInWorldStorage("config.xml", typeof(Session)))
			{
				try
				{
					using (TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("config.xml", typeof(ServerConfig)))
					{
						string data = reader.ReadToEnd();
						config = MyAPIGateway.Utilities.SerializeFromXML<ServerConfig>(data);
					}
				}
				catch { return new ServerConfig(); }
			}

			if (config == null)
				config = new ServerConfig();

			return config;
		}

		public void Save()
		{
			try
			{
				using (TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("config.xml", typeof(ServerConfig)))
				{
					string data = MyAPIGateway.Utilities.SerializeToXML<ServerConfig>(this);
					if (data != null)
						writer.Write(data);
				}
			}
			catch { return ; }
		}
	}
}
